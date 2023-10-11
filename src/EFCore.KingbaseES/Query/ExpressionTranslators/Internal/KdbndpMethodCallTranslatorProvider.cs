using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

public class KdbndpMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    public virtual KdbndpLTreeTranslator LTreeTranslator { get; }

    public KdbndpMethodCallTranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies,
        IModel model,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
        : base(dependencies)
    {
        var sqlExpressionFactory = (KdbndpSqlExpressionFactory)dependencies.SqlExpressionFactory;
        var typeMappingSource = (KdbndpTypeMappingSource)dependencies.RelationalTypeMappingSource;
        var jsonTranslator = new KdbndpJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model);
        LTreeTranslator = new KdbndpLTreeTranslator(typeMappingSource, sqlExpressionFactory, model);

        AddTranslators(new IMethodCallTranslator[]
        {
            new KdbndpArrayTranslator(sqlExpressionFactory, jsonTranslator, KdbndpSingletonOptions.UseRedshift),
            new KdbndpByteArrayMethodTranslator(sqlExpressionFactory),
            new KdbndpConvertTranslator(sqlExpressionFactory),
            new KdbndpDateTimeMethodTranslator(typeMappingSource, sqlExpressionFactory),
            new KdbndpFullTextSearchMethodTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpFuzzyStringMatchMethodTranslator(sqlExpressionFactory),
            new KdbndpJsonDomTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpJsonDbFunctionsTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpLikeTranslator(sqlExpressionFactory),
            LTreeTranslator,
            new KdbndpMathTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpNetworkTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpNewGuidTranslator(sqlExpressionFactory, KdbndpSingletonOptions),
            new KdbndpObjectToStringTranslator(typeMappingSource, sqlExpressionFactory),
            new KdbndpRandomTranslator(sqlExpressionFactory),
            new KdbndpRangeTranslator(typeMappingSource, sqlExpressionFactory, model, KdbndpSingletonOptions),
            new KdbndpRegexIsMatchTranslator(sqlExpressionFactory),
            new KdbndpStringMethodTranslator(typeMappingSource, sqlExpressionFactory, model),
            new KdbndpTrigramsMethodTranslator(typeMappingSource, sqlExpressionFactory, model),
        });
    }
}
