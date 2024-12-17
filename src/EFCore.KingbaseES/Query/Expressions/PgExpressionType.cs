namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;

/// <summary>
///     KingbaseES-specific expression node types.
/// </summary>
public enum PgExpressionType
{
    #region General operators

    /// <summary>
    ///     Represents a KingbaseES contains operator.
    /// </summary>
    Contains, // >> (inet/cidr), @>

    /// <summary>
    ///     Represents a KingbaseES contained-by operator.
    /// </summary>
    ContainedBy, // << (inet/cidr), <@

    /// <summary>
    ///     Represents a KingbaseES overlap operator.
    /// </summary>
    Overlaps, // &&

    /// <summary>
    ///     Represents a KingbaseES operator for finding the distance between two things (e.g. 2D distance between two geometries,
    ///     between timestamps...)
    /// </summary>
    Distance, // <->

    #endregion General operators

    #region Network

    /// <summary>
    ///     Represents a KingbaseES network contained-by-or-equal operator.
    /// </summary>
    NetworkContainedByOrEqual, // <<=

    /// <summary>
    ///     Represents a KingbaseES network contains-or-equal operator.
    /// </summary>
    NetworkContainsOrEqual, // >>=

    /// <summary>
    ///     Represents a KingbaseES network contains-or-contained-by operator.
    /// </summary>
    NetworkContainsOrContainedBy, // &&

    #endregion Network

    #region Range

    /// <summary>
    ///     Represents a KingbaseES operator for checking if a range is strictly to the left of another range.
    /// </summary>
    RangeIsStrictlyLeftOf, // <<

    /// <summary>
    ///     Represents a KingbaseES operator for checking if a range is strictly to the right of another range.
    /// </summary>
    RangeIsStrictlyRightOf, // >>

    /// <summary>
    ///     Represents a KingbaseES operator for checking if a range does not extend to the right of another range.
    /// </summary>
    RangeDoesNotExtendRightOf, // &<

    /// <summary>
    ///     Represents a KingbaseES operator for checking if a range does not extend to the left of another range.
    /// </summary>
    RangeDoesNotExtendLeftOf, // &>

    /// <summary>
    ///     Represents a KingbaseES operator for checking if a range is adjacent to another range.
    /// </summary>
    RangeIsAdjacentTo, // -|-

    /// <summary>
    ///     Represents a KingbaseES operator for performing a union between two ranges.
    /// </summary>
    RangeUnion, // +

    /// <summary>
    ///     Represents a KingbaseES operator for performing an intersection between two ranges.
    /// </summary>
    RangeIntersect, // *

    /// <summary>
    ///     Represents a KingbaseES operator for performing an except operation between two ranges.
    /// </summary>
    RangeExcept, // -

    #endregion Range

    #region Text search

    /// <summary>
    ///     Represents a KingbaseES operator for performing a full-text search match.
    /// </summary>
    TextSearchMatch, // @@

    /// <summary>
    ///     Represents a KingbaseES operator for logical AND within a full-text search match.
    /// </summary>
    TextSearchAnd, // &&

    /// <summary>
    ///     Represents a KingbaseES operator for logical OR within a full-text search match.
    /// </summary>
    TextSearchOr, // ||

    #endregion Text search

    #region JSON

    /// <summary>
    ///     Represents a KingbaseES operator for checking whether a key exists in a JSON document.
    /// </summary>
    JsonExists, // ?

    /// <summary>
    ///     Represents a KingbaseES operator for checking whether any of multiple keys exists in a JSON document.
    /// </summary>
    JsonExistsAny, // ?@>

    /// <summary>
    ///     Represents a KingbaseES operator for checking whether all the given keys exist in a JSON document.
    /// </summary>
    JsonExistsAll, // ?<@

    #endregion JSON

    #region LTree

    /// <summary>
    ///     Represents a KingbaseES operator for matching in an ltree type.
    /// </summary>
    LTreeMatches, // ~ or @

    /// <summary>
    ///     Represents a KingbaseES operator for matching in an ltree type.
    /// </summary>
    LTreeMatchesAny, // ?

    /// <summary>
    ///     Represents a KingbaseES operator for finding the first ancestor in an ltree type.
    /// </summary>
    LTreeFirstAncestor, // ?@>

    /// <summary>
    ///     Represents a KingbaseES operator for finding the first descendent in an ltree type.
    /// </summary>
    LTreeFirstDescendent, // ?<@

    /// <summary>
    ///     Represents a KingbaseES operator for finding the first match in an ltree type.
    /// </summary>
    LTreeFirstMatches, // ?~ or ?@

    #endregion LTree
}
