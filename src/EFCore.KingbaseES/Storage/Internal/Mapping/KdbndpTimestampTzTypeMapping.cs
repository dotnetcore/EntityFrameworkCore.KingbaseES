using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpTimestampTzTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static KdbndpTimestampTzTypeMapping Default { get; } = new(typeof(DateTime));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpTimestampTzTypeMapping(Type clrType)
        : base(
            "timestamp with time zone",
            clrType,
            KdbndpDbType.TimestampTz,
            clrType == typeof(DateTime)
                ? KdbndpJsonTimestampTzDateTimeReaderWriter.Instance
                : clrType == typeof(DateTimeOffset)
                    ? KdbndpJsonTimestampTzDateTimeOffsetReaderWriter.Instance
                    : throw new ArgumentException("clrType must be DateTime or DateTimeOffset", nameof(clrType)))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected KdbndpTimestampTzTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.TimestampTz)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTimestampTzTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) with time zone";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMPTZ '{Format(value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => @$"""{Format(value)}""";

    private static string Format(object value)
        => value switch
        {
            DateTime dateTime => Format(dateTime),
            DateTimeOffset dateTimeOffset => Format(dateTimeOffset),
            _ => throw new InvalidCastException(
                $"Attempted to generate timestamptz literal for type {value.GetType()}, only DateTime and DateTimeOffset are supported")
        };

    private static string Format(DateTime dateTime)
    {
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

        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z',

            DateTimeKind.Unspecified => KdbndpTypeMappingSource.LegacyTimestampBehavior || dateTime == default
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z'
                : throw new ArgumentException(
                    $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

            DateTimeKind.Local => KdbndpTypeMappingSource.LegacyTimestampBehavior
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture)
                : throw new ArgumentException(
                    $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

            _ => throw new UnreachableException()
        };
    }

    private static string Format(DateTimeOffset dateTimeOffset)
    {
        if (!KdbndpTypeMappingSource.DisableDateTimeInfinityConversions)
        {
            if (dateTimeOffset == DateTimeOffset.MinValue)
            {
                return "-infinity";
            }

            if (dateTimeOffset == DateTimeOffset.MaxValue)
            {
                return "infinity";
            }
        }

        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture);
    }

    private sealed class KdbndpJsonTimestampTzDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
    {
        public static KdbndpJsonTimestampTzDateTimeReaderWriter Instance { get; } = new();

        public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var s = manager.CurrentReader.GetString()!;

            if (!KdbndpTypeMappingSource.DisableDateTimeInfinityConversions)
            {
                switch (s)
                {
                    case "-infinity":
                        return DateTime.MinValue;
                    case "infinity":
                        return DateTime.MaxValue;
                }
            }

            // Our JSON string representation ends with Z (UTC), but DateTime.Parse returns a Local timestamp even in that case. Convert
            // it in order to return a DateTime with Kind UTC.
            return DateTime.Parse(s, CultureInfo.InvariantCulture).ToUniversalTime();
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => writer.WriteStringValue(Format(value));
    }

    private sealed class KdbndpJsonTimestampTzDateTimeOffsetReaderWriter : JsonValueReaderWriter<DateTimeOffset>
    {
        public static KdbndpJsonTimestampTzDateTimeOffsetReaderWriter Instance { get; } = new();

        public override DateTimeOffset FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var s = manager.CurrentReader.GetString()!;

            if (!KdbndpTypeMappingSource.DisableDateTimeInfinityConversions)
            {
                switch (s)
                {
                    case "-infinity":
                        return DateTimeOffset.MinValue;
                    case "infinity":
                        return DateTimeOffset.MaxValue;
                }
            }

            return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, DateTimeOffset value)
            => writer.WriteStringValue(Format(value));
    }
}
