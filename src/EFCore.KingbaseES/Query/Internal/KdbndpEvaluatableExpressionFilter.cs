using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
{
    private static readonly MethodInfo TsQueryParse =
        typeof(KdbndpTsQuery).GetRuntimeMethod(nameof(KdbndpTsQuery.Parse), new[] { typeof(string) })!;

    private static readonly MethodInfo TsVectorParse =
        typeof(KdbndpTsVector).GetRuntimeMethod(nameof(KdbndpTsVector.Parse), new[] { typeof(string) })!;

    public KdbndpEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        switch (expression)
        {
            case MethodCallExpression methodCallExpression:
                var declaringType = methodCallExpression.Method.DeclaringType;

                if (methodCallExpression.Method == TsQueryParse
                    || methodCallExpression.Method == TsVectorParse
                    || declaringType == typeof(KdbndpDbFunctionsExtensions)
                    || declaringType == typeof(KdbndpFullTextSearchDbFunctionsExtensions)
                    || declaringType == typeof(KdbndpFullTextSearchLinqExtensions)
                    || declaringType == typeof(KdbndpNetworkDbFunctionsExtensions)
                    || declaringType == typeof(KdbndpJsonDbFunctionsExtensions)
                    || declaringType == typeof(KdbndpRangeDbFunctionsExtensions))
                {
                    return false;
                }

                break;

            case MemberExpression memberExpression:
                // We support translating certain NodaTime patterns which accept a time zone as a parameter,
                // e.g. Instant.InZone(timezone), as long as the timezone is expressed as an access on DateTimeZoneProviders.Tzdb.
                // Prevent this from being evaluated locally and so parameterized, so we can access the access tree on
                // DateTimeZoneProviders and extract the constant (see KdbndpNodaTimeMethodCallTranslatorPlugin)
                if (memberExpression.Member.DeclaringType?.FullName == "NodaTime.DateTimeZoneProviders")
                {
                    return false;
                }

                break;
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}
