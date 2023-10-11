using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpRegdictionaryTypeMapping : KdbndpTypeMapping
{
    public KdbndpRegdictionaryTypeMapping() : base("regdictionary", typeof(uint), KdbndpDbType.Oid) { }

    protected KdbndpRegdictionaryTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Oid) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpRegdictionaryTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral((string)value)}'";

    private string EscapeSqlLiteral(string literal)
        => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
}
