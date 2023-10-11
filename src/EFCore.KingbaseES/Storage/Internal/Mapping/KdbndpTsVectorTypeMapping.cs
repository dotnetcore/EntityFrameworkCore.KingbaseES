using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTsVectorTypeMapping : KdbndpTypeMapping
{
    public KdbndpTsVectorTypeMapping() : base("tsvector", typeof(KdbndpTsVector), KdbndpDbType.TsVector) { }

    protected KdbndpTsVectorTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.TsVector) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTsVectorTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        Check.NotNull(value, nameof(value));
        var vector = (KdbndpTsVector)value;
        var builder = new StringBuilder();
        builder.Append("TSVECTOR  ");
        var indexOfFirstQuote = builder.Length - 1;
        builder.Append(vector);
        builder.Replace("'", "''");
        builder[indexOfFirstQuote] = '\'';
        builder.Append("'");
        return builder.ToString();
    }
}
