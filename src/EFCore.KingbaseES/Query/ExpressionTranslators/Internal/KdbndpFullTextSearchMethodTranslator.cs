using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translations for KingbaseES full-text search methods.
/// </summary>
public class KdbndpFullTextSearchMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo TsQueryParse =
        typeof(KdbndpTsQuery).GetMethod(nameof(KdbndpTsQuery.Parse), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo TsVectorParse =
        typeof(KdbndpTsVector).GetMethod(nameof(KdbndpTsVector.Parse), BindingFlags.Public | BindingFlags.Static)!;

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly IModel _model;
    private readonly RelationalTypeMapping _tsQueryMapping;
    private readonly RelationalTypeMapping _tsVectorMapping;
    private readonly RelationalTypeMapping _regconfigMapping;
    private readonly RelationalTypeMapping _regdictionaryMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpFullTextSearchMethodTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _model = model;
        _tsQueryMapping = typeMappingSource.FindMapping("tsquery")!;
        _tsVectorMapping = typeMappingSource.FindMapping("tsvector")!;
        _regconfigMapping = typeMappingSource.FindMapping("regconfig")!;
        _regdictionaryMapping = typeMappingSource.FindMapping("regdictionary")!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == TsQueryParse || method == TsVectorParse)
        {
            return _sqlExpressionFactory.Convert(arguments[0], method.ReturnType);
        }

        if (method.DeclaringType == typeof(KdbndpFullTextSearchDbFunctionsExtensions))
        {
            return method.Name switch
            {
                // Methods accepting a configuration (regconfig)
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.ToTsVector) when arguments.Count == 3
                    => ConfigAccepting("to_tsvector"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.PlainToTsQuery) when arguments.Count == 3
                    => ConfigAccepting("plainto_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.PhraseToTsQuery) when arguments.Count == 3
                    => ConfigAccepting("phraseto_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.ToTsQuery) when arguments.Count == 3
                    => ConfigAccepting("to_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.WebSearchToTsQuery) when arguments.Count == 3
                    => ConfigAccepting("websearch_to_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.Unaccent) when arguments.Count == 3
                    => DictionaryAccepting("unaccent"),

                // Methods not accepting a configuration
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.ArrayToTsVector)
                    => NonConfigAccepting("array_to_tsvector"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.ToTsVector)
                    => NonConfigAccepting("to_tsvector"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.PlainToTsQuery)
                    => NonConfigAccepting("plainto_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.PhraseToTsQuery)
                    => NonConfigAccepting("phraseto_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.ToTsQuery)
                    => NonConfigAccepting("to_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.WebSearchToTsQuery)
                    => NonConfigAccepting("websearch_to_tsquery"),
                nameof(KdbndpFullTextSearchDbFunctionsExtensions.Unaccent)
                    => NonConfigAccepting("unaccent"),

                _ => null
            };
        }

        if (method.DeclaringType == typeof(KdbndpFullTextSearchLinqExtensions))
        {
            if (method.Name is
                nameof(KdbndpFullTextSearchLinqExtensions.Rank) or nameof(KdbndpFullTextSearchLinqExtensions.RankCoverDensity))
            {
                var rankFunctionName = method.Name == nameof(KdbndpFullTextSearchLinqExtensions.Rank) ? "ts_rank" : "ts_rank_cd";

                return arguments.Count switch
                {
                    2 => _sqlExpressionFactory.Function(
                        rankFunctionName,
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        typeof(float),
                        _typeMappingSource.FindMapping(typeof(float), _model)),

                    3 => _sqlExpressionFactory.Function(
                        rankFunctionName,
                        new[]
                        {
                            arguments[1].Type == typeof(float[]) ? arguments[1] : arguments[0],
                            arguments[1].Type == typeof(float[]) ? arguments[0] : arguments[1],
                            arguments[2]
                        },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        typeof(float),
                        _typeMappingSource.FindMapping(typeof(float), _model)),

                    4 => _sqlExpressionFactory.Function(
                        rankFunctionName,
                        new[] { arguments[1], arguments[0], arguments[2], arguments[3] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[4],
                        method.ReturnType,
                        _typeMappingSource.FindMapping(typeof(float), _model)),

                    _ => throw new ArgumentException($"Invalid method overload for {rankFunctionName}")
                };
            }

            if (method.Name == nameof(KdbndpFullTextSearchLinqExtensions.SetWeight))
            {
                var newArgs = new List<SqlExpression>(arguments);
                if (newArgs[1].Type == typeof(KdbndpTsVector.Lexeme.Weight))
                {
                    newArgs[1] = newArgs[1] is SqlConstantExpression weightExpression
                        ? _sqlExpressionFactory.Constant(weightExpression.Value!.ToString()![0])
                        : throw new ArgumentException("Enum 'weight' argument for 'SetWeight' must be a constant expression.");
                }

                return _sqlExpressionFactory.Function(
                    "setweight",
                    newArgs,
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType);
            }

            return method.Name switch
            {
                // Operators

                nameof(KdbndpFullTextSearchLinqExtensions.And)
                    => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.TextSearchAnd, arguments[0], arguments[1]),
                nameof(KdbndpFullTextSearchLinqExtensions.Or)
                    => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.TextSearchOr, arguments[0], arguments[1]),

                nameof(KdbndpFullTextSearchLinqExtensions.ToNegative)
                    => new SqlUnaryExpression(
                        ExpressionType.Not, arguments[0], arguments[0].Type,
                        arguments[0].TypeMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.Contains)
                    => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
                nameof(KdbndpFullTextSearchLinqExtensions.IsContainedIn)
                    => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),

                nameof(KdbndpFullTextSearchLinqExtensions.Concat)
                    => _sqlExpressionFactory.Add(arguments[0], arguments[1], _tsVectorMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.Matches)
                    => _sqlExpressionFactory.MakePostgresBinary(
                        PgExpressionType.TextSearchMatch,
                        arguments[0],
                        arguments[1].Type == typeof(string)
                            ? _sqlExpressionFactory.Function(
                                "plainto_tsquery",
                                new[] { arguments[1] },
                                nullable: true,
                                argumentsPropagateNullability: TrueArrays[1],
                                typeof(KdbndpTsQuery),
                                _tsQueryMapping)
                            : arguments[1]),

                // Functions

                nameof(KdbndpFullTextSearchLinqExtensions.GetNodeCount)
                    => _sqlExpressionFactory.Function(
                        "numnode",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int),
                        _typeMappingSource.FindMapping(method.ReturnType, _model)),

                nameof(KdbndpFullTextSearchLinqExtensions.GetQueryTree)
                    => _sqlExpressionFactory.Function(
                        "querytree",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(string),
                        _typeMappingSource.FindMapping(method.ReturnType, _model)),

                nameof(KdbndpFullTextSearchLinqExtensions.GetResultHeadline) when arguments.Count == 2
                    => _sqlExpressionFactory.Function(
                        "ts_headline",
                        new[] { arguments[1], arguments[0] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),

                nameof(KdbndpFullTextSearchLinqExtensions.GetResultHeadline) when arguments.Count == 3 =>
                    _sqlExpressionFactory.Function(
                        "ts_headline",
                        new[] { arguments[1], arguments[0], arguments[2] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        method.ReturnType),

                nameof(KdbndpFullTextSearchLinqExtensions.GetResultHeadline) when arguments.Count == 4 =>
                    _sqlExpressionFactory.Function(
                        "ts_headline",
                        new[]
                        {
                            // For the regconfig parameter, if a constant string was provided, just pass it as a string - regconfig-accepting functions
                            // will implicitly cast to regconfig. For (string!) parameters, we add an explicit cast, since regconfig actually is an OID
                            // behind the scenes, and for parameter binary transfer no type coercion occurs.
                            arguments[1] is SqlConstantExpression constant
                                ? _sqlExpressionFactory.ApplyDefaultTypeMapping(constant)
                                : _sqlExpressionFactory.Convert(arguments[1], typeof(string), _regconfigMapping),
                            arguments[2],
                            arguments[0],
                            arguments[3]
                        },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[4],
                        method.ReturnType),

                nameof(KdbndpFullTextSearchLinqExtensions.Rewrite) when arguments.Count == 2
                    => _sqlExpressionFactory.Function(
                        "ts_rewrite",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        typeof(KdbndpTsQuery),
                        _tsQueryMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.Rewrite) when arguments.Count == 3
                    => _sqlExpressionFactory.Function(
                        "ts_rewrite",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        typeof(KdbndpTsQuery),
                        _tsQueryMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.ToPhrase)
                    => _sqlExpressionFactory.Function(
                        "tsquery_phrase",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[arguments.Count],
                        typeof(KdbndpTsQuery),
                        _tsQueryMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.Delete)
                    => _sqlExpressionFactory.Function(
                        "ts_delete",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType,
                        _tsVectorMapping),

                // TODO: Here we need to cast the char[] array we got into a "char"[] internal array...
                nameof(KdbndpFullTextSearchLinqExtensions.Filter)
                    => throw new NotImplementedException(),
                //=> _sqlExpressionFactory.Function("ts_filter", arguments, typeof(KdbndpTsVector), _tsVectorMapping),

                nameof(KdbndpFullTextSearchLinqExtensions.GetLength)
                    => _sqlExpressionFactory.Function(
                        "length",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        method.ReturnType,
                        _typeMappingSource.FindMapping(typeof(int), _model)),

                nameof(KdbndpFullTextSearchLinqExtensions.ToStripped)
                    => _sqlExpressionFactory.Function(
                        "strip",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[arguments.Count],
                        method.ReturnType,
                        _tsVectorMapping),

                _ => null
            };
        }

        return null;

        SqlExpression ConfigAccepting(string functionName)
            => _sqlExpressionFactory.Function(
                functionName, new[]
                {
                    // For the regconfig parameter, if a constant string was provided, just pass it as a string - regconfig-accepting functions
                    // will implicitly cast to regconfig. For (string!) parameters, we add an explicit cast, since regconfig actually is an OID
                    // behind the scenes, and for parameter binary transfer no type coercion occurs.
                    arguments[1] is SqlConstantExpression constant
                        ? _sqlExpressionFactory.ApplyDefaultTypeMapping(constant)
                        : _sqlExpressionFactory.Convert(arguments[1], typeof(string), _regconfigMapping),
                    arguments[2]
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count],
                method.ReturnType,
                _typeMappingSource.FindMapping(method.ReturnType, _model));

        SqlExpression DictionaryAccepting(string functionName)
            => _sqlExpressionFactory.Function(
                functionName, new[]
                {
                    // For the regdictionary parameter, if a constant string was provided, just pass it as a string - regdictionary-accepting functions
                    // will implicitly cast to regdictionary. For (string!) parameters, we add an explicit cast, since regdictionary actually is an OID
                    // behind the scenes, and for parameter binary transfer no type coercion occurs.
                    arguments[1] is SqlConstantExpression constant
                        ? _sqlExpressionFactory.ApplyDefaultTypeMapping(constant)
                        : _sqlExpressionFactory.Convert(arguments[1], typeof(string), _regdictionaryMapping),
                    arguments[2]
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count],
                method.ReturnType,
                _typeMappingSource.FindMapping(method.ReturnType, _model));

        SqlExpression NonConfigAccepting(string functionName)
            => _sqlExpressionFactory.Function(
                functionName,
                new[] { arguments[1] },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count],
                method.ReturnType,
                _typeMappingSource.FindMapping(method.ReturnType, _model));
    }
}
