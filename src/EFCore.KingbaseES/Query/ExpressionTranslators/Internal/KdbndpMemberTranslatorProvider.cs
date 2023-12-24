using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     A composite member translator that dispatches to multiple specialized member translators specific to Kdbndp.
/// </summary>
public class KdbndpMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual KdbndpJsonPocoTranslator JsonPocoTranslator { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpMemberTranslatorProvider(
        RelationalMemberTranslatorProviderDependencies dependencies,
        IModel model,
        IRelationalTypeMappingSource typeMappingSource,
        IDbContextOptions contextOptions)
        : base(dependencies)
    {
        var KdbndpOptions = contextOptions.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();
        var supportsMultiranges = KdbndpOptions.PostgresVersion.AtLeast(14);

        var sqlExpressionFactory = (KdbndpSqlExpressionFactory)dependencies.SqlExpressionFactory;
        JsonPocoTranslator = new KdbndpJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model);

        AddTranslators(
            new IMemberTranslator[]
            {
                new KdbndpBigIntegerMemberTranslator(sqlExpressionFactory),
                new KdbndpDateTimeMemberTranslator(typeMappingSource, sqlExpressionFactory),
                new KdbndpJsonDomTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpLTreeTranslator(typeMappingSource, sqlExpressionFactory, model),
                JsonPocoTranslator,
                new KdbndpRangeTranslator(typeMappingSource, sqlExpressionFactory, model, supportsMultiranges),
                new KdbndpStringMemberTranslator(sqlExpressionFactory),
                new KdbndpTimeSpanMemberTranslator(sqlExpressionFactory),
            });
    }
}
