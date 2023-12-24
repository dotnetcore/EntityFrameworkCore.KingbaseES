using System.Net.Security;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;

/// <summary>
///     Allows for options specific to KingbaseES to be configured for a <see cref="DbContext" />.
/// </summary>
public class KdbndpDbContextOptionsBuilder
    : RelationalDbContextOptionsBuilder<KdbndpDbContextOptionsBuilder, KdbndpOptionsExtension>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KdbndpDbContextOptionsBuilder" /> class.
    /// </summary>
    /// <param name="optionsBuilder"> The core options builder.</param>
    public KdbndpDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        : base(optionsBuilder)
    {
    }

    /// <summary>
    ///     Connect to this database for administrative operations (creating/dropping databases).
    /// </summary>
    /// <param name="dbName">The name of the database for administrative operations.</param>
    public virtual KdbndpDbContextOptionsBuilder UseAdminDatabase(string? dbName)
        => WithOption(e => e.WithAdminDatabase(dbName));

    /// <summary>
    ///     Configures the backend version to target.
    /// </summary>
    /// <param name="postgresVersion">The backend version to target.</param>
    public virtual KdbndpDbContextOptionsBuilder SetPostgresVersion(Version? postgresVersion)
        => WithOption(e => e.WithPostgresVersion(postgresVersion));

    /// <summary>
    ///     Configures the backend version to target.
    /// </summary>
    /// <param name="major">The KingbaseES major version to target.</param>
    /// <param name="minor">The KingbaseES minor version to target.</param>
    public virtual KdbndpDbContextOptionsBuilder SetPostgresVersion(int major, int minor)
        => SetPostgresVersion(new Version(major, minor));

    /// <summary>
    ///     Configures the provider to work in Redshift compatibility mode, which avoids certain unsupported features from modern
    ///     KingbaseES versions.
    /// </summary>
    /// <param name="useRedshift">Whether to target Redshift.</param>
    public virtual KdbndpDbContextOptionsBuilder UseRedshift(bool useRedshift = true)
        => WithOption(e => e.WithRedshift(useRedshift));

    /// <summary>
    ///     Maps a user-defined KingbaseES range type for use.
    /// </summary>
    /// <typeparam name="TSubtype">
    ///     The CLR type of the range's subtype (or element).
    ///     The actual mapped type will be an <see cref="KdbndpRange{T}" /> over this type.
    /// </typeparam>
    /// <param name="rangeName">The name of the KingbaseES range type to be mapped.</param>
    /// <param name="schemaName">The name of the KingbaseES schema in which the range is defined.</param>
    /// <param name="subtypeName">
    ///     Optionally, the name of the range's KingbaseES subtype (or element).
    ///     This is usually not needed - the subtype will be inferred based on <typeparamref name="TSubtype" />.
    /// </param>
    /// <example>
    ///     To map a range of KingbaseES real, use the following:
    ///     <code>KdbndpTypeMappingSource.MapRange{float}("floatrange");</code>
    /// </example>
    public virtual KdbndpDbContextOptionsBuilder MapRange<TSubtype>(
        string rangeName,
        string? schemaName = null,
        string? subtypeName = null)
        => MapRange(rangeName, typeof(TSubtype), schemaName, subtypeName);

    /// <summary>
    ///     Maps a user-defined KingbaseES range type for use.
    /// </summary>
    /// <param name="rangeName">The name of the KingbaseES range type to be mapped.</param>
    /// <param name="schemaName">The name of the KingbaseES schema in which the range is defined.</param>
    /// <param name="subtypeClrType">
    ///     The CLR type of the range's subtype (or element).
    ///     The actual mapped type will be an <see cref="KdbndpRange{T}" /> over this type.
    /// </param>
    /// <param name="subtypeName">
    ///     Optionally, the name of the range's KingbaseES subtype (or element).
    ///     This is usually not needed - the subtype will be inferred based on <paramref name="subtypeClrType" />.
    /// </param>
    /// <example>
    ///     To map a range of KingbaseES real, use the following:
    ///     <code>KdbndpTypeMappingSource.MapRange("floatrange", typeof(float));</code>
    /// </example>
    public virtual KdbndpDbContextOptionsBuilder MapRange(
        string rangeName,
        Type subtypeClrType,
        string? schemaName = null,
        string? subtypeName = null)
        => WithOption(e => e.WithUserRangeDefinition(rangeName, schemaName, subtypeClrType, subtypeName));

    /// <summary>
    ///     Appends NULLS FIRST to all ORDER BY clauses. This is important for the tests which were written
    ///     for SQL Server. Note that to fully implement null-first ordering indexes also need to be generated
    ///     accordingly, and since this isn't done this feature isn't publicly exposed.
    /// </summary>
    /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
    internal virtual KdbndpDbContextOptionsBuilder ReverseNullOrdering(bool reverseNullOrdering = true)
        => WithOption(e => e.WithReverseNullOrdering(reverseNullOrdering));

    #region Authentication

    /// <summary>
    ///     Configures the <see cref="DbContext" /> to use the specified <see cref="ProvideClientCertificatesCallback" />.
    /// </summary>
    /// <param name="callback">The callback to use.</param>
    public virtual KdbndpDbContextOptionsBuilder ProvideClientCertificatesCallback(ProvideClientCertificatesCallback? callback)
        => WithOption(e => e.WithProvideClientCertificatesCallback(callback));

    /// <summary>
    ///     Configures the <see cref="DbContext" /> to use the specified <see cref="RemoteCertificateValidationCallback" />.
    /// </summary>
    /// <param name="callback">The callback to use.</param>
    public virtual KdbndpDbContextOptionsBuilder RemoteCertificateValidationCallback(RemoteCertificateValidationCallback? callback)
        => WithOption(e => e.WithRemoteCertificateValidationCallback(callback));

    #endregion Authentication

    #region Retrying execution strategy

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="KdbndpDbContextOptionsBuilder" /> configured to use
    ///     the default retrying <see cref="IExecutionStrategy" />.
    /// </returns>
    public virtual KdbndpDbContextOptionsBuilder EnableRetryOnFailure()
        => ExecutionStrategy(c => new KdbndpRetryingExecutionStrategy(c));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="KdbndpDbContextOptionsBuilder" /> with the specified parameters.
    /// </returns>
    public virtual KdbndpDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
        => ExecutionStrategy(c => new KdbndpRetryingExecutionStrategy(c, maxRetryCount));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <param name="errorCodesToAdd">Additional error codes that should be considered transient.</param>
    /// <returns>
    ///     An instance of <see cref="KdbndpDbContextOptionsBuilder" /> with the specified parameters.
    /// </returns>
    public virtual KdbndpDbContextOptionsBuilder EnableRetryOnFailure(ICollection<string>? errorCodesToAdd)
        => ExecutionStrategy(c => new KdbndpRetryingExecutionStrategy(c, errorCodesToAdd));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="maxRetryDelay">The maximum delay between retries.</param>
    /// <param name="errorCodesToAdd">Additional error codes that should be considered transient.</param>
    /// <returns>
    ///     An instance of <see cref="KdbndpDbContextOptionsBuilder" /> with the specified parameters.
    /// </returns>
    public virtual KdbndpDbContextOptionsBuilder EnableRetryOnFailure(
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<string>? errorCodesToAdd)
        => ExecutionStrategy(c => new KdbndpRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorCodesToAdd));

    #endregion Retrying execution strategy
}
