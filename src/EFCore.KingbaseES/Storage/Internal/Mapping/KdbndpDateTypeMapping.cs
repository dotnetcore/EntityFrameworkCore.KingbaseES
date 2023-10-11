using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpDateTypeMapping : KdbndpTypeMapping
{
    public KdbndpDateTypeMapping(Type clrType) : base("date", clrType, KdbndpDbType.Date) {}

    protected KdbndpDateTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Date) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpDateTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"DATE '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        switch (value)
        {
            case DateTime dateTime:
                if (!KdbndpTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateTime == DateTime.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateTime == DateTime.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            case DateOnly dateOnly:
                if (!KdbndpTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateOnly == DateOnly.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateOnly == DateOnly.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            default:
                throw new InvalidCastException($"Can't generate a date SQL literal for CLR type {value.GetType()}");
        }
    }
}
