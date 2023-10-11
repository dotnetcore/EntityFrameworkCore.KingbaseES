using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;
using Kdbndp.EntityFrameworkCore.KingbaseES.Utilities;
using KdbndpTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

public class KdbndpTypeMappingSource : RelationalTypeMappingSource
{
#if DEBUG
    internal static bool LegacyTimestampBehavior;
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
    private readonly ReferenceNullabilityDecoder _referenceNullabilityDecoder = new();

    protected virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
    protected virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

    private readonly IReadOnlyList<UserRangeDefinition> _userRangeDefinitions;

    /// <summary>
    /// Maps range subtypes to a list of type mappings for those ranges.
    /// </summary>
    private readonly Dictionary<Type, List<KdbndpRangeTypeMapping>> _rangeTypeMapings;

    /// <summary>
    /// Maps multirange subtypes to a list of type mappings for those multiranges.
    /// </summary>
    private readonly Dictionary<Type, List<KdbndpMultirangeTypeMapping>> _multirangeTypeMapings;

    private static MethodInfo? _adoUserTypeMappingsGetMethodInfo;

    private readonly bool _supportsMultiranges;

    #region Mappings

    // Numeric types
    private readonly KdbndpFloatTypeMapping        _float4             = new();
    private readonly KdbndpDoubleTypeMapping       _float8             = new();
    private readonly KdbndpDecimalTypeMapping      _numeric            = new();
    private readonly KdbndpBigIntegerTypeMapping   _bigInteger         = new();
    private readonly KdbndpDecimalTypeMapping      _numericAsFloat     = new(typeof(float));
    private readonly KdbndpDecimalTypeMapping      _numericAsDouble    = new(typeof(double));
    private readonly KdbndpMoneyTypeMapping        _money              = new();
    private readonly GuidTypeMapping               _uuid               = new("uuid", DbType.Guid);
    private readonly ShortTypeMapping              _int2               = new("smallint", DbType.Int16);
    private readonly ByteTypeMapping               _int2Byte           = new("smallint", DbType.Byte);
    private readonly IntTypeMapping                _int4               = new("integer", DbType.Int32);
    private readonly LongTypeMapping               _int8               = new("bigint", DbType.Int64);

    // Character types
    private readonly StringTypeMapping                _text               = new("text", DbType.String);
    private readonly KdbndpStringTypeMapping          _varchar            = new("character varying", KdbndpDbType.Varchar);
    private readonly KdbndpCharacterStringTypeMapping _char               = new("character");
    private readonly KdbndpCharacterCharTypeMapping   _singleChar         = new("character(1)");
    private readonly KdbndpStringTypeMapping          _xml                = new("xml", KdbndpDbType.Xml);
    private readonly KdbndpStringTypeMapping          _citext             = new("citext", KdbndpDbType.Citext);

    // JSON mappings
    private readonly KdbndpJsonTypeMapping         _jsonbString        = new("jsonb", typeof(string));
    private readonly KdbndpJsonTypeMapping         _jsonString         = new("json", typeof(string));
    private readonly KdbndpJsonTypeMapping         _jsonbDocument      = new("jsonb", typeof(JsonDocument));
    private readonly KdbndpJsonTypeMapping         _jsonDocument       = new("json", typeof(JsonDocument));
    private readonly KdbndpJsonTypeMapping         _jsonbElement       = new("jsonb", typeof(JsonElement));
    private readonly KdbndpJsonTypeMapping         _jsonElement        = new("json", typeof(JsonElement));

    // Date/Time types
    private readonly KdbndpDateTypeMapping         _dateDateTime       = new(typeof(DateTime));
    private readonly KdbndpTimestampTypeMapping    _timestamp          = new();
    private readonly KdbndpTimestampTzTypeMapping  _timestamptz        = new(typeof(DateTime));
    private readonly KdbndpTimestampTzTypeMapping  _timestamptzDto     = new(typeof(DateTimeOffset));
    private readonly KdbndpIntervalTypeMapping     _interval           = new();
    private readonly KdbndpTimeTypeMapping         _timeTimeSpan       = new(typeof(TimeSpan));
    private readonly KdbndpTimeTzTypeMapping       _timetz             = new();

