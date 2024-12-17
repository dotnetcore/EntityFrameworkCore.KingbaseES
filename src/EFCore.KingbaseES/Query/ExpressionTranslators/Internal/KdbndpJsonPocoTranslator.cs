using System.Text.Json.Serialization;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpJsonPocoTranslator : IMemberTranslator, IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _stringTypeMapping;
    private readonly IModel _model;

    private static readonly MethodInfo Enumerable_AnyWithoutPredicate =
        typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpJsonPocoTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _model = model;
        _stringTypeMapping = typeMappingSource.FindMapping(typeof(string), model)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Predicate-less Any - translate to a simple length check.
        if (method.IsClosedFormOf(Enumerable_AnyWithoutPredicate)
            && TranslateArrayLength(arguments[0]) is SqlExpression arrayLengthTranslation)
        {
            return _sqlExpressionFactory.GreaterThan(arrayLengthTranslation, _sqlExpressionFactory.Constant(0));
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance?.TypeMapping is not KdbndpJsonTypeMapping && instance is not PgJsonTraversalExpression)
        {
            return null;
        }

        if (member is { Name: "Count", DeclaringType.IsGenericType: true }
            && member.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return TranslateArrayLength(instance);
        }

        return TranslateMemberAccess(
            instance,
            _sqlExpressionFactory.Constant(member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? member.Name),
            returnType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateMemberAccess(SqlExpression instance, SqlExpression member, Type returnType)
    {
        return instance switch
        {
            // The first time we see a JSON traversal it's on a column - create a JsonTraversalExpression.
            // Traversals on top of that get appended into the same expression.
            ColumnExpression { TypeMapping: KdbndpJsonTypeMapping } columnExpression
                => ConvertFromText(
                    _sqlExpressionFactory.JsonTraversal(
                        columnExpression,
                        [member],
                        returnsText: true,
                        typeof(string),
                        _stringTypeMapping),
                    returnType),

            PgJsonTraversalExpression prevPathTraversal
                => ConvertFromText(
                    prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(member)),
                    returnType),

            _ => null
        };

        // The KingbaseES traversal operator always returns text.
        // If the type returned is a scalar (int, bool, etc.), we need to apply a conversion from string.
        SqlExpression ConvertFromText(SqlExpression expression, Type returnType)
            => _typeMappingSource.FindMapping(returnType.UnwrapNullableType(), _model) switch
            {
                // Type mapping not found - this isn't a scalar
                null => expression,

                // Arrays are dealt with as JSON arrays, not array scalars
                KdbndpArrayTypeMapping => expression,

                // Text types don't a a conversion to string
                { StoreTypeNameBase: "text" or "varchar" or "char" } => expression,

                // For any other type mapping, this is a scalar; apply a conversion to the type from string.
                var mapping => _sqlExpressionFactory.Convert(expression, returnType, mapping)
            };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateArrayLength(SqlExpression expression)
    {
        switch (expression)
        {
            case ColumnExpression { TypeMapping: KdbndpJsonTypeMapping mapping }:
                return _sqlExpressionFactory.Function(
                    mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    [expression],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int));

            case PgJsonTraversalExpression traversal:
                // The traversal expression has ReturnsText=true (e.g. ->> not ->), so we recreate it to return
                // the JSON object instead.
                var lastPathComponent = traversal.Path.Last();
                var newTraversal = new PgJsonTraversalExpression(
                    traversal.Expression, traversal.Path,
                    returnsText: false,
                    lastPathComponent.Type,
                    _typeMappingSource.FindMapping(lastPathComponent.Type, _model));

                var jsonMapping = (KdbndpJsonTypeMapping)traversal.Expression.TypeMapping!;
                return _sqlExpressionFactory.Function(
                    jsonMapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    [newTraversal],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int));

            default:
                return null;
        }
    }
}
