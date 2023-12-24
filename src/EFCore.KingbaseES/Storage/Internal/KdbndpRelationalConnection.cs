using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Transactions;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpRelationalConnection : RelationalConnection, IKdbndpRelationalConnection
{
    private readonly ProvideClientCertificatesCallback? _provideClientCertificatesCallback;
    private readonly RemoteCertificateValidationCallback? _remoteCertificateValidationCallback;

    private DbDataSource? _dataSource;

    /// <summary>
    ///     Indicates whether the store connection supports ambient transactions
    /// </summary>
    protected override bool SupportsAmbientTransactions
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpRelationalConnection(RelationalConnectionDependencies dependencies, IKdbndpSingletonOptions options)
        : this(dependencies, dataSource: null)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected KdbndpRelationalConnection(RelationalConnectionDependencies dependencies, DbDataSource? dataSource)
        : base(dependencies)
    {
        if (dataSource is not null)
        {
            _dataSource = dataSource;

#if DEBUG
            // We validate in KdbndpOptionsExtensions.Validate that DataSource and these callbacks aren't specified together
            if (dependencies.ContextOptions.FindExtension<KdbndpOptionsExtension>() is { } KdbndpOptions)
            {
                Check.DebugAssert(
                    KdbndpOptions?.ProvideClientCertificatesCallback is null,
                    "Both DataSource and ProvideClientCertificatesCallback are non-null");
                Check.DebugAssert(
                    KdbndpOptions?.RemoteCertificateValidationCallback is null,
                    "Both DataSource and RemoteCertificateValidationCallback are non-null");
            }
#endif
        }
        else if (dependencies.ContextOptions.FindExtension<KdbndpOptionsExtension>() is { } KdbndpOptions)
        {
            _provideClientCertificatesCallback = KdbndpOptions.ProvideClientCertificatesCallback;
            _remoteCertificateValidationCallback = KdbndpOptions.RemoteCertificateValidationCallback;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override DbConnection CreateDbConnection()
    {
        if (_dataSource is not null)
        {
            return _dataSource.CreateConnection();
        }

        var conn = new KdbndpConnection(ConnectionString);

        if (_provideClientCertificatesCallback is not null)
        {
            conn.ProvideClientCertificatesCallback = _provideClientCertificatesCallback;
        }

        if (_remoteCertificateValidationCallback is not null)
        {
            conn.UserCertificateValidationCallback = _remoteCertificateValidationCallback;
        }

        return conn;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // TODO: Remove after DbDataSource support is added to EF Core (https://github.com/dotnet/efcore/issues/28266)
    public override string? ConnectionString
    {
        get => _dataSource is null ? base.ConnectionString : _dataSource.ConnectionString;
        set
        {
            base.ConnectionString = value;

            _dataSource = null;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [AllowNull]
    public new virtual KdbndpConnection DbConnection
    {
        get => (KdbndpConnection)base.DbConnection;
        set
        {
            base.DbConnection = value;

            _dataSource = null;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbDataSource? DbDataSource
    {
        get => _dataSource;
        set
        {
            DbConnection = null;
            ConnectionString = null;
            _dataSource = value;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IKdbndpRelationalConnection CreateAdminConnection()
    {
        if (Dependencies.ContextOptions.FindExtension<KdbndpOptionsExtension>() is not { } KdbndpOptions)
        {
            throw new InvalidOperationException($"{nameof(KdbndpOptionsExtension)} not found in {nameof(CreateAdminConnection)}");
        }

        var adminConnectionString = new KdbndpConnectionStringBuilder(ConnectionString)
        {
            Database = KdbndpOptions.AdminDatabase ?? "postgres",
            Pooling = false,
            Multiplexing = false
        }.ToString();

        var adminKdbndpOptions = _dataSource is not null
            ? KdbndpOptions.WithConnection(((KdbndpConnection)CreateDbConnection()).CloneWith(adminConnectionString))
            : KdbndpOptions.Connection is not null
                ? KdbndpOptions.WithConnection(DbConnection.CloneWith(adminConnectionString))
                : KdbndpOptions.WithConnectionString(adminConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(adminKdbndpOptions);

        return new KdbndpRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options }, dataSource: null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Accessing Transaction.Current is expensive, so don't do it if Enlist is false in the connection string
    public override Transaction? CurrentAmbientTransaction
        => ConnectionString is null || !ConnectionString.Contains("Enlist=false", StringComparison.InvariantCultureIgnoreCase)
            ? Transaction.Current
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual KdbndpRelationalConnection CloneWith(string connectionString)
    {
        var clonedDbConnection = DbConnection.CloneWith(connectionString);

        var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions)
            .WithConnectionString(null)
            .WithConnection(clonedDbConnection);

        var optionsBuilder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(relationalOptions);

        return new KdbndpRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options }, dataSource: null);
    }
}