    private readonly KdbndpDateTypeMapping         _dateDateOnly       = new(typeof(DateOnly));
    private readonly KdbndpTimeTypeMapping         _timeTimeOnly       = new(typeof(TimeOnly));

    // Network address types
    private readonly KdbndpMacaddrTypeMapping      _macaddr            = new();
    private readonly KdbndpMacaddr8TypeMapping     _macaddr8           = new();
    private readonly KdbndpInetTypeMapping         _inet               = new();
    private readonly KdbndpCidrTypeMapping         _cidr               = new();

    // Built-in geometric types
    private readonly KdbndpPointTypeMapping        _point              = new();
    private readonly KdbndpBoxTypeMapping          _box                = new();
    private readonly KdbndpLineTypeMapping         _line               = new();
    private readonly KdbndpLineSegmentTypeMapping  _lseg               = new();
    private readonly KdbndpPathTypeMapping         _path               = new();
    private readonly KdbndpPolygonTypeMapping      _polygon            = new();
    private readonly KdbndpCircleTypeMapping       _circle             = new();

    // uint mappings
    private readonly KdbndpUintTypeMapping         _xid                = new("xid", KdbndpDbType.Xid);
    private readonly KdbndpUintTypeMapping         _oid                = new("oid", KdbndpDbType.Oid);
    private readonly KdbndpUintTypeMapping         _cid                = new("cid", KdbndpDbType.Cid);
    private readonly KdbndpUintTypeMapping         _regtype            = new("regtype", KdbndpDbType.Regtype);
    private readonly KdbndpUintTypeMapping         _lo                 = new("lo", KdbndpDbType.Oid);

    // Full text search mappings
    private readonly KdbndpTsVectorTypeMapping  _tsvector              = new();
    private readonly KdbndpRegconfigTypeMapping _regconfig             = new();
    private readonly KdbndpTsRankingNormalizationTypeMapping _rankingNormalization = new();

    // Unaccent mapping
    private readonly KdbndpRegdictionaryTypeMapping _regdictionary = new();

    // Built-in ranges
    private readonly KdbndpRangeTypeMapping        _int4range;
    private readonly KdbndpRangeTypeMapping        _int8range;
    private readonly KdbndpRangeTypeMapping        _numrange;
    private readonly KdbndpRangeTypeMapping        _tsrange;
    private readonly KdbndpRangeTypeMapping        _tstzrange;
    private readonly KdbndpRangeTypeMapping        _dateOnlyDaterange;
    private readonly KdbndpRangeTypeMapping        _dateTimeDaterange;

    // Built-in multiranges
    private readonly KdbndpMultirangeTypeMapping _int4multirangeArray;
    private readonly KdbndpMultirangeTypeMapping _int8multirangeArray;
    private readonly KdbndpMultirangeTypeMapping _nummultirangeArray;
    private readonly KdbndpMultirangeTypeMapping _tsmultirangeArray;
    private readonly KdbndpMultirangeTypeMapping _tstzmultirangeArray;
    private readonly KdbndpMultirangeTypeMapping _dateTimeDatemultirangeArray;
    private readonly KdbndpMultirangeTypeMapping _dateOnlyDatemultirangeArray;

    private readonly KdbndpMultirangeTypeMapping _int4multirangeList;
    private readonly KdbndpMultirangeTypeMapping _int8multirangeList;
    private readonly KdbndpMultirangeTypeMapping _nummultirangeList;
    private readonly KdbndpMultirangeTypeMapping _tsmultirangeList;
    private readonly KdbndpMultirangeTypeMapping _tstzmultirangeList;
    private readonly KdbndpMultirangeTypeMapping _dateTimeMultirangeList;
    private readonly KdbndpMultirangeTypeMapping _dateOnlyDatemultirangeList;

    // Other types
    private readonly KdbndpBoolTypeMapping            _bool            = new();
    private readonly KdbndpBitTypeMapping             _bit             = new();
    private readonly KdbndpVarbitTypeMapping          _varbit          = new();
    private readonly KdbndpByteArrayTypeMapping       _bytea           = new();
    private readonly KdbndpHstoreTypeMapping          _hstore          = new(typeof(Dictionary<string, string>));
    private readonly KdbndpHstoreTypeMapping          _immutableHstore = new(typeof(ImmutableDictionary<string, string>));
    private readonly KdbndpTidTypeMapping             _tid             = new();

