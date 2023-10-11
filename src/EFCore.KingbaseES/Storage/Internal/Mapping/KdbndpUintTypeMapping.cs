using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpUintTypeMapping : KdbndpTypeMapping
{
    public KdbndpUintTypeMapping(string storeType, KdbndpDbType KdbndpDbType)
        : base(storeType, typeof(uint), KdbndpDbType) {}

    protected KdbndpUintTypeMapping(RelationalTypeMappingParameters parameters, KdbndpDbType KdbndpDbType)
        : base(parameters, KdbndpDbType) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpUintTypeMapping(parameters, KdbndpDbType);
}
