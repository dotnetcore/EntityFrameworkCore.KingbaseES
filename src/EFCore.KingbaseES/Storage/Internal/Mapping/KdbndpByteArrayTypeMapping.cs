using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpByteArrayTypeMapping : RelationalTypeMapping
{
    public KdbndpByteArrayTypeMapping() : base("bytea", typeof(byte[]), System.Data.DbType.Binary) {}

    protected KdbndpByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpByteArrayTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        Check.NotNull(value, nameof(value));
        var bytea = (byte[])value;

        var builder = new StringBuilder(bytea.Length * 2 + 6);

        builder.Append("BYTEA E'\\\\x");
        foreach (var b in bytea)
        {
            builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }

        builder.Append('\'');

        return builder.ToString();
    }
}
