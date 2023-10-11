using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpIntervalTypeMapping : KdbndpTypeMapping
{
    public KdbndpIntervalTypeMapping() : base("interval", typeof(TimeSpan), KdbndpDbType.Interval) {}

    protected KdbndpIntervalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Interval) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpIntervalTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"interval({parameters.Precision})";

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INTERVAL '{FormatTimeSpanAsInterval((TimeSpan)value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{FormatTimeSpanAsInterval((TimeSpan)value)}""";

    public static string FormatTimeSpanAsInterval(TimeSpan ts)
        => ts.ToString(
            $@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}",
            CultureInfo.InvariantCulture);
}
