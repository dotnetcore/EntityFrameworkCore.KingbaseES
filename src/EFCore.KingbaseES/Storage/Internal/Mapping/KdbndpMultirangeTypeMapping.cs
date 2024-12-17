using System.Collections;
using System.Data.Common;
using System.Text;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
///     The type mapping for KingbaseES multirange types.
/// </summary>
/// <remarks>
///     See: https://www.KingbaseES.org/docs/current/static/rangetypes.html
/// </remarks>
public class KdbndpMultirangeTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     The relational type mapping of the ranges contained in this multirange.
    /// </summary>
    public virtual KdbndpRangeTypeMapping RangeMapping
        => (KdbndpRangeTypeMapping)ElementTypeMapping!;

    /// <summary>
    ///     The relational type mapping of the values contained in this multirange.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    ///     The database type used by Kdbndp.
    /// </summary>
    public virtual KdbndpDbType KdbndpDbType { get; }

    /// <summary>
    ///     Constructs an instance of the <see cref="KdbndpRangeTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    public KdbndpMultirangeTypeMapping(string storeType, Type clrType, KdbndpRangeTypeMapping rangeMapping)
        // TODO: Need to do comparer, converter
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType, elementMapping: rangeMapping),
                storeType))
    {
        SubtypeMapping = rangeMapping.SubtypeMapping;
        KdbndpDbType = GenerateKdbndpDbType(rangeMapping.SubtypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected KdbndpMultirangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        KdbndpDbType npgsqlDbType)
        : base(parameters)
    {
        var rangeMapping = (KdbndpRangeTypeMapping)parameters.CoreParameters.ElementTypeMapping!;

        SubtypeMapping = rangeMapping.SubtypeMapping;
        KdbndpDbType = npgsqlDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpMultirangeTypeMapping(parameters, KdbndpDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value, RangeMapping, StoreType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string GenerateNonNullSqlLiteral(object value, RelationalTypeMapping rangeMapping, string multirangeStoreType)
    {
        var multirange = (IList)value;

        var sb = new StringBuilder();
        sb.Append("'{");

        for (var i = 0; i < multirange.Count; i++)
        {
            sb.Append(rangeMapping.GenerateEmbeddedSqlLiteral(multirange[i]));
            if (i < multirange.Count - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append("}'::");
        sb.Append(multirangeStoreType);
        return sb.ToString();
    }

    private static KdbndpDbType GenerateKdbndpDbType(RelationalTypeMapping subtypeMapping)
    {
        KdbndpDbType subtypeKdbndpDbType;
        if (subtypeMapping is IKdbndpTypeMapping npgsqlTypeMapping)
        {
            subtypeKdbndpDbType = npgsqlTypeMapping.KdbndpDbType;
        }
        else
        {
            // We're using a built-in, non-Kdbndp mapping such as IntTypeMapping.
            // Infer the KdbndpDbType from the DbType (somewhat hacky but why not).
            Debug.Assert(subtypeMapping.DbType.HasValue);
            var p = new KdbndpParameter { DbType = subtypeMapping.DbType.Value };
            subtypeKdbndpDbType = p.KdbndpDbType;
        }

        return KdbndpDbType.Multirange | subtypeKdbndpDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        // Note that arrays are handled in EF Core's CSharpHelper, so this method doesn't get called for them.

        // Unfortunately, List<KdbndpRange<T>> requires MemberInit, which CSharpHelper doesn't support
        var type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            throw new NotSupportedException("Cannot generate code literals for List<T>, consider using arrays instead");
        }

        throw new InvalidCastException();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not KdbndpParameter npgsqlParameter)
        {
            throw new ArgumentException(
                $"Kdbndp-specific type mapping {GetType()} being used with non-Kdbndp parameter type {parameter.GetType().Name}");
        }

        npgsqlParameter.KdbndpDbType = KdbndpDbType;
    }
}
