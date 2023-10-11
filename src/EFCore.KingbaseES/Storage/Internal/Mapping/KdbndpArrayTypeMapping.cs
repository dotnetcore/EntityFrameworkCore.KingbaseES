using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.ValueConversion;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// Abstract base class for KingbaseES array mappings (i.e. CLR array and <see cref="List{T}"/>.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/arrays.html
/// </remarks>
public abstract class KdbndpArrayTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// The relational type mapping used to initialize the array mapping.
    /// </summary>
    public virtual RelationalTypeMapping ElementMapping { get; }

    /// <summary>
    /// The database type used by Kdbndp.
    /// </summary>
    public virtual KdbndpDbType? KdbndpDbType { get; }

    /// <summary>
    /// Whether the array's element is nullable. This is required since <see cref="Type"/> and <see cref="ElementMapping"/> do not
    /// contain nullable reference type information.
    /// </summary>
    public virtual bool IsElementNullable { get; }

    protected KdbndpArrayTypeMapping(
        RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping, bool isElementNullable)
        : base(parameters)
    {
        ElementMapping = elementMapping;
        IsElementNullable = isElementNullable;

        // If the element mapping has an KdbndpDbType or DbType, set our own KdbndpDbType as an array of that.
        // Otherwise let the ADO.NET layer infer the KingbaseES type. We can't always let it infer, otherwise
        // when given a byte[] it will infer byte (but we want smallint[])
        KdbndpDbType = KdbndpTypes.KdbndpDbType.Array |
            (elementMapping is IKdbndpTypeMapping elementKdbndpTypeMapping
                ? elementKdbndpTypeMapping.KdbndpDbType
                : elementMapping.DbType.HasValue
                    ? new KdbndpParameter { DbType = elementMapping.DbType.Value }.KdbndpDbType
                    : default(KdbndpDbType?));
    }

    /// <summary>
    /// Returns a copy of this type mapping with <see cref="IsElementNullable"/> set to <see langword="false"/>.
    /// </summary>
    public abstract KdbndpArrayTypeMapping MakeNonNullable();

    public override CoreTypeMapping Clone(ValueConverter? converter)
    {
        // When the mapping is cloned to apply a value converter, we need to also apply that value converter to the element, otherwise
        // we end up with an array mapping over a converter-less element mapping. This is important in some inference situations.
        // If the array converter was properly set up, it's a IKdbndpArrayConverter with a reference to its element's converter.
        // Just clone the element's mapping with that (same with the null converter case).
        if (converter is IKdbndpArrayConverter or null)
        {
            return Clone(
                Parameters.WithComposedConverter(converter),
                (RelationalTypeMapping)ElementMapping.Clone(converter is IKdbndpArrayConverter arrayConverter
                    ? arrayConverter.ElementConverter
                    : null));
        }

        throw new NotSupportedException(
            $"Value converters for array or List properties must be configured via {nameof(KdbndpPropertyBuilderExtensions.HasPostgresArrayConversion)}.");
    }

    protected abstract RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping);

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        var elementMapping = ElementMapping;

        // Apply precision, scale and size to the element mapping, not to the array
        if (parameters.Size is not null)
        {
            elementMapping = elementMapping.Clone(elementMapping.StoreType, parameters.Size);
            parameters = Parameters.WithStoreTypeAndSize(elementMapping.StoreType, size: null);
        }

        if (parameters.Precision is not null || parameters.Scale is not null)
        {
            elementMapping = elementMapping.Clone(parameters.Precision, parameters.Scale);
            parameters = Parameters.WithPrecision(null).WithScale(null);
        }

        parameters = parameters.WithStoreTypeAndSize(elementMapping.StoreType + "[]", size: null);

        return Clone(parameters, elementMapping);
    }

    /// <summary>
    /// Returns a type mapping identical to this one, but over the other CLR array type. That is, convert a CLR array mapping to a List
    /// mapping and vice versa.
    /// </summary>
    public abstract KdbndpArrayTypeMapping FlipArrayListClrType(Type newType);

    // The array-to-array mapping needs to know how to generate an SQL literal for a List<>, and
    // the list-to-array mapping needs to know how to generate an SQL literal for an array.
    // This is because in cases such as ctx.SomeListColumn.SequenceEquals(new[] { 1, 2, 3}), the list mapping
    // from the left side gets applied to the right side.
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var type = value.GetType();

        if (!type.IsArray && !type.IsGenericList())
        {
            throw new ArgumentException("Parameter must be an array or List<>", nameof(value));
        }

        if (value is Array array && array.Rank != 1)
        {
            throw new NotSupportedException("Multidimensional array literals aren't supported");
        }

        var list = (IList)value;

        var sb = new StringBuilder();
        sb.Append("ARRAY[");
        for (var i = 0; i < list.Count; i++)
        {
            sb.Append(ElementMapping.GenerateProviderValueSqlLiteral(list[i]));
            if (i < list.Count - 1)
            {
                sb.Append(",");
            }
        }

        sb.Append("]::");
        sb.Append(ElementMapping.StoreType);
        sb.Append("[]");
        return sb.ToString();
    }

    protected override void ConfigureParameter(DbParameter parameter)
    {
        var KdbndpParameter = parameter as KdbndpParameter;
        if (KdbndpParameter is null)
        {
            throw new ArgumentException($"Kdbndp-specific type mapping {GetType()} being used with non-Kdbndp parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);

        if (KdbndpDbType.HasValue)
        {
            KdbndpParameter.KdbndpDbType = KdbndpDbType.Value;
        }
    }

    // isElementNullable is provided for reference-type properties by decoding NRT information from the property, since that's not
    // available on the CLR type. Note, however, that because of value conversion we may get a discrepancy between the model property's
    // nullability and the provider types' (e.g. array of nullable reference property value-converted to array of non-nullable value
    // type).
    private protected static bool CalculateElementNullability(Type elementType, bool? isElementNullable)
        => elementType.IsValueType
            ? elementType.IsNullableType()
            : isElementNullable ?? true;

    protected class NullableEqualityComparer<T> : IEqualityComparer<T?>
        where T : struct
    {
        private readonly IEqualityComparer<T> _underlyingComparer;

        public NullableEqualityComparer(IEqualityComparer<T> underlyingComparer)
            => _underlyingComparer = underlyingComparer;

        public bool Equals(T? x, T? y)
            => x is null
                ? y is null
                : y.HasValue && _underlyingComparer.Equals(x.Value, y.Value);

        public int GetHashCode(T? obj)
            => obj is null ? 0 : _underlyingComparer.GetHashCode(obj.Value);
    }
}
