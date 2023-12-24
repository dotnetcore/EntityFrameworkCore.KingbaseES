using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions.Internal;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpMiscAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo StringJoin
        = typeof(string).GetRuntimeMethod(nameof(string.Join), new[] { typeof(string), typeof(IEnumerable<string>) })!;

    private static readonly MethodInfo StringConcat
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(IEnumerable<string>) })!;

    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly IModel _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpMiscAggregateMethodTranslator(
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
        _model = model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Docs: https://www.KingbaseES.org/docs/current/functions-aggregate.html

        if (source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        if (method == StringJoin || method == StringConcat)
        {
            // string_agg filters out nulls, but string.Join treats them as empty strings; coalesce unless we know we're aggregating over
            // a non-nullable column.
            if (sqlExpression is not ColumnExpression { IsNullable: false })
            {
                sqlExpression = _sqlExpressionFactory.Coalesce(
                    sqlExpression,
                    _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
            }

            // string_agg returns null when there are no rows (or non-null values), but string.Join returns an empty string.
            return _sqlExpressionFactory.Coalesce(
                _sqlExpressionFactory.AggregateFunction(
                    "string_agg",
                    new[]
                    {
                        sqlExpression,
                        method == StringJoin ? arguments[0] : _sqlExpressionFactory.Constant(string.Empty, typeof(string))
                    },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    typeof(string),
                    _typeMappingSource.FindMapping("text")), // Note that string_agg returns text even if its inputs are varchar(x)
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
        }

        if (method.DeclaringType == typeof(KdbndpAggregateDbFunctionsExtensions))
        {
            switch (method.Name)
            {
                case nameof(KdbndpAggregateDbFunctionsExtensions.ArrayAgg):
                    return _sqlExpressionFactory.AggregateFunction(
                        "array_agg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: method.ReturnType,
                        typeMapping: sqlExpression.TypeMapping is null
                            ? null
                            : _typeMappingSource.FindMapping(method.ReturnType, _model, sqlExpression.TypeMapping));

                case nameof(KdbndpAggregateDbFunctionsExtensions.JsonAgg):
                    return _sqlExpressionFactory.AggregateFunction(
                        "json_agg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, "json"));

                case nameof(KdbndpAggregateDbFunctionsExtensions.JsonbAgg):
                    return _sqlExpressionFactory.AggregateFunction(
                        "jsonb_agg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, "jsonb"));

                case nameof(KdbndpAggregateDbFunctionsExtensions.Sum):
                    return _sqlExpressionFactory.AggregateFunction(
                        "sum",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: sqlExpression.Type,
                        sqlExpression.TypeMapping);

                case nameof(KdbndpAggregateDbFunctionsExtensions.Average):
                    return _sqlExpressionFactory.AggregateFunction(
                        "avg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: sqlExpression.Type,
                        sqlExpression.TypeMapping);

                case nameof(KdbndpAggregateDbFunctionsExtensions.JsonbObjectAgg):
                case nameof(KdbndpAggregateDbFunctionsExtensions.JsonObjectAgg):
                    var isJsonb = method.Name == nameof(KdbndpAggregateDbFunctionsExtensions.JsonbObjectAgg);

                    // These methods accept two enumerable (column) arguments; this is represented in LINQ as a projection from the grouping
                    // to a tuple of the two columns. Since we generally translate tuples to PostgresRowValueExpression, we take it apart
                    // here.
                    if (source.Selector is not PgRowValueExpression rowValueExpression)
                    {
                        return null;
                    }

                    var (keys, values) = (rowValueExpression.Values[0], rowValueExpression.Values[1]);

                    return _sqlExpressionFactory.AggregateFunction(
                        isJsonb ? "jsonb_object_agg" : "json_object_agg",
                        new[] { keys, values },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[2],
                        returnType: method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, isJsonb ? "jsonb" : "json"));
            }
        }

        if (method.DeclaringType == typeof(KdbndpRangeDbFunctionsExtensions))
        {
            switch (method.Name)
            {
                case nameof(KdbndpRangeDbFunctionsExtensions.RangeAgg):
                    var arrayClrType = sqlExpression.Type.MakeArrayType();

                    return _sqlExpressionFactory.AggregateFunction(
                        "range_agg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: arrayClrType,
                        _typeMappingSource.FindMapping(arrayClrType));

                case nameof(KdbndpRangeDbFunctionsExtensions.RangeIntersectAgg):
                    return _sqlExpressionFactory.AggregateFunction(
                        "range_intersect_agg",
                        new[] { sqlExpression },
                        source,
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        returnType: sqlExpression.Type,
                        sqlExpression.TypeMapping);
            }
        }

        return null;
    }
}
