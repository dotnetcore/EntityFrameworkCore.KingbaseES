using System;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTimeTzTypeMapping : KdbndpTypeMapping
{
    public KdbndpTimeTzTypeMapping() : base("time with time zone", typeof(DateTimeOffset), KdbndpDbType.TimeTz) {}

    protected KdbndpTimeTzTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.TimeTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTimeTzTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"time({parameters.Precision}) with time zone";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => FormattableString.Invariant($"TIMETZ '{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}'");

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => FormattableString.Invariant(@$"""{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}""");
}
