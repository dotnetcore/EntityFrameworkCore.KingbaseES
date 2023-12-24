using System.Data.Common;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpSingletonOptions : IKdbndpSingletonOptions
{
    /// <inheritdoc />
    public virtual Version PostgresVersion { get; private set; } = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsPostgresVersionSet { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool UseRedshift { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ReverseNullOrderingEnabled { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbDataSource? DataSource { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider? ApplicationServiceProvider { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpSingletonOptions()
    {
        UserRangeDefinitions = Array.Empty<UserRangeDefinition>();
    }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var kdbndpOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();
        var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        PostgresVersion = kdbndpOptions.PostgresVersion;
        IsPostgresVersionSet = kdbndpOptions.IsPostgresVersionSet;
        UseRedshift = kdbndpOptions.UseRedshift;
        ReverseNullOrderingEnabled = kdbndpOptions.ReverseNullOrdering;
        UserRangeDefinitions = kdbndpOptions.UserRangeDefinitions;

        // TODO: Remove after https://github.com/dotnet/efcore/pull/29950
        ApplicationServiceProvider = coreOptions.ApplicationServiceProvider;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var kdbndpOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();

        if (PostgresVersion != kdbndpOptions.PostgresVersion)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.SetPostgresVersion),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UseRedshift != kdbndpOptions.UseRedshift)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.UseRedshift),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (ReverseNullOrderingEnabled != kdbndpOptions.ReverseNullOrdering)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.ReverseNullOrdering),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (kdbndpOptions.DataSource is not null && !ReferenceEquals(DataSource, kdbndpOptions.DataSource))
        {
            throw new InvalidOperationException(
                KdbndpStrings.TwoDataSourcesInSameServiceProvider(nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (!UserRangeDefinitions.SequenceEqual(kdbndpOptions.UserRangeDefinitions))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.MapRange),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}
