using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpBoolTypeMapping : RelationalTypeMapping
{
    public KdbndpBoolTypeMapping() : base("boolean", typeof(bool), System.Data.DbType.Boolean) {}

    protected KdbndpBoolTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpBoolTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => (bool)value ? "TRUE" : "FALSE";
}
