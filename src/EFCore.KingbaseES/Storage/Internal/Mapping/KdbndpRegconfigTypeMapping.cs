using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpRegconfigTypeMapping : KdbndpTypeMapping
{
    public KdbndpRegconfigTypeMapping() : base("regconfig", typeof(uint), KdbndpDbType.Regconfig) { }

    protected KdbndpRegconfigTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Regconfig) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpRegconfigTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral((string)value)}'";

    private string EscapeSqlLiteral(string literal)
        => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
}
