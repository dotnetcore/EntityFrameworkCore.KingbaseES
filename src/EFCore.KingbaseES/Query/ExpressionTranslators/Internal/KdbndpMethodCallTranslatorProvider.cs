using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual KdbndpLTreeTranslator LTreeTranslator { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpMethodCallTranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies,
        IModel model,
        IDbContextOptions contextOptions)
        : base(dependencies)
    {
        var KdbndpOptions = contextOptions.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();
        var supportsMultiranges = KdbndpOptions.PostgresVersion.AtLeast(14);

        var sqlExpressionFactory = (KdbndpSqlExpressionFactory)dependencies.SqlExpressionFactory;
        var typeMappingSource = (KdbndpTypeMappingSource)dependencies.RelationalTypeMappingSource;
        var jsonTranslator = new KdbndpJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model);
        LTreeTranslator = new KdbndpLTreeTranslator(typeMappingSource, sqlExpressionFactory, model);

        AddTranslators(
            new IMethodCallTranslator[]
            {
                new KdbndpArrayMethodTranslator(sqlExpressionFactory, jsonTranslator),
                new KdbndpByteArrayMethodTranslator(sqlExpressionFactory),
                new KdbndpConvertTranslator(sqlExpressionFactory),
                new KdbndpDateTimeMethodTranslator(typeMappingSource, sqlExpressionFactory),
                new KdbndpFullTextSearchMethodTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpFuzzyStringMatchMethodTranslator(sqlExpressionFactory),
                new KdbndpJsonDomTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpJsonDbFunctionsTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpLikeTranslator(sqlExpressionFactory),
                LTreeTranslator,
                new KdbndpMathTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpNetworkTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpNewGuidTranslator(sqlExpressionFactory, KdbndpOptions.PostgresVersion),
                new KdbndpObjectToStringTranslator(typeMappingSource, sqlExpressionFactory),
                new KdbndpRandomTranslator(sqlExpressionFactory),
                new KdbndpRangeTranslator(typeMappingSource, sqlExpressionFactory, model, supportsMultiranges),
                new KdbndpRegexIsMatchTranslator(sqlExpressionFactory),
                new KdbndpRowValueTranslator(sqlExpressionFactory),
                new KdbndpStringMethodTranslator(typeMappingSource, sqlExpressionFactory),
                new KdbndpTrigramsMethodTranslator(typeMappingSource, sqlExpressionFactory, model),
            });
    }
}