    // Special stuff
    // ReSharper disable once InconsistentNaming
    public readonly StringTypeMapping      EStringTypeMapping  = new KdbndpEStringTypeMapping();

    #endregion Mappings

    public KdbndpTypeMappingSource(TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
        : base(dependencies, relationalDependencies)
    {
        _sqlGenerationHelper = Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
        _supportsMultiranges = KdbndpSingletonOptions.PostgresVersionWithoutDefault is null
            || KdbndpSingletonOptions.PostgresVersionWithoutDefault.AtLeast(14);

        // Initialize some mappings which depend on other mappings
        _int4range         = new KdbndpRangeTypeMapping("int4range", typeof(KdbndpRange<int>),      _int4,         sqlGenerationHelper);
        _int8range         = new KdbndpRangeTypeMapping("int8range", typeof(KdbndpRange<long>),     _int8,         sqlGenerationHelper);
        _numrange          = new KdbndpRangeTypeMapping("numrange",  typeof(KdbndpRange<decimal>),  _numeric,      sqlGenerationHelper);
        _tsrange           = new KdbndpRangeTypeMapping("tsrange",   typeof(KdbndpRange<DateTime>), _timestamp,    sqlGenerationHelper);
        _tstzrange         = new KdbndpRangeTypeMapping("tstzrange", typeof(KdbndpRange<DateTime>), _timestamptz,  sqlGenerationHelper);
        _dateOnlyDaterange = new KdbndpRangeTypeMapping("daterange", typeof(KdbndpRange<DateOnly>), _dateDateOnly, sqlGenerationHelper);
        _dateTimeDaterange = new KdbndpRangeTypeMapping("daterange", typeof(KdbndpRange<DateTime>), _dateDateTime, sqlGenerationHelper);

        _rangeTypeMapings = new()
        {
            { typeof(int), new() { _int4range } },
            { typeof(long), new() { _int8range } },
            { typeof(decimal), new() { _numrange } },
            { typeof(DateOnly), new() { _dateOnlyDaterange } },
            { typeof(DateTime), new() { _tsrange, _tstzrange, _dateTimeDaterange } }
        };

        _int4multirangeArray         = new KdbndpMultirangeTypeMapping("int4multirange", typeof(KdbndpRange<int>[]),          _int4range,         sqlGenerationHelper);
        _int8multirangeArray         = new KdbndpMultirangeTypeMapping("int8multirange", typeof(KdbndpRange<long>[]),         _int8range,         sqlGenerationHelper);
        _nummultirangeArray          = new KdbndpMultirangeTypeMapping("nummultirange",  typeof(KdbndpRange<decimal>[]),      _numrange,          sqlGenerationHelper);
        _tsmultirangeArray           = new KdbndpMultirangeTypeMapping("tsmultirange",   typeof(KdbndpRange<DateTime>[]),     _tsrange,           sqlGenerationHelper);
        _tstzmultirangeArray         = new KdbndpMultirangeTypeMapping("tstzmultirange", typeof(KdbndpRange<DateTime>[]),     _tstzrange,         sqlGenerationHelper);
        _dateOnlyDatemultirangeArray = new KdbndpMultirangeTypeMapping("datemultirange", typeof(KdbndpRange<DateOnly>[]),     _dateOnlyDaterange, sqlGenerationHelper);
        _dateTimeDatemultirangeArray = new KdbndpMultirangeTypeMapping("datemultirange", typeof(KdbndpRange<DateTime>[]),     _dateTimeDaterange, sqlGenerationHelper);

        _int4multirangeList          = new KdbndpMultirangeTypeMapping("int4multirange", typeof(List<KdbndpRange<int>>),      _int4range,         sqlGenerationHelper);
        _int8multirangeList          = new KdbndpMultirangeTypeMapping("int8multirange", typeof(List<KdbndpRange<long>>),     _int8range,         sqlGenerationHelper);
        _nummultirangeList           = new KdbndpMultirangeTypeMapping("nummultirange",  typeof(List<KdbndpRange<decimal>>),  _numrange,          sqlGenerationHelper);
        _tsmultirangeList            = new KdbndpMultirangeTypeMapping("tsmultirange",   typeof(List<KdbndpRange<DateTime>>), _tsrange,           sqlGenerationHelper);
        _tstzmultirangeList          = new KdbndpMultirangeTypeMapping("tstzmultirange", typeof(List<KdbndpRange<DateTime>>), _tstzrange,         sqlGenerationHelper);
        _dateOnlyDatemultirangeList  = new KdbndpMultirangeTypeMapping("datemultirange", typeof(List<KdbndpRange<DateOnly>>), _dateOnlyDaterange, sqlGenerationHelper);
        _dateTimeMultirangeList      = new KdbndpMultirangeTypeMapping("datemultirange", typeof(List<KdbndpRange<DateTime>>), _dateTimeDaterange, sqlGenerationHelper);

        _multirangeTypeMapings = new()
        {
            { typeof(int), new() { _int4multirangeArray, _int4multirangeList } },
            { typeof(long), new() { _int8multirangeArray, _int8multirangeList } },
            { typeof(decimal), new() { _nummultirangeArray, _nummultirangeList } },
            { typeof(DateOnly), new() { _dateOnlyDatemultirangeArray, _dateOnlyDatemultirangeList } },
            {
                typeof(DateTime), new()
                {
                    _tsmultirangeArray, _tsmultirangeList,
                    _tstzmultirangeArray, _tstzmultirangeList,
                    _dateTimeDatemultirangeArray, _dateTimeMultirangeList
                }
            }
        };

// ReSharper disable CoVariantArrayConversion
        // Note that KingbaseES has aliases to some built-in type name aliases (e.g. int4 for integer),
        // these are mapped as well.
        // https://www.KingbaseES.org/docs/current/static/datatype.html#DATATYPE-TABLE
        var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "smallint",                    new RelationalTypeMapping[] { _int2, _int2Byte } },
            { "int2",                        new RelationalTypeMapping[] { _int2, _int2Byte } },
            { "integer",                     new[] { _int4                         } },
            { "int",                         new[] { _int4                         } },
            { "int4",                        new[] { _int4                         } },
            { "bigint",                      new[] { _int8                         } },
            { "int8",                        new[] { _int8                         } },
            { "real",                        new[] { _float4                       } },
            { "float4",                      new[] { _float4                       } },
            { "double precision",            new[] { _float8                       } },
            { "float8",                      new[] { _float8                       } },
            { "numeric",                     new RelationalTypeMapping[] { _numeric, _bigInteger, _numericAsFloat, _numericAsDouble } },
            { "decimal",                     new RelationalTypeMapping[] { _numeric, _bigInteger, _numericAsFloat, _numericAsDouble } },
            { "money",                       new[] { _money                        } },

            { "text",                        new[] { _text                         } },
            { "jsonb",                       new RelationalTypeMapping[] { _jsonbString, _jsonbDocument, _jsonbElement } },
            { "json",                        new RelationalTypeMapping[] { _jsonString, _jsonDocument, _jsonElement } },
            { "xml",                         new[] { _xml                          } },
            { "citext",                      new[] { _citext                       } },
            { "character varying",           new[] { _varchar                      } },
            { "varchar",                     new[] { _varchar                      } },
            // See FindBaseMapping below for special treatment of 'character'

            { "timestamp without time zone", new[] { _timestamp                    } },
            { "timestamp with time zone",    new[] { _timestamptz, _timestamptzDto } },
            { "interval",                    new[] { _interval                     } },
            { "date",                        new RelationalTypeMapping[] { _dateDateOnly, _dateDateTime } },
            { "time without time zone",      new RelationalTypeMapping[] { _timeTimeOnly, _timeTimeSpan } },
            { "time with time zone",         new[] { _timetz                       } },

            { "boolean",                     new[] { _bool                         } },
            { "bool",                        new[] { _bool                         } },
            { "bytea",                       new[] { _bytea                        } },
            { "uuid",                        new[] { _uuid                         } },
            { "bit",                         new[] { _bit                          } },
            { "bit varying",                 new[] { _varbit                       } },
            { "varbit",                      new[] { _varbit                       } },
            { "hstore",                      new RelationalTypeMapping[] { _hstore, _immutableHstore } },

            { "macaddr",                     new[] { _macaddr                      } },
            { "macaddr8",                    new[] { _macaddr8                     } },
            { "inet",                        new[] { _inet                         } },
            { "cidr",                        new[] { _cidr                         } },

            { "point",                       new[] { _point                        } },
            { "box",                         new[] { _box                          } },
            { "line",                        new[] { _line                         } },
            { "lseg",                        new[] { _lseg                         } },
            { "path",                        new[] { _path                         } },
            { "polygon",                     new[] { _polygon                      } },
            { "circle",                      new[] { _circle                       } },

            { "xid",                         new[] { _xid                          } },
            { "oid",                         new[] { _oid                          } },
            { "cid",                         new[] { _cid                          } },
            { "regtype",                     new[] { _regtype                      } },
            { "lo",                          new[] { _lo                           } },
            { "tid",                         new[] { _tid                          } },

            { "int4range",                   new[] { _int4range                    } },
            { "int8range",                   new[] { _int8range                    } },
            { "numrange",                    new[] { _numrange                     } },
            { "tsrange",                     new[] { _tsrange                      } },
            { "tstzrange",                   new[] { _tstzrange                    } },
            { "daterange",                   new[] { _dateOnlyDaterange, _dateTimeDaterange } },

            { "tsvector",                    new[] { _tsvector                     } },
            { "regconfig",                   new[] { _regconfig                    } },

            { "regdictionary",               new[] { _regdictionary                } }
        };
// ReSharper restore CoVariantArrayConversion

