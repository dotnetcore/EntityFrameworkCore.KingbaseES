using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpTypeMappingSource : RelationalTypeMappingSource
{
#if DEBUG
    internal static bool LegacyTimestampBehavior;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    internal static bool DisableDateTimeInfinityConversions;
#else
    internal static readonly bool LegacyTimestampBehavior;
    internal static readonly bool DisableDateTimeInfinityConversions;
#endif

    static KdbndpTypeMappingSource()
    {
        LegacyTimestampBehavior = AppContext.TryGetSwitch("Kdbndp.EnableLegacyTimestampBehavior", out var enabled) && enabled;
        DisableDateTimeInfinityConversions = AppContext.TryGetSwitch("Kdbndp.DisableDateTimeInfinityConversions", out enabled) && enabled;
    }

    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

    private readonly IReadOnlyList<EnumDefinition> _enumDefinitions;
    private readonly IReadOnlyList<UserRangeDefinition> _userRangeDefinitions;

    private readonly bool _supportsMultiranges;

    #region Mappings

    // Numeric types
    private readonly KdbndpFloatTypeMapping _float4 = KdbndpFloatTypeMapping.Default;
    private readonly KdbndpDoubleTypeMapping _float8 = KdbndpDoubleTypeMapping.Default;
    private readonly KdbndpDecimalTypeMapping _numeric = KdbndpDecimalTypeMapping.Default;
    private readonly KdbndpBigIntegerTypeMapping _bigInteger = KdbndpBigIntegerTypeMapping.Default;
    private readonly KdbndpDecimalTypeMapping _numericAsFloat = new(typeof(float));
    private readonly KdbndpDecimalTypeMapping _numericAsDouble = new(typeof(double));
    private readonly KdbndpMoneyTypeMapping _money = KdbndpMoneyTypeMapping.Default;
    private readonly GuidTypeMapping _uuid = new("uuid", DbType.Guid);
    private readonly ShortTypeMapping _int2 = new("smallint", DbType.Int16);
    private readonly ByteTypeMapping _int2Byte = new("smallint", DbType.Byte);
    private readonly IntTypeMapping _int4 = new("integer", DbType.Int32);
    private readonly LongTypeMapping _int8 = new("bigint", DbType.Int64);

    // Character types
    private readonly StringTypeMapping _text = new("text", DbType.String);
    private readonly KdbndpStringTypeMapping _varchar = new("character varying", KdbndpDbType.Varchar);
    private readonly KdbndpCharacterStringTypeMapping _char = new("character");
    private readonly KdbndpCharacterCharTypeMapping _singleChar = new("character(1)");
    private readonly KdbndpStringTypeMapping _xml = new("xml", KdbndpDbType.Xml);
    private readonly KdbndpStringTypeMapping _citext = new("citext", KdbndpDbType.Citext);
    private readonly KdbndpStringTypeMapping _jsonpath = new("jsonpath", KdbndpDbType.JsonPath);

    // JSON mappings - EF owned entity support
    private readonly KdbndpOwnedJsonTypeMapping _jsonbOwned = new("jsonb");
    private readonly KdbndpOwnedJsonTypeMapping _jsonOwned = new("json");

    // JSON mappings - older string/weakly-typed support
    private readonly KdbndpJsonTypeMapping _jsonbString = new("jsonb", typeof(string));
    private readonly KdbndpJsonTypeMapping _jsonString = new("json", typeof(string));
    private readonly KdbndpJsonTypeMapping _jsonbDocument = new("jsonb", typeof(JsonDocument));
    private readonly KdbndpJsonTypeMapping _jsonDocument = new("json", typeof(JsonDocument));
    private readonly KdbndpJsonTypeMapping _jsonbElement = new("jsonb", typeof(JsonElement));
    private readonly KdbndpJsonTypeMapping _jsonElement = new("json", typeof(JsonElement));

    // Date/Time types
    private readonly KdbndpDateTimeDateTypeMapping _dateDateTime = KdbndpDateTimeDateTypeMapping.Default;
    private readonly KdbndpTimestampTypeMapping _timestamp = KdbndpTimestampTypeMapping.Default;
    private readonly KdbndpTimestampTzTypeMapping _timestamptz = KdbndpTimestampTzTypeMapping.Default;
    private readonly KdbndpTimestampTzTypeMapping _timestamptzDto = new(typeof(DateTimeOffset));
    private readonly KdbndpIntervalTypeMapping _interval = KdbndpIntervalTypeMapping.Default;
    private readonly KdbndpTimeTypeMapping _timeTimeSpan = new(typeof(TimeSpan));
    private readonly KdbndpTimeTzTypeMapping _timetz = KdbndpTimeTzTypeMapping.Default;

    private readonly KdbndpDateOnlyTypeMapping _dateDateOnly = KdbndpDateOnlyTypeMapping.Default;
    private readonly KdbndpTimeTypeMapping _timeTimeOnly = KdbndpTimeTypeMapping.Default;

    // Network address types
    private readonly KdbndpMacaddrTypeMapping _macaddr = KdbndpMacaddrTypeMapping.Default;
    private readonly KdbndpMacaddr8TypeMapping _macaddr8 = KdbndpMacaddr8TypeMapping.Default;
    private readonly KdbndpInetTypeMapping _inetAsIPAddress = KdbndpInetTypeMapping.Default;
    private readonly KdbndpInetTypeMapping _inetAsKdbndpInet = new(typeof(KdbndpInet));
    private readonly KdbndpCidrTypeMapping _cidr = KdbndpCidrTypeMapping.Default;

    // Built-in geometric types
    private readonly KdbndpPointTypeMapping _point = KdbndpPointTypeMapping.Default;
    private readonly KdbndpBoxTypeMapping _box = KdbndpBoxTypeMapping.Default;
    private readonly KdbndpLineTypeMapping _line = KdbndpLineTypeMapping.Default;
    private readonly KdbndpLineSegmentTypeMapping _lseg = KdbndpLineSegmentTypeMapping.Default;
    private readonly KdbndpPathTypeMapping _path = KdbndpPathTypeMapping.Default;
    private readonly KdbndpPolygonTypeMapping _polygon = KdbndpPolygonTypeMapping.Default;
    private readonly KdbndpCircleTypeMapping _circle = KdbndpCircleTypeMapping.Default;

    // uint/ulong mappings
    private readonly KdbndpUIntTypeMapping _xid = new("xid", KdbndpDbType.Xid);
    private readonly KdbndpULongTypeMapping _xid8 = new("xid8", KdbndpDbType.Xid8);
    private readonly KdbndpUIntTypeMapping _oid = new("oid", KdbndpDbType.Oid);
    private readonly KdbndpUIntTypeMapping _cid = new("cid", KdbndpDbType.Cid);
    private readonly KdbndpUIntTypeMapping _regtype = new("regtype", KdbndpDbType.Regtype);
    private readonly KdbndpUIntTypeMapping _lo = new("lo", KdbndpDbType.Oid);

    // Full text search mappings
    private readonly KdbndpTsQueryTypeMapping _tsquery = KdbndpTsQueryTypeMapping.Default;
    private readonly KdbndpTsVectorTypeMapping _tsvector = KdbndpTsVectorTypeMapping.Default;
    private readonly KdbndpRegconfigTypeMapping _regconfig = KdbndpRegconfigTypeMapping.Default;
    private readonly KdbndpTsRankingNormalizationTypeMapping _rankingNormalization = KdbndpTsRankingNormalizationTypeMapping.Default;

    // Unaccent mapping
    private readonly KdbndpRegdictionaryTypeMapping _regdictionary = new();

    // Built-in ranges
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly KdbndpRangeTypeMapping _int4range;
    private readonly KdbndpRangeTypeMapping _int8range;
    private readonly KdbndpRangeTypeMapping _numrange;
    private readonly KdbndpRangeTypeMapping _tsrange;
    private readonly KdbndpRangeTypeMapping _tstzrange, _tstzrangeDto;
    private readonly KdbndpRangeTypeMapping _dateOnlyDaterange;
    private readonly KdbndpRangeTypeMapping _dateTimeDaterange;

    // Other types
    private readonly KdbndpBoolTypeMapping _bool = KdbndpBoolTypeMapping.Default;
    private readonly KdbndpBitTypeMapping _bit = KdbndpBitTypeMapping.Default;
    private readonly KdbndpVarbitTypeMapping _varbit = KdbndpVarbitTypeMapping.Default;
    private readonly KdbndpByteArrayTypeMapping _bytea = KdbndpByteArrayTypeMapping.Default;
    private readonly KdbndpHstoreTypeMapping _hstore = KdbndpHstoreTypeMapping.Default;
    private readonly KdbndpHstoreTypeMapping _immutableHstore = new(typeof(ImmutableDictionary<string, string>));
    private readonly KdbndpTidTypeMapping _tid = KdbndpTidTypeMapping.Default;
    private readonly KdbndpKbLsnTypeMapping _pgLsn = KdbndpKbLsnTypeMapping.Default;

    private readonly KdbndpLTreeTypeMapping _ltree = KdbndpLTreeTypeMapping.Default;
    private readonly KdbndpStringTypeMapping _ltreeString = new("ltree", KdbndpDbType.LTree);
    private readonly KdbndpStringTypeMapping _lquery = new("lquery", KdbndpDbType.LQuery);
    private readonly KdbndpStringTypeMapping _ltxtquery = new("ltxtquery", KdbndpDbType.LTxtQuery);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Special stuff
    // ReSharper disable once InconsistentNaming
    public readonly KdbndpEStringTypeMapping EStringTypeMapping = KdbndpEStringTypeMapping.Default;

    #endregion Mappings

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper,
        IKdbndpSingletonOptions options)
        : base(dependencies, relationalDependencies)
    {
        _supportsMultiranges = options.PostgresVersion.AtLeast(14);
        _sqlGenerationHelper = Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));

        // Initialize range mappings, which reference on other mappings
        _int4range = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "int4range", typeof(KdbndpRange<int>), KdbndpDbType.IntegerRange, _int4);
        _int8range = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "int8range", typeof(KdbndpRange<long>), KdbndpDbType.BigIntRange, _int8);
        _numrange = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "numrange", typeof(KdbndpRange<decimal>), KdbndpDbType.NumericRange, _numeric);
        _tsrange = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "tsrange", typeof(KdbndpRange<DateTime>), KdbndpDbType.TimestampRange, _timestamp);
        _tstzrange = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "tstzrange", typeof(KdbndpRange<DateTime>), KdbndpDbType.TimestampTzRange, _timestamptz);
        _tstzrangeDto = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "tstzrange", typeof(KdbndpRange<DateTimeOffset>), KdbndpDbType.TimestampTzRange, _timestamptzDto);
        _dateOnlyDaterange = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "daterange", typeof(KdbndpRange<DateOnly>), KdbndpDbType.DateRange, _dateDateOnly);
        _dateTimeDaterange = KdbndpRangeTypeMapping.CreatBuiltInRangeMapping(
            "daterange", typeof(KdbndpRange<DateTime>), KdbndpDbType.DateRange, _dateDateTime);

