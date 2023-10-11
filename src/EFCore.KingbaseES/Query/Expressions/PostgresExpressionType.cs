namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;

/// <summary>
/// KingbaseES-specific expression node types.
/// </summary>
public enum PostgresExpressionType
{
    Contains,                     // >> (inet/cidr), @>
    ContainedBy,                  // << (inet/cidr), <@
    Overlaps,                     // &&

    AtTimeZone,                   // AT TIME ZONE

    NetworkContainedByOrEqual,    // <<=
    NetworkContainsOrEqual,       // >>=
    NetworkContainsOrContainedBy, // &&

    RangeIsStrictlyLeftOf,        // <<
    RangeIsStrictlyRightOf,       // >>
    RangeDoesNotExtendRightOf,    // &<
    RangeDoesNotExtendLeftOf,     // &>
    RangeIsAdjacentTo,            // -|-
    RangeUnion,                   // +
    RangeIntersect,               // *
    RangeExcept,                  // -

    TextSearchMatch,              // @@
    TextSearchAnd,                // &&
    TextSearchOr,                 // ||

    JsonExists,                   // ?
    JsonExistsAny,                // ?@>
    JsonExistsAll,                // ?<@

    LTreeMatches,                 // ~ or @
    LTreeMatchesAny,              // ?
    LTreeFirstAncestor,           // ?@>
    LTreeFirstDescendent,         // ?<@
    LTreeFirstMatches,            // ?~ or ?@

    PostgisDistanceKnn            // <->
}