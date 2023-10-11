using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for KingbaseES multirange types.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/rangetypes.html
/// </remarks>
public class KdbndpMultirangeTypeMapping : KdbndpTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    /// The relational type mapping of the ranges contained in this multirange.
    /// </summary>
    public virtual KdbndpRangeTypeMapping RangeMapping { get; }

    /// <summary>
    /// The relational type mapping of the values contained in this multirange.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public KdbndpMultirangeTypeMapping(
        string storeType,
        Type clrType,
        KdbndpRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : this(storeType, storeTypeSchema: null, clrType, rangeMapping, sqlGenerationHelper) { }

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="storeTypeSchema">The schema of the type.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public KdbndpMultirangeTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type clrType,
        KdbndpRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(
            sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType,
            GenerateKdbndpDbType(rangeMapping.SubtypeMapping))
    {
        RangeMapping = rangeMapping;
        SubtypeMapping = rangeMapping.SubtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    protected KdbndpMultirangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        KdbndpDbType KdbndpDbType,
        KdbndpRangeTypeMapping rangeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(parameters, KdbndpDbType)
    {
        RangeMapping = rangeMapping;
        SubtypeMapping = rangeMapping.SubtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpMultirangeTypeMapping(parameters, KdbndpDbType, RangeMapping, _sqlGenerationHelper);

    protected override string GenerateNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value, RangeMapping, StoreType);

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
        if (subtypeMapping is IKdbndpTypeMapping KdbndpTypeMapping)
        {
            subtypeKdbndpDbType = KdbndpTypeMapping.KdbndpDbType;
        }
        else
        {
            // We're using a built-in, non-Kdbndp mapping such as IntTypeMapping.
            // Infer the KdbndpDbType from the DbType (somewhat hacky but why not).
            Debug.Assert(subtypeMapping.DbType.HasValue);
            var p = new KdbndpParameter { DbType = subtypeMapping.DbType.Value };
            subtypeKdbndpDbType = p.KdbndpDbType;
        }

        return subtypeKdbndpDbType;
    }

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
}
