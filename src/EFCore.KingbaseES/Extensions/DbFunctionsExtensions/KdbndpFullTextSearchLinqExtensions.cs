using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides EF Core extension methods for Kdbndp full-text search types.
/// </summary>
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
public static class KdbndpFullTextSearchLinqExtensions
{
    /// <summary>
    ///     AND tsquerys together. Generates the "&amp;&amp;" operator.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery And(this KdbndpTsQuery query1, KdbndpTsQuery query2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(And)));

    /// <summary>
    ///     OR tsquerys together. Generates the "||" operator.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery Or(this KdbndpTsQuery query1, KdbndpTsQuery query2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(Or)));

    /// <summary>
    ///     Negate a tsquery. Generates the "!!" operator.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery ToNegative(this KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(ToNegative)));

    /// <summary>
    ///     Returns whether <paramref name="query1" /> contains <paramref name="query2" />.
    ///     Generates the "@&gt;" operator.
    ///     http://www.KingbaseES.org/docs/current/static/functions-textsearch.html
    /// </summary>
    public static bool Contains(this KdbndpTsQuery query1, KdbndpTsQuery query2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(Contains)));

    /// <summary>
    ///     Returns whether <paramref name="query1" /> is contained within <paramref name="query2" />.
    ///     Generates the "&lt;@" operator.
    ///     http://www.KingbaseES.org/docs/current/static/functions-textsearch.html
    /// </summary>
    public static bool IsContainedIn(this KdbndpTsQuery query1, KdbndpTsQuery query2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(IsContainedIn)));

    /// <summary>
    ///     Returns the number of lexemes plus operators in <paramref name="query" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static int GetNodeCount(this KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(GetNodeCount)));

    /// <summary>
    ///     Get the indexable part of <paramref name="query" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static string GetQueryTree(this KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(GetQueryTree)));

    /// <summary>
    ///     Returns a string suitable for display containing a query match.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
    /// </summary>
    public static string GetResultHeadline(this KdbndpTsQuery query, string document)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(GetResultHeadline)));

    /// <summary>
    ///     Returns a string suitable for display containing a query match.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
    /// </summary>
    public static string GetResultHeadline(this KdbndpTsQuery query, string document, string options)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(GetResultHeadline)));

    /// <summary>
    ///     Returns a string suitable for display containing a query match using the text
    ///     search configuration specified by <paramref name="config" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
    /// </summary>
    public static string GetResultHeadline(this KdbndpTsQuery query, string config, string document, string options)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(GetResultHeadline)));

    /// <summary>
    ///     Searches <paramref name="query" /> for occurrences of <paramref name="target" />, and replaces
    ///     each occurrence with a <paramref name="substitute" />. All parameters are of type tsquery.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery Rewrite(this KdbndpTsQuery query, KdbndpTsQuery target, KdbndpTsQuery substitute)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(Rewrite)));

    /// <summary>
    ///     For each row of the SQL <paramref name="select" /> result, occurrences of the first column value (the target)
    ///     are replaced by the second column value (the substitute) within the current <paramref name="query" /> value.
    ///     The <paramref name="select" /> must yield two columns of tsquery type.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery Rewrite(this KdbndpTsQuery query, string select)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(Rewrite)));

    /// <summary>
    ///     Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
    ///     to <paramref name="query2" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery ToPhrase(this KdbndpTsQuery query1, KdbndpTsQuery query2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(ToPhrase)));

    /// <summary>
    ///     Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
    ///     to <paramref name="query2" /> at a distance of <paramref name="distance" /> lexemes using
    ///     the &lt;N&gt; tsquery operator
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
    /// </summary>
    public static KdbndpTsQuery ToPhrase(this KdbndpTsQuery query1, KdbndpTsQuery query2, int distance)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsQuery) + "." + nameof(ToPhrase)));

    /// <summary>
    ///     This method generates the "@@" match operator. The <paramref name="query" /> parameter is
    ///     assumed to be a plain search query and will be converted to a tsquery using plainto_tsquery.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
    /// </summary>
    public static bool Matches(this KdbndpTsVector vector, string query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Matches)));

    /// <summary>
    ///     This method generates the "@@" match operator.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
    /// </summary>
    public static bool Matches(this KdbndpTsVector vector, KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Matches)));

    /// <summary>
    ///     Returns a vector which combines the lexemes and positional information of <paramref name="vector1" />
    ///     and <paramref name="vector2" /> using the || tsvector operator. Positions and weight labels are retained
    ///     during the concatenation.
    ///     https://www.KingbaseES.org/docs/10/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector Concat(this KdbndpTsVector vector1, KdbndpTsVector vector2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Concat)));

    /// <summary>
    ///     Assign weight to each element of <paramref name="vector" /> and return a new
    ///     weighted tsvector.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector SetWeight(this KdbndpTsVector vector, KdbndpTsVector.Lexeme.Weight weight)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(SetWeight)));

    /// <summary>
    ///     Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
    ///     return a new weighted tsvector.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector SetWeight(this KdbndpTsVector vector, KdbndpTsVector.Lexeme.Weight weight, string[] lexemes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(SetWeight)));

    /// <summary>
    ///     Assign weight to each element of <paramref name="vector" /> and return a new
    ///     weighted tsvector.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector SetWeight(this KdbndpTsVector vector, char weight)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(SetWeight)));

    /// <summary>
    ///     Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
    ///     return a new weighted tsvector.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector SetWeight(this KdbndpTsVector vector, char weight, string[] lexemes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(SetWeight)));

    /// <summary>
    ///     Return a new vector with <paramref name="lexeme" /> removed from <paramref name="vector" />
    ///     https://www.KingbaseES.org/docs/current/static/functions-textsearch.html
    /// </summary>
    public static KdbndpTsVector Delete(this KdbndpTsVector vector, string lexeme)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Delete)));

    /// <summary>
    ///     Return a new vector with <paramref name="lexemes" /> removed from <paramref name="vector" />
    ///     https://www.KingbaseES.org/docs/current/static/functions-textsearch.html
    /// </summary>
    public static KdbndpTsVector Delete(this KdbndpTsVector vector, string[] lexemes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Delete)));

    /// <summary>
    ///     Returns a new vector with only lexemes having weights specified in <paramref name="weights" />.
    ///     https://www.KingbaseES.org/docs/current/static/functions-textsearch.html
    /// </summary>
    public static KdbndpTsVector Filter(this KdbndpTsVector vector, char[] weights)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Filter)));

    /// <summary>
    ///     Returns the number of lexemes in <paramref name="vector" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static int GetLength(this KdbndpTsVector vector)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(GetLength)));

    /// <summary>
    ///     Removes weights and positions from <paramref name="vector" /> and returns
    ///     a new stripped tsvector.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
    /// </summary>
    public static KdbndpTsVector ToStripped(this KdbndpTsVector vector)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(ToStripped)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float Rank(this KdbndpTsVector vector, KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Rank)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
    ///     the result according to the behaviors specified by <paramref name="normalization" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float Rank(this KdbndpTsVector vector, KdbndpTsQuery query, KdbndpTsRankingNormalization normalization)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Rank)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> with custom
    ///     weighting for word instances depending on their labels (D, C, B or A).
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float Rank(this KdbndpTsVector vector, float[] weights, KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Rank)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
    ///     the result according to the behaviors specified by <paramref name="normalization" />
    ///     and using custom weighting for word instances depending on their labels (D, C, B or A).
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float Rank(
        this KdbndpTsVector vector,
        float[] weights,
        KdbndpTsQuery query,
        KdbndpTsRankingNormalization normalization)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(Rank)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
    ///     density method.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float RankCoverDensity(this KdbndpTsVector vector, KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(RankCoverDensity)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
    ///     density method while normalizing the result according to the behaviors specified by
    ///     <paramref name="normalization" />.
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float RankCoverDensity(this KdbndpTsVector vector, KdbndpTsQuery query, KdbndpTsRankingNormalization normalization)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(RankCoverDensity)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
    ///     density method with custom weighting for word instances depending on their labels (D, C, B or A).
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float RankCoverDensity(this KdbndpTsVector vector, float[] weights, KdbndpTsQuery query)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(RankCoverDensity)));

    /// <summary>
    ///     Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover density
    ///     method while normalizing the result according to the behaviors specified by <paramref name="normalization" />
    ///     and using custom weighting for word instances depending on their labels (D, C, B or A).
    ///     http://www.KingbaseES.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
    /// </summary>
    public static float RankCoverDensity(
        this KdbndpTsVector vector,
        float[] weights,
        KdbndpTsQuery query,
        KdbndpTsRankingNormalization normalization)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(KdbndpTsVector) + "." + nameof(RankCoverDensity)));
}
