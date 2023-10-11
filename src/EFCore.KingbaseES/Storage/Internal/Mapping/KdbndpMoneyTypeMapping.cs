using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpMoneyTypeMapping : DecimalTypeMapping
{
    public KdbndpMoneyTypeMapping() : base("money", System.Data.DbType.Currency) {}

    protected KdbndpMoneyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpMoneyTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => base.GenerateNonNullSqlLiteral(value) + "::money";
}
