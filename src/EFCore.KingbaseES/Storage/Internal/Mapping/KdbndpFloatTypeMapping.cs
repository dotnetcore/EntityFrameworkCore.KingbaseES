using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpFloatTypeMapping : FloatTypeMapping
{
    public KdbndpFloatTypeMapping() : base("real", System.Data.DbType.Single) {}

    protected KdbndpFloatTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpFloatTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var singleValue = Convert.ToSingle(value);
        if (double.IsNaN(singleValue))
        {
            return "'NaN'";
        }

        if (double.IsPositiveInfinity(singleValue))
        {
            return "'Infinity'";
        }

        if (double.IsNegativeInfinity(singleValue))
        {
            return "'-Infinity'";
        }

        return base.GenerateNonNullSqlLiteral(singleValue);
    }
}