// ReSharper disable CoVariantArrayConversion
        // Note that KingbaseES has aliases to some built-in type name aliases (e.g. int4 for integer),
        // these are mapped as well.
        // https://www.KingbaseES.org/docs/current/static/datatype.html#DATATYPE-TABLE
        var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "smallint", [_int2, _int2Byte] },
            { "int2", [_int2, _int2Byte] },
            { "integer", [_int4] },
            { "int", [_int4] },
            { "int4", [_int4] },
            { "bigint", [_int8] },
            { "int8", [_int8] },
            { "real", [_float4] },
            { "float4", [_float4] },
            { "double precision", [_float8] },
            { "float8", [_float8] },
            { "numeric", [_numeric, _bigInteger, _numericAsFloat, _numericAsDouble] },
            { "decimal", [_numeric, _bigInteger, _numericAsFloat, _numericAsDouble] },
            { "money", [_money] },
            { "text", [_text] },
            { "jsonb", [_jsonbString, _jsonbDocument, _jsonbElement] },
            { "json", [_jsonString, _jsonDocument, _jsonElement] },
            { "jsonpath", [_jsonpath] },
            { "xml", [_xml] },
            { "citext", [_citext] },
            { "character varying", [_varchar] },
            { "varchar", [_varchar] },
            // See FindBaseMapping below for special treatment of 'character'

            { "timestamp without time zone", [_timestamp] },
            { "timestamp with time zone", [_timestamptz, _timestamptzDto] },
            { "interval", [_interval] },
            { "date", [_dateDateOnly, _dateDateTime] },
            { "time without time zone", [_timeTimeOnly, _timeTimeSpan] },
            { "time with time zone", [_timetz] },
            { "boolean", [_bool] },
            { "bool", [_bool] },
            { "bytea", [_bytea] },
            { "uuid", [_uuid] },
            { "bit", [_bit] },
            { "bit varying", [_varbit] },
            { "varbit", [_varbit] },
            { "hstore", [_hstore, _immutableHstore] },
            { "macaddr", [_macaddr] },
            { "macaddr8", [_macaddr8] },
            { "inet", [_inetAsIPAddress, _inetAsKdbndpInet] },
            { "cidr", [_cidr] },
            { "point", [_point] },
            { "box", [_box] },
            { "line", [_line] },
            { "lseg", [_lseg] },
            { "path", [_path] },
            { "polygon", [_polygon] },
            { "circle", [_circle] },
            { "xid", [_xid] },
            { "xid8", [_xid8] },
            { "oid", [_oid] },
            { "cid", [_cid] },
            { "regtype", [_regtype] },
            { "lo", [_lo] },
            { "tid", [_tid] },
            { "sys_lsn", [_pgLsn] },
            { "int4range", [_int4range] },
            { "int8range", [_int8range] },
            { "numrange", [_numrange] },
            { "tsrange", [_tsrange] },
            { "tstzrange", [_tstzrange, _tstzrangeDto] },
            { "daterange", [_dateOnlyDaterange, _dateTimeDaterange] },
            { "tsquery", [_tsquery] },
            { "tsvector", [_tsvector] },
            { "regconfig", [_regconfig] },
            { "ltree", [_ltree, _ltreeString] },
            { "lquery", [_lquery] },
            { "ltxtquery", [_ltxtquery] },
            { "regdictionary", [_regdictionary] }
        };
