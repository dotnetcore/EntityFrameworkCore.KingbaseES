using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpDoubleTypeMapping : DoubleTypeMapping
{
    public KdbndpDoubleTypeMapping() : base("double precision", System.Data.DbType.Double) {}

    protected KdbndpDoubleTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpDoubleTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var doubleValue = Convert.ToDouble(value);
        if (double.IsNaN(doubleValue))
        {
            return "'NaN'";
        }

        if (double.IsPositiveInfinity(doubleValue))
        {
            return "'Infinity'";
        }

        if (double.IsNegativeInfinity(doubleValue))
        {
            return "'-Infinity'";
        }

        return base.GenerateNonNullSqlLiteral(doubleValue);
    }
}
