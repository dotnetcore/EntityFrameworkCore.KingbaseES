namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;

/// <summary>
///     Defines strategies to use when generating values for database columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>.
/// </remarks>
public enum KdbndpValueGenerationStrategy
{
    /// <summary>
    ///     No Kdbndp-specific strategy.
    /// </summary>
    None,

    /// <summary>
    ///     <para>
    ///         A sequence-based hi-lo pattern where blocks of IDs are allocated from the server and
    ///         used client-side for generating keys.
    ///     </para>
    ///     <para>
    ///         This is an advanced pattern--only use this strategy if you are certain it is what you need.
    ///     </para>
    /// </summary>
    SequenceHiLo,

    /// <summary>
    ///     <para>
    ///         Selects the serial column strategy, which is a regular column backed by an auto-created index.
    ///     </para>
    ///     <para>
    ///         If you are creating a new project on KingbaseES 10 or above, consider using <see cref="IdentityByDefaultColumn" /> instead.
    ///     </para>
    /// </summary>
    SerialColumn,

    /// <summary>
    ///     <para>Selects the always-identity column strategy (a value cannot be provided).</para>
    ///     <para>Available only starting KingbaseES 10.</para>
    /// </summary>
    IdentityAlwaysColumn,

    /// <summary>
    ///     <para>Selects the by-default-identity column strategy (a value can be provided to override the identity mechanism).</para>
    ///     <para>Available only starting KingbaseES 10.</para>
    /// </summary>
    IdentityByDefaultColumn,

    /// <summary>
    ///     A pattern that uses a database sequence to generate values for the column.
    /// </summary>
    Sequence
}

/// <summary>
///     Extension methods over <see cref="KdbndpValueGenerationStrategy" />.
/// </summary>
public static class KdbndpValueGenerationStrategyExtensions
{
    /// <summary>
    ///     Whether the given strategy is either <see cref="KdbndpValueGenerationStrategy.IdentityByDefaultColumn" /> or
    ///     <see cref="KdbndpValueGenerationStrategy.IdentityAlwaysColumn" />.
    /// </summary>
    public static bool IsIdentity(this KdbndpValueGenerationStrategy strategy)
        => strategy is KdbndpValueGenerationStrategy.IdentityByDefaultColumn or KdbndpValueGenerationStrategy.IdentityAlwaysColumn;

    /// <summary>
    ///     Whether the given strategy is either <see cref="KdbndpValueGenerationStrategy.IdentityByDefaultColumn" /> or
    ///     <see cref="KdbndpValueGenerationStrategy.IdentityAlwaysColumn" />.
    /// </summary>
    public static bool IsIdentity(this KdbndpValueGenerationStrategy? strategy)
        => strategy is KdbndpValueGenerationStrategy.IdentityByDefaultColumn or KdbndpValueGenerationStrategy.IdentityAlwaysColumn;
}
