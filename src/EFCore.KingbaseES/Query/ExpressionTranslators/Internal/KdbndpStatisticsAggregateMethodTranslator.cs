using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions.Internal;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpStatisticsAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _doubleTypeMapping, _longTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpStatisticsAggregateMethodTranslator(
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double))!;
        _longTypeMapping = typeMappingSource.FindMapping(typeof(long))!;
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
        // Docs: https://www.KingbaseES.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-STATISTICS-TABLE

        if (method.DeclaringType != typeof(KdbndpAggregateDbFunctionsExtensions)
            || source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        // These four functions are simple and take a single enumerable argument
        var functionName = method.Name switch
        {
            nameof(KdbndpAggregateDbFunctionsExtensions.StandardDeviationSample) => "stddev_samp",
            nameof(KdbndpAggregateDbFunctionsExtensions.StandardDeviationPopulation) => "stddev_pop",
            nameof(KdbndpAggregateDbFunctionsExtensions.VarianceSample) => "var_samp",
            nameof(KdbndpAggregateDbFunctionsExtensions.VariancePopulation) => "var_pop",
            _ => null
        };

        if (functionName is not null)
        {
            return _sqlExpressionFactory.AggregateFunction(
                functionName,
                new[] { sqlExpression },
                source,
                nullable: true,
                argumentsPropagateNullability: FalseArrays[1],
                typeof(double),
                _doubleTypeMapping);
        }

        functionName = method.Name switch
        {
            nameof(KdbndpAggregateDbFunctionsExtensions.Correlation) => "corr",
            nameof(KdbndpAggregateDbFunctionsExtensions.CovariancePopulation) => "covar_pop",
            nameof(KdbndpAggregateDbFunctionsExtensions.CovarianceSample) => "covar_samp",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrAverageX) => "regr_avgx",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrAverageY) => "regr_avgy",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrCount) => "regr_count",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrIntercept) => "regr_intercept",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrR2) => "regr_r2",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrSlope) => "regr_slope",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrSXX) => "regr_sxx",
            nameof(KdbndpAggregateDbFunctionsExtensions.RegrSXY) => "regr_sxy",
            _ => null
        };

        if (functionName is not null)
        {
            // These methods accept two enumerable (column) arguments; this is represented in LINQ as a projection from the grouping
            // to a tuple of the two columns. Since we generally translate tuples to PostgresRowValueExpression, we take it apart here.
            if (source.Selector is not PgRowValueExpression rowValueExpression)
            {
                return null;
            }

            var (y, x) = (rowValueExpression.Values[0], rowValueExpression.Values[1]);

            return method.Name == nameof(KdbndpAggregateDbFunctionsExtensions.RegrCount)
                ? _sqlExpressionFactory.AggregateFunction(
                    functionName,
                    new[] { y, x },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[2],
                    typeof(long),
                    _longTypeMapping)
                : _sqlExpressionFactory.AggregateFunction(
                    functionName,
                    new[] { y, x },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[2],
                    typeof(double),
                    _doubleTypeMapping);
        }

        return null;
    }
}