        // Set up aliases
        storeTypeMappings["timestamp"] = storeTypeMappings["timestamp without time zone"];
        storeTypeMappings["timestamptz"] = storeTypeMappings["timestamp with time zone"];
        storeTypeMappings["time"] = storeTypeMappings["time without time zone"];
        storeTypeMappings["timetz"] = storeTypeMappings["time with time zone"];

        var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(bool),                                _bool                 },
            { typeof(byte[]),                              _bytea                },
            { typeof(Guid),                                _uuid                 },

            { typeof(byte),                                _int2Byte             },
            { typeof(short),                               _int2                 },
            { typeof(int),                                 _int4                 },
            { typeof(long),                                _int8                 },
            { typeof(float),                               _float4               },
            { typeof(double),                              _float8               },
            { typeof(decimal),                             _numeric              },
            { typeof(BigInteger),                          _bigInteger           },

            { typeof(string),                              _text                 },
            { typeof(JsonDocument),                        _jsonbDocument        },
            { typeof(JsonElement),                         _jsonbElement         },
            { typeof(char),                                _singleChar           },

            { typeof(DateTime),                            LegacyTimestampBehavior ? _timestamp : _timestamptz },
            { typeof(DateOnly),                            _dateDateOnly         },
            { typeof(TimeOnly),                            _timeTimeOnly         },
            { typeof(TimeSpan),                            _interval             },
            { typeof(DateTimeOffset),                      _timestamptzDto       },

            { typeof(PhysicalAddress),                     _macaddr              },
            { typeof(IPAddress),                           _inet                 },
            { typeof((IPAddress, int)),                    _cidr                 },

            { typeof(BitArray),                            _varbit               },
            { typeof(ImmutableDictionary<string, string>), _immutableHstore      },
            { typeof(Dictionary<string, string>),          _hstore               },
            { typeof(KdbndpTid),                           _tid                  },

            { typeof(KdbndpPoint),                         _point                },
            { typeof(KdbndpBox),                           _box                  },
            { typeof(KdbndpLine),                          _line                 },
            { typeof(KdbndpLSeg),                          _lseg                 },
            { typeof(KdbndpPath),                          _path                 },
            { typeof(KdbndpPolygon),                       _polygon              },
            { typeof(KdbndpCircle),                        _circle               },

            { typeof(KdbndpRange<int>),                    _int4range            },
            { typeof(KdbndpRange<long>),                   _int8range            },
            { typeof(KdbndpRange<decimal>),                _numrange             },
            { typeof(KdbndpRange<DateTime>),               LegacyTimestampBehavior ? _tsrange : _tstzrange },
            { typeof(KdbndpRange<DateTimeOffset>),          _tstzrange           },
            { typeof(KdbndpRange<DateOnly>),               _dateOnlyDaterange },

            { typeof(KdbndpTsVector),                      _tsvector             },
            { typeof(KdbndpTsRankingNormalization),        _rankingNormalization },
        };

        if (_supportsMultiranges)
        {
            storeTypeMappings["int4multirange"] = new[] { _int4multirangeArray, _int4multirangeList };
            storeTypeMappings["int8multirange"] = new[] { _int8multirangeArray, _int8multirangeList };
            storeTypeMappings["nummultirange"] = new[] { _nummultirangeArray, _nummultirangeList   };
            storeTypeMappings["tsmultirange"] = new[] { _tsmultirangeArray, _tsmultirangeList     };
            storeTypeMappings["tstzmultirange"] = new[] { _tstzmultirangeArray, _tstzmultirangeList };
            storeTypeMappings["datemultirange"] = new[] { _dateOnlyDatemultirangeArray, _dateOnlyDatemultirangeList, _dateTimeDatemultirangeArray, _dateTimeMultirangeList };

            clrTypeMappings[typeof(KdbndpRange<int>[])] = _int4multirangeArray;
            clrTypeMappings[typeof(KdbndpRange<long>[])] = _int8multirangeArray;
            clrTypeMappings[typeof(KdbndpRange<decimal>[])] = _nummultirangeArray;
            clrTypeMappings[typeof(KdbndpRange<DateTime>[])] = LegacyTimestampBehavior ? _tsmultirangeArray : _tstzmultirangeArray;
            clrTypeMappings[typeof(KdbndpRange<DateOnly>[])] = _dateOnlyDatemultirangeArray;

            clrTypeMappings[typeof(List<KdbndpRange<int>>)] = _int4multirangeList;
            clrTypeMappings[typeof(List<KdbndpRange<long>>)] = _int8multirangeList;
            clrTypeMappings[typeof(List<KdbndpRange<decimal>>)] = _nummultirangeList;
            clrTypeMappings[typeof(List<KdbndpRange<DateTime>>)] = LegacyTimestampBehavior ? _tsmultirangeList : _tstzmultirangeList;
            clrTypeMappings[typeof(List<KdbndpRange<DateOnly>>)] = _dateOnlyDatemultirangeList;
        }

        StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
        ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

        LoadUserDefinedTypeMappings(sqlGenerationHelper);

        _userRangeDefinitions = KdbndpSingletonOptions?.UserRangeDefinitions ?? new UserRangeDefinition[0];
    }

    /// <summary>
    /// To be used in case user-defined mappings are added late, after this TypeMappingSource has already been initialized.
    /// This is basically only for test usage.
    /// </summary>
    public virtual void LoadUserDefinedTypeMappings(ISqlGenerationHelper sqlGenerationHelper)
        => SetupEnumMappings(sqlGenerationHelper);

    /// <summary>
    /// Gets all global enum mappings from the ADO.NET layer and creates mappings for them
    /// </summary>
    protected virtual void SetupEnumMappings(ISqlGenerationHelper sqlGenerationHelper)
    {
        _adoUserTypeMappingsGetMethodInfo ??= KdbndpConnection.GlobalTypeMapper.GetType().GetProperty("UserTypeMappings")?.GetMethod;

        if (_adoUserTypeMappingsGetMethodInfo is null)
        {
            return;
        }
    }

    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
        // First, try any plugins, allowing them to override built-in mappings (e.g. NodaTime)
        base.FindMapping(mappingInfo)
        ?? FindBaseMapping(mappingInfo)?.Clone(mappingInfo)
        ?? FindArrayMapping(mappingInfo)?.Clone(mappingInfo)
        ?? FindUserRangeMapping(mappingInfo);

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
                if (storeTypeName == "jsonb" || storeTypeName == "json")
                {
                    return new KdbndpJsonTypeMapping(storeTypeName, clrType);
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0].Clone(in mappingInfo);
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m.Clone(in mappingInfo);
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

            // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
            // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
        }

        if (clrType is null ||
            !ClrTypeMappings.TryGetValue(clrType, out var mapping) ||
            // Special case for byte[] mapped as smallint[] - don't return bytea mapping
            storeTypeName is not null && storeTypeName == "smallint[]")
        {
            return null;
        }

        if (mappingInfo.Size.HasValue)
        {
            if (clrType == typeof(string))
            {
                mapping = mappingInfo.IsFixedLength ?? false ? _char : _varchar;

                // See #342 for when size > 10485760
                return mappingInfo.Size <= 10485760
                    ? mapping.Clone($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size)
                    : _text;
            }

            if (clrType == typeof(BitArray))
            {
                mapping = mappingInfo.IsFixedLength ?? false ? (RelationalTypeMapping)_bit : _varbit;
                return mapping.Clone($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size);
            }
        }

        return mapping;
    }

    protected virtual RelationalTypeMapping? FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        Type? elementClrType = null;

        if (clrType is not null && !clrType.TryGetElementType(out elementClrType))
        {
            return null; // Not an array/list
        }

        var storeType = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
        if (storeType is not null)
        {
            // KingbaseES array type names are the element plus []
            if (!storeType.EndsWith("[]", StringComparison.Ordinal))
            {
                return null;
            }

            var elementStoreType = storeType.Substring(0, storeType.Length - 2);
            var elementStoreTypeNameBase = storeTypeNameBase!.Substring(0, storeTypeNameBase.Length - 2);

            RelationalTypeMapping? elementMapping;

            elementMapping = elementClrType is null
                ? FindMapping(new RelationalTypeMappingInfo(
                    elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.Precision, mappingInfo.Scale))
                : FindMapping(new RelationalTypeMappingInfo(
                    elementClrType, elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsKeyOrIndex, mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.IsRowVersion,
                    mappingInfo.IsFixedLength, mappingInfo.Precision, mappingInfo.Scale));

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            if (elementMapping is null || elementMapping is KdbndpArrayTypeMapping)
            {
                return null;
            }

            return clrType is null || clrType.IsArray
                ? new KdbndpArrayArrayTypeMapping(storeType, elementMapping)
                : new KdbndpArrayListTypeMapping(storeType, elementMapping);
        }

        if (clrType is null)
        {
            return null;
        }

        if (clrType.IsArray)
        {
            var elementType = clrType.GetElementType();
            Debug.Assert(elementType is not null, "Detected array type but element type is null");

            var elementMapping = (RelationalTypeMapping?)FindMapping(elementType);

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            if (elementMapping is null || elementMapping is KdbndpArrayTypeMapping)
            {
                return null;
            }

            // Not that the element mapping found above was stripped of nullability
            // (so we get a mapping for int, not int?).
            Debug.Assert(
                Nullable.GetUnderlyingType(elementType) is null ||
                Nullable.GetUnderlyingType(elementType) == elementMapping.ClrType);

            return new KdbndpArrayArrayTypeMapping(clrType, elementMapping);
        }

        if (clrType.IsGenericList())
        {
            var elementType = clrType.GetGenericArguments()[0];

            // If an element isn't supported, neither is its array
            var elementMapping = (RelationalTypeMapping?)FindMapping(elementType);
            if (elementMapping is null)
            {
                return null;
            }

            // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by KingbaseES
            if (elementMapping is KdbndpArrayTypeMapping)
            {
                return null;
            }

            return new KdbndpArrayListTypeMapping(clrType, elementMapping);
        }

        return null;
    }

    protected virtual RelationalTypeMapping? FindUserRangeMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        UserRangeDefinition? rangeDefinition = null;
        var rangeStoreType = mappingInfo.StoreTypeName;
        var rangeClrType = mappingInfo.ClrType;

        // If the incoming MappingInfo contains a ClrType, make sure it's an KdbndpRange<T>, otherwise bail
        if (rangeClrType is not null &&
            (!rangeClrType.IsGenericType || rangeClrType.GetGenericTypeDefinition() != typeof(KdbndpRange<>)))
        {
            return null;
        }

        // Try to find a user range definition (defined by the user on their context options), based on the
        // incoming MappingInfo's StoreType or ClrType
        if (rangeStoreType is not null)
        {
            rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.RangeName == rangeStoreType);

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
        var subtypeMapping = (RelationalTypeMapping?)(rangeDefinition.SubtypeName is null
            ? FindMapping(rangeDefinition.SubtypeClrType)
            : FindMapping(rangeDefinition.SubtypeName));

        if (subtypeMapping is null)
        {
            throw new Exception($"Could not map range {rangeDefinition.RangeName}, no mapping was found its subtype");
        }

        return new KdbndpRangeTypeMapping(rangeDefinition.RangeName, rangeDefinition.SchemaName, rangeClrType, subtypeMapping, _sqlGenerationHelper);
    }

    /// <summary>
    /// Finds the mapping for a container given its CLR type and its containee's type mapping; this is currently used to infer type
    /// mappings for ranges and multiranges from their values.
    /// </summary>
    public virtual RelationalTypeMapping? FindContainerMapping(Type containerClrType, RelationalTypeMapping containeeTypeMapping)
    {
        if (containerClrType.TryGetRangeSubtype(out var subtypeType))
        {
            return _rangeTypeMapings.TryGetValue(subtypeType, out var candidateMappings)
                ? candidateMappings.FirstOrDefault(m => m.SubtypeMapping.StoreType == containeeTypeMapping.StoreType)
                : null;
        }

        if (_supportsMultiranges && containerClrType.TryGetMultirangeSubtype(out subtypeType))
        {
            return _multirangeTypeMapings.TryGetValue(subtypeType, out var candidateMappings)
                ? candidateMappings.FirstOrDefault(m => m.SubtypeMapping.StoreType == containeeTypeMapping.StoreType)
                : null;
        }

        return null;
    }

    private static bool NameBasesUsesPrecision(ReadOnlySpan<char> span)
        => span.ToString() switch
        {
            "decimal"     => true,
            "dec"         => true,
            "numeric"     => true,
            "timestamp"   => true,
            "timestamptz" => true,
            "time"        => true,
            "interval"    => true,
            _             => false
        };

    // We override to support parsing array store names (e.g. varchar(32)[]), timestamp(5) with time zone, etc.
    protected override string? ParseStoreTypeName(
        string? storeTypeName,
        out bool? unicode,
        out int? size,
        out int? precision,
        out int? scale)
    {
        (unicode, size, precision, scale) = (null, null, null, null);

        if (storeTypeName is null)
        {
            return null;
        }

        var span = storeTypeName.AsSpan().Trim();

        var openParen = span.IndexOf("(", StringComparison.Ordinal);
        if (openParen == -1)
        {
            return storeTypeName;
        }

        var afterOpenParen = span.Slice(openParen + 1).TrimStart();
        var closeParen = afterOpenParen.IndexOf(")", StringComparison.Ordinal);
        if (closeParen == -1)
        {
            return storeTypeName;
        }

        var preParens = span[..openParen].Trim();
        var inParens = afterOpenParen[..closeParen].Trim();
        // There may be stuff after the closing parentheses (e.g. varchar(32)[], timestamp(3) with time zone)
        var postParens = afterOpenParen.Slice(closeParen + 1);

        var comma = inParens.IndexOf(",", StringComparison.Ordinal);
        if (comma != -1)
        {
            if (int.TryParse(inParens[..comma].Trim(), out var parsedPrecision))
            {
                precision = parsedPrecision;
            }

            if (int.TryParse(inParens.Slice(comma + 1), out var parsedScale))
            {
                scale = parsedScale;
            }
        }
        else if (int.TryParse(inParens, out var parsedSize))
        {
            if (NameBasesUsesPrecision(preParens))
            {
                precision = parsedSize;
                scale = 0;
            }
            else
            {
                size = parsedSize;
            }
        }
        else
        {
            return storeTypeName;
        }

        if (postParens.Length > 0)
        {
            return new StringBuilder()
                .Append(preParens)
                .Append(postParens)
                .ToString();
        }

        return preParens.ToString();
    }

    public override CoreTypeMapping? FindMapping(IProperty property)
    {
        var mapping = base.FindMapping(property);

        // For arrays over reference types, the CLR type doesn't convey nullability (unlike with arrays over value types).
        // We decode NRT annotations here to return the correct type mapping.
        if (mapping is KdbndpArrayTypeMapping arrayMapping
            && !arrayMapping.ElementMapping.ClrType.IsValueType
            && !property.IsShadowProperty()
            && property.GetMemberInfo(forMaterialization: false, forSet: false) is MemberInfo memberInfo
            && memberInfo.GetMemberType().IsArrayOrGenericList())
        {
            if (_referenceNullabilityDecoder.IsArrayOrListElementNonNullable(memberInfo))
            {
                return arrayMapping.MakeNonNullable();
            }
        }

        return mapping;
    }
}
