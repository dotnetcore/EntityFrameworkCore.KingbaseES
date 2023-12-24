namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpFuzzyStringMatchMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> Functions = new()
    {
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchSoundex), typeof(DbFunctions), typeof(string))]
            = "soundex",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDifference), typeof(DbFunctions), typeof(string), typeof(string))]
            = "difference",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshtein), typeof(DbFunctions), typeof(string), typeof(string))]
            = "levenshtein",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshtein), typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int))]
            = "levenshtein",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshteinLessEqual), typeof(DbFunctions), typeof(string), typeof(string), typeof(int))]
            = "levenshtein_less_equal",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshteinLessEqual), typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int))]
            = "levenshtein_less_equal",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchMetaphone), typeof(DbFunctions), typeof(string), typeof(int))]
            = "metaphone",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDoubleMetaphone), typeof(DbFunctions), typeof(string))]
            = "dmetaphone",
        [GetRuntimeMethod(nameof(KdbndpFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDoubleMetaphoneAlt), typeof(DbFunctions), typeof(string))]
            = "dmetaphone_alt"
    };

    private static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
        => typeof(KdbndpFuzzyStringMatchDbFunctionsExtensions).GetRuntimeMethod(name, parameters)!;

    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;

    private static readonly bool[][] TrueArrays =
    {
        Array.Empty<bool>(),
        new[] { true },
        new[] { true, true },
        new[] { true, true, true },
        new[] { true, true, true, true },
        new[] { true, true, true, true, true },
        new[] { true, true, true, true, true, true }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpFuzzyStringMatchMethodTranslator(KdbndpSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => Functions.TryGetValue(method, out var function)
            ? _sqlExpressionFactory.Function(
                function,
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                method.ReturnType)
            : null;
}
