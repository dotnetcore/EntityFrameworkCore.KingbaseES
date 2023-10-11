using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTsRankingNormalizationTypeMapping : IntTypeMapping
{
    public KdbndpTsRankingNormalizationTypeMapping() : base(new RelationalTypeMappingParameters(
        new CoreTypeMappingParameters(typeof(KdbndpTsRankingNormalization), new EnumToNumberConverter<KdbndpTsRankingNormalization, int>()),
        "integer")) {}

    protected KdbndpTsRankingNormalizationTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTsRankingNormalizationTypeMapping(parameters);
}
