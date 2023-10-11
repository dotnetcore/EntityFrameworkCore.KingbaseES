using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The base class for mapping Kdbndp-specific string types. It configures parameters with the
/// <see cref="KdbndpDbType"/> provider-specific type enum.
/// </summary>
public class KdbndpStringTypeMapping : StringTypeMapping, IKdbndpTypeMapping
{
    /// <inheritdoc />
    public virtual KdbndpDbType KdbndpDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="KdbndpDbType">The database type used by Kdbndp.</param>
    public KdbndpStringTypeMapping(string storeType, KdbndpDbType KdbndpDbType)
        : base(storeType, System.Data.DbType.String)
        => this.KdbndpDbType = KdbndpDbType;

    protected KdbndpStringTypeMapping(
        RelationalTypeMappingParameters parameters,
        KdbndpDbType KdbndpDbType)
        : base(parameters)
        => this.KdbndpDbType = KdbndpDbType;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpStringTypeMapping(parameters, KdbndpDbType);

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not KdbndpParameter KdbndpParameter)
        {
            throw new InvalidOperationException($"Kdbndp-specific type mapping {GetType().Name} being used with non-Kdbndp parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        KdbndpParameter.KdbndpDbType = KdbndpDbType;
    }
}