// ReSharper restore CoVariantArrayConversion

        // Set up aliases
        storeTypeMappings["timestamp"] = storeTypeMappings["timestamp without time zone"];
        storeTypeMappings["timestamptz"] = storeTypeMappings["timestamp with time zone"];
        storeTypeMappings["time"] = storeTypeMappings["time without time zone"];
        storeTypeMappings["timetz"] = storeTypeMappings["time with time zone"];

        var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(bool), _bool },
            { typeof(Guid), _uuid },
            { typeof(byte), _int2Byte },
            { typeof(short), _int2 },
            { typeof(int), _int4 },
            { typeof(long), _int8 },
            { typeof(float), _float4 },
            { typeof(double), _float8 },
            { typeof(decimal), _numeric },
            { typeof(BigInteger), _bigInteger },
            { typeof(string), _text },
            { typeof(JsonDocument), _jsonbDocument },
            // { typeof(JsonElement),                         _jsonbElement         },
            { typeof(JsonElement), _jsonbOwned },
            { typeof(char), _singleChar },
            { typeof(DateTime), LegacyTimestampBehavior ? _timestamp : _timestamptz },
            { typeof(DateOnly), _dateDateOnly },
            { typeof(TimeOnly), _timeTimeOnly },
            { typeof(TimeSpan), _interval },
            { typeof(DateTimeOffset), _timestamptzDto },
            { typeof(PhysicalAddress), _macaddr },
            { typeof(IPAddress), _inetAsIPAddress },
            { typeof(KdbndpInet), _inetAsKdbndpInet },
            { typeof(KdbndpCidr), _cidr },
            { typeof(BitArray), _varbit },
            { typeof(ImmutableDictionary<string, string>), _immutableHstore },
            { typeof(Dictionary<string, string>), _hstore },
            { typeof(KdbndpTid), _tid },
            { typeof(KdbndpLogSequenceNumber), _pgLsn },
            { typeof(KdbndpPoint), _point },
            { typeof(KdbndpBox), _box },
            { typeof(KdbndpLine), _line },
            { typeof(KdbndpLSeg), _lseg },
            { typeof(KdbndpPath), _path },
            { typeof(KdbndpPolygon), _polygon },
            { typeof(KdbndpCircle), _circle },
            { typeof(KdbndpRange<int>), _int4range },
            { typeof(KdbndpRange<long>), _int8range },
            { typeof(KdbndpRange<decimal>), _numrange },
            { typeof(KdbndpRange<DateTime>), LegacyTimestampBehavior ? _tsrange : _tstzrange },
            { typeof(KdbndpRange<DateTimeOffset>), _tstzrange },
            { typeof(KdbndpRange<DateOnly>), _dateOnlyDaterange },
            { typeof(KdbndpTsQuery), _tsquery },
            { typeof(KdbndpTsVector), _tsvector },
            { typeof(KdbndpTsRankingNormalization), _rankingNormalization },
            { typeof(LTree), _ltree }
        };

        StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
        ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

        _enumDefinitions = options.EnumDefinitions;
        _userRangeDefinitions = options.UserRangeDefinitions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        // First, try any plugins, allowing them to override built-in mappings (e.g. NodaTime)
        => base.FindMapping(mappingInfo)
            ?? FindBaseMapping(mappingInfo)?.Clone(mappingInfo)
            ?? FindEnumMapping(mappingInfo)
            ?? FindRowValueMapping(mappingInfo)?.Clone(mappingInfo)
            ?? FindUserRangeMapping(mappingInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

        if (storeTypeName is not null)
        {
            if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
            {
                // We found the user-specified store type. No CLR type was provided - we're probably
                // scaffolding from an existing database, take the first mapping as the default.
                if (clrType is null)
                {
                    return mappings[0];
                }

                // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                // immediately.
                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                // Map arbitrary user POCOs to JSON
                if (storeTypeName is "jsonb" or "json")
                {
                    return new KdbndpJsonTypeMapping(storeTypeName, clrType);
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            // 'character' is special: 'character' (no size) and 'character(1)' map to a single char, whereas 'character(n)' maps
            // to a string
            if (storeTypeNameBase is "character" or "char")
            {
                if (mappingInfo.Size is null or 1 && clrType is null || clrType == typeof(char))
                {
                    return _singleChar.Clone(mappingInfo);
                }

                if (clrType is null || clrType == typeof(string))
                {
                    return _char.Clone(mappingInfo);
                }
            }

            if ((storeTypeName.EndsWith("[]", StringComparison.Ordinal)
                    || storeTypeName is "int4multirange" or "int8multirange" or "nummultirange" or "datemultirange" or "tsmultirange"
                        or "tstzmultirange")
                && FindCollectionMapping(mappingInfo, mappingInfo.ClrType, providerType: null, elementMapping: null) is
                    RelationalTypeMapping collectionMapping)
            {
                return collectionMapping;
            }

            // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
            // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
        }

        if (clrType is not null)
        {
            if (ClrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                // Handle types with the size facet (string, bitarray)
                if (mappingInfo.Size > 0)
                {
                    if (clrType == typeof(string))
                    {
                        mapping = mappingInfo.IsFixedLength ?? false ? _char : _varchar;

                        // See #342 for when size > 10485760
                        return mappingInfo.Size <= 10485760
                            ? mapping.WithStoreTypeAndSize($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size)
                            : _text;
                    }

                    if (clrType == typeof(BitArray))
                    {
                        mapping = mappingInfo.IsFixedLength ?? false ? _bit : _varbit;
                        return mapping.WithStoreTypeAndSize($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size);
                    }
                }

                return mapping;
            }

            if (clrType == typeof(byte[]) && mappingInfo.ElementTypeMapping is null)
            {
                if (storeTypeName == "smallint[]")
                {
                    // KingbaseES has no tinyint (single-byte) type, but we allow mapping CLR byte to PG smallint (2-bytes).
                    // The same applies to arrays - as always - so byte[] should be mappable to smallint[].
                    // However, byte[] also has a base mapping to bytea, which is the default. So when the user explicitly specified
                    // mapping to smallint[], we don't return that to allow the array mapping to work.
                    // TODO: This is a workaround; RelationalTypeMappingSource first attempts to find a value converter before trying
                    // to find a collection. We should reverse the order and call FindCollectionMapping before attempting to find a
                    // value converter.
                    // TODO: Make sure the providerType should be null
                    return FindCollectionMapping(mappingInfo, typeof(byte[]), providerType: null, elementMapping: null);
                    // return null;
                }

                return _bytea;
            }

            if (mappingInfo.IsRowVersion == true)
            {
                if (clrType == typeof(uint))
                {
                    return _xid;
                }

                if (clrType == typeof(ulong))
                {
                    return _xid8;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping? FindCollectionMapping(
        RelationalTypeMappingInfo info,
        // Note that modelType is nullable (in the relational base signature it isn't) because of array scaffolding, i.e. we call
        // FindCollectionMapping from our own FindMapping, where the clrType is null when scaffolding.
        Type? modelType,
        Type? providerType,
        CoreTypeMapping? elementMapping)
    {
        if (elementMapping is not null and not RelationalTypeMapping)
        {
            return null;
        }

        Type concreteCollectionType;
        Type? elementType = null;

        if (modelType is not null)
        {
            // We do GetElementType for multidimensional arrays - these don't implement generic IEnumerable<>
            elementType = modelType.TryGetElementType(typeof(IEnumerable<>)) ?? modelType.GetElementType();

            // E.g. Newtonsoft.Json's JToken is enumerable over itself, exclude that scenario to avoid stack overflow.
            if (elementType is null || elementType == modelType || modelType.GetGenericTypeImplementations(typeof(IDictionary<,>)).Any())
            {
                return null;
            }
        }

        var storeType = info.StoreTypeName;
        if (storeType is null)
        {
            if (modelType is null)
            {
                return null;
            }

            // If no mapping was found for the element CLR type, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            Check.DebugAssert(elementType is not null, "elementClrType is null");

            var relationalElementMapping = elementMapping as RelationalTypeMapping ?? FindMapping(elementType);
            if (relationalElementMapping is not { ElementTypeMapping: null })
            {
                return null;
            }

            // If the element type mapping is a range, default to return a multirange type mapping (if the PG version supports it).
            // Otherwise an array over the range will be returned.
            if (_supportsMultiranges)
            {
                if (relationalElementMapping is KdbndpRangeTypeMapping rangeMapping)
                {
                    var multirangeStoreType = rangeMapping.StoreType switch
                    {
                        "int4range" => "int4multirange",
                        "int8range" => "int8multirange",
                        "numrange" => "nummultirange",
                        "tsrange" => "tsmultirange",
                        "tstzrange" => "tstzmultirange",
                        "daterange" => "datemultirange",

                        _ => throw new InvalidOperationException(
                            $"Cannot create multirange type mapping for range type '{rangeMapping.StoreType}'")
                    };

                    return new KdbndpMultirangeTypeMapping(multirangeStoreType, modelType, rangeMapping);
                }

                // TODO: This needs to move to the NodaTime plugin, but there's no FindCollectionMapping extension yet for plugins
                if (relationalElementMapping.GetType() is
                    { Name: "IntervalRangeMapping", Namespace: "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal" } type1)
                {
                    return (RelationalTypeMapping)Activator.CreateInstance(
                        type1.Assembly.GetType(
                            "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.IntervalMultirangeMapping")!,
                        modelType,
                        relationalElementMapping)!;
                }

                if (relationalElementMapping.GetType() is
                    { Name: "DateIntervalRangeMapping", Namespace: "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal" } type2)
                {
                    return (RelationalTypeMapping)Activator.CreateInstance(
                        type2.Assembly.GetType(
                            "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.DateIntervalMultirangeMapping")!,
                        modelType,
                        relationalElementMapping)!;
                }
            }

            // Not a multirange - map as a PG array type
            concreteCollectionType = FindTypeToInstantiate(modelType, elementType);

            return (KdbndpArrayTypeMapping)Activator.CreateInstance(
                typeof(KdbndpArrayTypeMapping<,,>).MakeGenericType(modelType, concreteCollectionType, elementType),
                relationalElementMapping)!;
        }

        if (storeType.EndsWith("[]", StringComparison.Ordinal))
        {
            // We have an array store type (either because we're reverse engineering or the user explicitly specified it)
            var elementStoreType = storeType.Substring(0, storeType.Length - 2);

            // Note that we ignore the elementMapping argument here (but not in the CLR type-only path above).
            // This is because the user-provided storeType for the array should take precedence over the element type mapping that gets
            // calculated purely based on the element's CLR type in base.FindMappingWithConversion.
            var relationalElementMapping = elementMapping as RelationalTypeMapping
                ?? (elementType is null
                    ? FindMapping(elementStoreType)
                    : FindMapping(elementType, elementStoreType));
            if (relationalElementMapping is not { ElementTypeMapping: null })
            {
                return null;
            }

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            if (relationalElementMapping is not null and not KdbndpArrayTypeMapping)
            {
                if (modelType is null)
                {
                    // There's no model type - we're scaffolding purely from the store type.
                    elementType = relationalElementMapping.ClrType;
                    modelType = concreteCollectionType = typeof(List<>).MakeGenericType(elementType);
                }
                else
                {
                    concreteCollectionType = FindTypeToInstantiate(modelType, elementType!);
                    Check.DebugAssert(elementType is not null, "elementType is null");
                }

                return (KdbndpArrayTypeMapping)Activator.CreateInstance(
                    typeof(KdbndpArrayTypeMapping<,,>).MakeGenericType(modelType, concreteCollectionType, elementType),
                    storeType, relationalElementMapping)!;
            }
        }
        else if (IsMultirange(storeType, out var rangeStoreType) && _supportsMultiranges)
        {
            // Note that we ignore the elementMapping argument here (but not in the CLR type-only path above).
            // This is because the user-provided storeType for the array should take precedence over the element type mapping that gets
            // calculated purely based on the element's CLR type in base.FindMappingWithConversion.
            var relationalElementMapping = elementType is null
                ? FindMapping(rangeStoreType)
                : FindMapping(elementType, rangeStoreType);

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            if (relationalElementMapping is KdbndpRangeTypeMapping rangeMapping
                // TODO: Why exclude if there's an element converter??
                && (relationalElementMapping.Converter is null || modelType is null || modelType.IsArrayOrGenericList()))
            {
                return new KdbndpMultirangeTypeMapping(
                    storeType, modelType ?? typeof(List<>).MakeGenericType(relationalElementMapping.ClrType), rangeMapping);
            }

            // TODO: This needs to move to the NodaTime plugin, but there's no FindCollectionMapping extension yet for plugins
            if (relationalElementMapping?.GetType() is
                { Name: "IntervalRangeMapping", Namespace: "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal" } type1)
            {
                return (RelationalTypeMapping)Activator.CreateInstance(
                    type1.Assembly.GetType(
                        "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.IntervalMultirangeMapping")!,
                    modelType ?? relationalElementMapping.ClrType.MakeArrayType(),
                    relationalElementMapping)!;
            }

            if (relationalElementMapping?.GetType() is
                { Name: "DateIntervalRangeMapping", Namespace: "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal" } type2)
            {
                return (RelationalTypeMapping)Activator.CreateInstance(
                    type2.Assembly.GetType(
                        "Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.DateIntervalMultirangeMapping")!,
                    modelType ?? relationalElementMapping.ClrType.MakeArrayType(),
                    relationalElementMapping)!;
            }
        }

        return null;

        static bool IsMultirange(string multiRangeStoreType, [NotNullWhen(true)] out string? rangeStoreType)
        {
            rangeStoreType = multiRangeStoreType switch
            {
                "int4multirange" => "int4range",
                "int8multirange" => "int8range",
                "nummultirange" => "numrange",
                "tsmultirange" => "tsrange",
                "tstzmultirange" => "tstzrange",
                "datemultirange" => "daterange",
                _ => null
            };

            return rangeStoreType is not null;
        }

        static Type FindTypeToInstantiate(Type collectionType, Type elementType)
        {
            if (collectionType.IsArray)
            {
                return collectionType;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);

            if (collectionType.IsAssignableFrom(listOfT))
            {
                if (!collectionType.IsAbstract)
                {
                    var constructor = collectionType.GetDeclaredConstructor(null);
                    if (constructor?.IsPublic == true)
                    {
                        return collectionType;
                    }
                }

                return listOfT;
            }

            return collectionType;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindRowValueMapping(in RelationalTypeMappingInfo mappingInfo)
        => mappingInfo.ClrType is { } clrType
            && clrType.IsAssignableTo(typeof(ITuple))
                ? new KdbndpRowValueTypeMapping(clrType)
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindEnumMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var storeType = mappingInfo.StoreTypeName;
        var clrType = mappingInfo.ClrType;

        if (clrType is not null and not { IsEnum: true, IsClass: false })
        {
            return null;
        }

        // Try to find an enum definition (defined by the user on their context options), based on the
        // incoming MappingInfo's StoreType or ClrType
        EnumDefinition? enumDefinition;
        if (storeType is null)
        {
            enumDefinition = _enumDefinitions.SingleOrDefault(m => m.ClrType == clrType);
        }
        else
        {
            // TODO: Not sure what to do about quoting. Is the user expected to configure properties
            // TODO: with a quoted (schema-qualified) store type or not?
            var dot = storeType.IndexOf('.');
            enumDefinition = dot is -1
                ? _enumDefinitions.SingleOrDefault(m => m.StoreTypeName == storeType)
                : _enumDefinitions.SingleOrDefault(m => m.StoreTypeName == storeType[(dot + 1)..] && m.StoreTypeSchema == storeType[..dot]);
        }

        if (enumDefinition is null)
        {
            return null;
        }

        // We now have an enum definition from the context options.

        // We need the following store type names:
        // 1. The quoted type name is used in migrations, where quoting is needed
        // 2. The unquoted type name is set on KdbndpParameter.DataTypeName
        //    (though see https://github.com/npgsql/npgsql/issues/5710).
        var (name, schema) = (enumDefinition.StoreTypeName, enumDefinition.StoreTypeSchema);
        return new KdbndpEnumTypeMapping(
            _sqlGenerationHelper.DelimitIdentifier(name, schema),
            schema is null ? name : schema + "." + name,
            enumDefinition.ClrType,
            enumDefinition.Labels);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindUserRangeMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        UserRangeDefinition? rangeDefinition = null;
        var rangeStoreType = mappingInfo.StoreTypeName;
        var rangeClrType = mappingInfo.ClrType;

        // If the incoming MappingInfo contains a ClrType, make sure it's an KdbndpRange<T>, otherwise bail
        if (rangeClrType is not null && (!rangeClrType.IsGenericType || rangeClrType.GetGenericTypeDefinition() != typeof(KdbndpRange<>)))
        {
            return null;
        }

        // Try to find a user range definition (defined by the user on their context options), based on the
        // incoming MappingInfo's StoreType or ClrType
        if (rangeStoreType is not null)
        {
            rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.StoreTypeName == rangeStoreType);

            if (rangeDefinition is null)
            {
                return null;
            }

            if (rangeClrType is null)
            {
                // The incoming MappingInfo does not contain a ClrType, only a StoreType (i.e. scaffolding).
                // Construct the range ClrType from the range definition's subtype ClrType
                rangeClrType = typeof(KdbndpRange<>).MakeGenericType(rangeDefinition.SubtypeClrType);
            }
            else if (rangeClrType != typeof(KdbndpRange<>).MakeGenericType(rangeDefinition.SubtypeClrType))
            {
                // If the incoming MappingInfo also contains a ClrType (in addition to the StoreType), make sure it
                // corresponds to the subtype ClrType on the range definition
                return null;
            }
        }
        else if (rangeClrType is not null)
        {
            rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.SubtypeClrType == rangeClrType.GetGenericArguments()[0]);
        }

        if (rangeClrType is null || rangeDefinition is null)
        {
            return null;
        }

        // We now have a user-defined range definition from the context options. Use it to get the subtype's mapping
        var subtypeMapping = rangeDefinition.SubtypeName is null
            ? FindMapping(rangeDefinition.SubtypeClrType)
            : FindMapping(rangeDefinition.SubtypeName);

        if (subtypeMapping is null)
        {
            throw new Exception($"Could not map range {rangeDefinition.StoreTypeName}, no mapping was found its subtype");
        }

        // We need to store types for the user-defined range:
        // 1. The quoted type name is used in migrations, where quoting is needed
        // 2. The unquoted type name is set on KdbndpParameter.DataTypeName
        var quotedRangeStoreType = _sqlGenerationHelper.DelimitIdentifier(rangeDefinition.StoreTypeName, rangeDefinition.StoreTypeSchema);
        var unquotedRangeStoreType = rangeDefinition.StoreTypeSchema is null
            ? rangeDefinition.StoreTypeName
            : rangeDefinition.StoreTypeSchema + '.' + rangeDefinition.StoreTypeName;

        return KdbndpRangeTypeMapping.CreatUserDefinedRangeMapping(
            quotedRangeStoreType, unquotedRangeStoreType, rangeClrType, subtypeMapping);
    }

    /// <summary>
    ///     Finds the mapping for a container given its CLR type and its containee's type mapping; this is used when inferring type mappings
    ///     for arrays and ranges/multiranges.
    /// </summary>
    public virtual RelationalTypeMapping? FindContainerMapping(
        Type containerClrType,
        RelationalTypeMapping containeeTypeMapping,
        IModel model)
    {
        // Ranges aren't handled by the general FindMapping logic below, as we don't represent range type mappings as having an element
        // (they're not queryable).
        if (containerClrType.IsRange())
        {
            var rangeStoreType = containeeTypeMapping.StoreType switch
            {
                "int" or "integer" => "int4range",
                "bigint" => "int8range",
                "decimal" or "numeric" => "numrange",
                "date" => "daterange",
                "timestamp" or "timestamp without time zone" => "tsrange",
                "timestamptz" or "timestamp with time zone" => "tstzrange",
                _ => null
            };

            return rangeStoreType is null ? null : FindMapping(containerClrType, rangeStoreType);
        }

        // If this containment of a range within a multirange, just flow down to the general collection mapping logic; multiranges are
        // handled just like normal collections over ranges (since they can be unnested).
        // However, we also support containment of the base type (e.g. int) directly in its multirange (e.g. int4range). A multirange
        // is *not* a collection over the base type, so handle that specific case here.
        if (containerClrType.IsMultirange() && !containeeTypeMapping.ClrType.IsRange())
        {
            var multirangeStoreType = containeeTypeMapping.StoreType switch
            {
                "int" or "integer" => "int4multirange",
                "bigint" => "int8multirange",
                "decimal" or "numeric" => "nummultirange",
                "date" => "datemultirange",
                "timestamp" or "timestamp without time zone" => "tsmultirange",
                "timestamptz" or "timestamp with time zone" => "tstzmultirange",
                _ => null
            };

            return !_supportsMultiranges || multirangeStoreType is null ? null : FindMapping(containerClrType, multirangeStoreType);
        }

        // Then, try to find the mapping with the containee mapping as the element type mapping.
        // This is the standard EF lookup mechanism, and takes care of regular arrays and multiranges, which are supported as full primitive
        // collections.
        if (FindMapping(containerClrType, model, containeeTypeMapping) is RelationalTypeMapping containerMapping)
        {
            return containerMapping;
        }

        return null;
    }

    private static bool NameBasesUsesPrecision(ReadOnlySpan<char> span)
        => span.ToString() switch
        {
            "decimal" => true,
            "dec" => true,
            "numeric" => true,
            "timestamp" => true,
            "timestamptz" => true,
            "time" => true,
            "interval" => true,
            _ => false
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // We override to support parsing array store names (e.g. varchar(32)[]), timestamp(5) with time zone, etc.
    protected override string? ParseStoreTypeName(
        string? storeTypeName,
        ref bool? unicode,
        ref int? size,
        ref int? precision,
        ref int? scale)
    {
        if (storeTypeName is null)
        {
            return null;
        }

        var s = storeTypeName.AsSpan().Trim();

        // If this is an array store type, any facets (size, precision...) apply to the element and not to the array (e.g. varchar(32)[]
        // is an array mapping with Size=null over an element mapping of varchar with Size=32).
        if (s.EndsWith("[]", StringComparison.Ordinal))
        {
            return storeTypeName;
        }

        var openParen = s.IndexOf("(", StringComparison.Ordinal);
        if (openParen == -1)
        {
            return storeTypeName;
        }

        var preParens = s[..openParen].Trim();
        s = s.Slice(openParen + 1);
        var closeParen = s.IndexOf(")", StringComparison.Ordinal);
        if (closeParen == -1)
        {
            return storeTypeName;
        }

        var inParens = s[..closeParen].Trim();
        // There may be stuff after the closing parentheses (e.g. timestamp(3) with time zone)
        var postParens = s.Slice(closeParen + 1);

        switch (s.IndexOf(",", StringComparison.Ordinal))
        {
            // No comma inside the parentheses, parse the value either as size or precision
            case -1:
                if (!int.TryParse(inParens, out var p))
                {
                    return storeTypeName;
                }

                if (NameBasesUsesPrecision(preParens))
                {
                    precision = p;
                    scale = 0;
                }
                else
                {
                    size = p;
                }

                break;

            case var comma:
                if (int.TryParse(s[..comma].Trim(), out var parsedPrecision))
                {
                    precision = parsedPrecision;
                }
                else
                {
                    return storeTypeName;
                }

                if (int.TryParse(s[(comma + 1)..closeParen].Trim(), out var parsedScale))
                {
                    scale = parsedScale;
                }
                else
                {
                    return storeTypeName;
                }

                break;
        }

        if (postParens.Length == 0)
        {
            return preParens.Length == storeTypeName.Length
                ? storeTypeName
                : preParens.ToString();
        }

        return new StringBuilder(preParens.Length).Append(preParens).Append(postParens).ToString();
    }
}
