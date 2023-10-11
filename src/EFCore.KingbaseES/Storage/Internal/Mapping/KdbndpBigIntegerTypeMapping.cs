using System.Numerics;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpBigIntegerTypeMapping : KdbndpTypeMapping
{
    public KdbndpBigIntegerTypeMapping() : base("numeric", typeof(BigInteger), KdbndpDbType.Numeric) {}

    protected KdbndpBigIntegerTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Numeric)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpBigIntegerTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null
            ? storeType
            : parameters.Scale is null
                ? $"numeric({parameters.Precision})"
                : $"numeric({parameters.Precision},{parameters.Scale})";
}
