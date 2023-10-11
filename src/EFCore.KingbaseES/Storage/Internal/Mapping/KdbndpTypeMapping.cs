using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The base class for mapping Kdbndp-specific types. It configures parameters with the
/// <see cref="KdbndpDbType"/> provider-specific type enum.
/// </summary>
public abstract class KdbndpTypeMapping : RelationalTypeMapping, IKdbndpTypeMapping
{
    /// <inheritdoc />
    public virtual KdbndpDbType KdbndpDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="KdbndpDbType">The database type used by Kdbndp.</param>
    public KdbndpTypeMapping(string storeType, Type clrType, KdbndpDbType KdbndpDbType)
        : base(storeType, clrType)
        => this.KdbndpDbType = KdbndpDbType;

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpTypeMapping"/> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <param name="KdbndpDbType">The database type of the range subtype.</param>
    protected KdbndpTypeMapping(RelationalTypeMappingParameters parameters, KdbndpDbType KdbndpDbType)
        : base(parameters)
        => this.KdbndpDbType = KdbndpDbType;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not KdbndpParameter KdbndpParameter)
        {
            throw new InvalidOperationException($"Kdbndp-specific type mapping {GetType().Name} being used with non-Kdbndp parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        KdbndpParameter.KdbndpDbType = KdbndpDbType;
    }

    /// <summary>
    /// Generates the SQL representation of a literal value meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedSqlLiteral(object? value)
    {
        value = ConvertUnderlyingEnumValueToEnum(value);

        if (Converter != null)
        {
            value = Converter.ConvertToProvider(value);
        }

        return GenerateEmbeddedProviderValueSqlLiteral(value);
    }

    /// <summary>
    /// Generates the SQL representation of a literal value without conversion, meant to be embedded in another literal value,
    /// e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedProviderValueSqlLiteral(object? value)
        => value == null
            ? "NULL"
            : GenerateEmbeddedNonNullSqlLiteral(value);

    /// <summary>
    /// Generates the SQL representation of a non-null literal value, meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    protected virtual string GenerateEmbeddedNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value);

    // Copied from RelationalTypeMapping
    private object? ConvertUnderlyingEnumValueToEnum(object? value)
        => value?.GetType().IsInteger() == true && ClrType.UnwrapNullableType().IsEnum
            ? Enum.ToObject(ClrType.UnwrapNullableType(), value)
            : value;
}
