using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for KingbaseES range types.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/rangetypes.html
/// </remarks>
public class KdbndpRangeTypeMapping : KdbndpTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private PropertyInfo? _isEmptyProperty;
    private PropertyInfo? _lowerProperty;
    private PropertyInfo? _upperProperty;
    private PropertyInfo? _lowerInclusiveProperty;
    private PropertyInfo? _upperInclusiveProperty;
    private PropertyInfo? _lowerInfiniteProperty;
    private PropertyInfo? _upperInfiniteProperty;

    private ConstructorInfo? _rangeConstructor1;
    private ConstructorInfo? _rangeConstructor2;
    private ConstructorInfo? _rangeConstructor3;

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The relational type mapping of the range's subtype.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public KdbndpRangeTypeMapping(
        string storeType,
        Type clrType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : this(storeType, storeTypeSchema: null, clrType, subtypeMapping, sqlGenerationHelper) {}

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="storeTypeSchema">The schema of the type.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public KdbndpRangeTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type clrType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType, GenerateKdbndpDbType(subtypeMapping))
    {
        Debug.Assert(clrType == typeof(KdbndpRange<>).MakeGenericType(subtypeMapping.ClrType));

        SubtypeMapping = subtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    protected KdbndpRangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        KdbndpDbType KdbndpDbType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(parameters, KdbndpDbType)
    {
        SubtypeMapping = subtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpRangeTypeMapping(parameters, KdbndpDbType, SubtypeMapping, _sqlGenerationHelper);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{GenerateEmbeddedNonNullSqlLiteral(value)}'::{StoreType}";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        InitializeAccessors(ClrType, SubtypeMapping.ClrType);

        var builder = new StringBuilder();

        if ((bool)_isEmptyProperty.GetValue(value)!)
        {
            builder.Append("empty");
        }
        else
        {
            builder.Append((bool)_lowerInclusiveProperty.GetValue(value)! ? '[' : '(');

            if (!(bool)_lowerInfiniteProperty.GetValue(value)!)
            {
                builder.Append(SubtypeMapping.GenerateEmbeddedSqlLiteral(_lowerProperty.GetValue(value)));
            }

            builder.Append(',');

            if (!(bool)_upperInfiniteProperty.GetValue(value)!)
            {
                builder.Append(SubtypeMapping.GenerateEmbeddedSqlLiteral(_upperProperty.GetValue(value)));
            }

            builder.Append((bool)_upperInclusiveProperty.GetValue(value)! ? ']' : ')');
        }

        return builder.ToString();
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

        return KdbndpDbType.Range | subtypeKdbndpDbType;
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        InitializeAccessors(ClrType, SubtypeMapping.ClrType);

        var lower = _lowerProperty.GetValue(value);
        var upper = _upperProperty.GetValue(value);
        var lowerInclusive = (bool)_lowerInclusiveProperty.GetValue(value)!;
        var upperInclusive = (bool)_upperInclusiveProperty.GetValue(value)!;
        var lowerInfinite = (bool)_lowerInfiniteProperty.GetValue(value)!;
        var upperInfinite = (bool)_upperInfiniteProperty.GetValue(value)!;

        return lowerInfinite || upperInfinite
            ? Expression.New(
                _rangeConstructor3,
                Expression.Constant(lower),
                Expression.Constant(lowerInclusive),
                Expression.Constant(lowerInfinite),
                Expression.Constant(upper),
                Expression.Constant(upperInclusive),
                Expression.Constant(upperInfinite))
            : lowerInclusive && upperInclusive
                ? Expression.New(
                    _rangeConstructor1,
                    Expression.Constant(lower),
                    Expression.Constant(upper))
                : Expression.New(
                    _rangeConstructor2,
                    Expression.Constant(lower),
                    Expression.Constant(lowerInclusive),
                    Expression.Constant(upper),
                    Expression.Constant(upperInclusive));
    }

    [MemberNotNull(
        "_isEmptyProperty",
        "_lowerProperty", "_upperProperty",
        "_lowerInclusiveProperty", "_upperInclusiveProperty",
        "_lowerInfiniteProperty", "_upperInfiniteProperty",
        "_rangeConstructor1", "_rangeConstructor2", "_rangeConstructor3")]
    private void InitializeAccessors(Type rangeClrType, Type subtypeClrType)
    {
        _isEmptyProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.IsEmpty))!;
        _lowerProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.LowerBound))!;
        _upperProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.UpperBound))!;
        _lowerInclusiveProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.LowerBoundIsInclusive))!;
        _upperInclusiveProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.UpperBoundIsInclusive))!;
        _lowerInfiniteProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.LowerBoundInfinite))!;
        _upperInfiniteProperty = rangeClrType.GetProperty(nameof(KdbndpRange<int>.UpperBoundInfinite))!;

        _rangeConstructor1 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, subtypeClrType })!;
        _rangeConstructor2 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, typeof(bool), subtypeClrType, typeof(bool) })!;
        _rangeConstructor3 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, typeof(bool), typeof(bool), subtypeClrType, typeof(bool), typeof(bool) })!;
    }
}
