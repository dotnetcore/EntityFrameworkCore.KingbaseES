using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpDecimalTypeMapping : KdbndpTypeMapping
{
    private const string DecimalFormatConst = "{0:0.0###########################}";

    public KdbndpDecimalTypeMapping(Type? clrType = null) : base("numeric", clrType ?? typeof(decimal), KdbndpTypes.KdbndpDbType.Numeric) {}

    protected KdbndpDecimalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpTypes.KdbndpDbType.Numeric)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpDecimalTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null
            ? storeType
            : parameters.Scale is null
                ? $"numeric({parameters.Precision})"
                : $"numeric({parameters.Precision},{parameters.Scale})";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string SqlLiteralFormatString
        => DecimalFormatConst;
}
