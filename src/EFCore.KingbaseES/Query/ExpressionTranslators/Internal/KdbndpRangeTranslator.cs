using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpRangeTranslator : IMethodCallTranslator, IMemberTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly IModel _model;
    private readonly bool _supportsMultiranges;

    private static readonly MethodInfo EnumerableAnyWithoutPredicate =
        typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpRangeTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory KdbndpSqlExpressionFactory,
        IModel model,
        bool supportsMultiranges)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = KdbndpSqlExpressionFactory;
        _model = model;
        _supportsMultiranges = supportsMultiranges;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Any() over multirange -> NOT isempty(). KdbndpRange<T> has IsEmpty which is translated below.
        if (_supportsMultiranges
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableAnyWithoutPredicate)
        {
            return _sqlExpressionFactory.Not(
                _sqlExpressionFactory.Function(
                    "isempty",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)));
        }

        if (method.DeclaringType != typeof(KdbndpRangeDbFunctionsExtensions)
            && (method.DeclaringType != typeof(KdbndpMultirangeDbFunctionsExtensions) || !_supportsMultiranges))
        {
            return null;
        }

        if (method.Name == nameof(KdbndpRangeDbFunctionsExtensions.Merge))
        {
            if (method.DeclaringType == typeof(KdbndpRangeDbFunctionsExtensions))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    inferredMapping);
            }

            if (method.DeclaringType == typeof(KdbndpMultirangeDbFunctionsExtensions))
            {
                return null;
            }
        }

        return method.Name switch
        {
            nameof(KdbndpRangeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsStrictlyLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeIsStrictlyLeftOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsStrictlyRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeIsStrictlyRightOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.DoesNotExtendRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeDoesNotExtendRightOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.DoesNotExtendLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeDoesNotExtendLeftOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsAdjacentTo)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeIsAdjacentTo, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeUnion, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeIntersect, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Except)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeExcept, arguments[0], arguments[1]),

            _ => null
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var type = member.DeclaringType;
        if (type is null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(KdbndpRange<>))
        {
            return null;
        }

        if (member.Name is nameof(KdbndpRange<int>.LowerBound) or nameof(KdbndpRange<int>.UpperBound))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(KdbndpRange<int>.IsEmpty) => SingleArgBoolFunction("isempty", instance!),
            nameof(KdbndpRange<int>.LowerBoundIsInclusive) => SingleArgBoolFunction("lower_inc", instance!),
            nameof(KdbndpRange<int>.UpperBoundIsInclusive) => SingleArgBoolFunction("upper_inc", instance!),
            nameof(KdbndpRange<int>.LowerBoundInfinite) => SingleArgBoolFunction("lower_inf", instance!),
            nameof(KdbndpRange<int>.UpperBoundInfinite) => SingleArgBoolFunction("upper_inf", instance!),

            _ => null
        };

        SqlFunctionExpression SingleArgBoolFunction(string name, SqlExpression argument)
            => _sqlExpressionFactory.Function(
                name,
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(bool));
    }
}
