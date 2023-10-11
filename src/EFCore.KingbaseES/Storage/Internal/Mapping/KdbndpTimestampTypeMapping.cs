using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTimestampTypeMapping : KdbndpTypeMapping
{
    public KdbndpTimestampTypeMapping() : base("timestamp without time zone", typeof(DateTime), KdbndpDbType.Timestamp) {}

    protected KdbndpTimestampTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Timestamp) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTimestampTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) without time zone";

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMP '{GenerateLiteralCore(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
    {
        var dateTime = (DateTime)value;

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

        return KdbndpTypeMappingSource.LegacyTimestampBehavior || dateTime.Kind != DateTimeKind.Utc
            ? dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF", CultureInfo.InvariantCulture)
            : throw new InvalidCastException("'timestamp without time zone' literal cannot be generated for a UTC DateTime");
    }
}
