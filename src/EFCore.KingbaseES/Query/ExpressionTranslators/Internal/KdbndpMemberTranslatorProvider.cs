using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
/// A composite member translator that dispatches to multiple specialized member translators specific to Kdbndp.
/// </summary>
public class KdbndpMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    public virtual KdbndpJsonPocoTranslator JsonPocoTranslator { get; }

    public KdbndpMemberTranslatorProvider(
        RelationalMemberTranslatorProviderDependencies dependencies,
        IModel model,
        IRelationalTypeMappingSource typeMappingSource,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
        : base(dependencies)
    {
        var sqlExpressionFactory = (KdbndpSqlExpressionFactory)dependencies.SqlExpressionFactory;
        JsonPocoTranslator = new KdbndpJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model);

        AddTranslators(
            new IMemberTranslator[] {
                new KdbndpArrayTranslator(sqlExpressionFactory, JsonPocoTranslator, KdbndpSingletonOptions.UseRedshift),
                new KdbndpBigIntegerMemberTranslator(sqlExpressionFactory),
                new KdbndpDateTimeMemberTranslator(typeMappingSource, sqlExpressionFactory),
                new KdbndpJsonDomTranslator(typeMappingSource, sqlExpressionFactory, model),
                new KdbndpLTreeTranslator(typeMappingSource, sqlExpressionFactory, model),
                JsonPocoTranslator,
                new KdbndpRangeTranslator(typeMappingSource, sqlExpressionFactory, model, KdbndpSingletonOptions),
                new KdbndpStringMemberTranslator(sqlExpressionFactory),
                new KdbndpTimeSpanMemberTranslator(sqlExpressionFactory),
            });
    }
}
