using System.Collections.Concurrent;
using System.Data.Common;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

/// <summary>
///     Manages resolving and creating <see cref="KdbndpDataSource" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class KdbndpDataSourceManager : IDisposable, IAsyncDisposable
{
    private readonly IEnumerable<IKdbndpDataSourceConfigurationPlugin> _plugins;
    private readonly ConcurrentDictionary<string, KdbndpDataSource> _dataSources = new();
    private volatile int _isDisposed;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpDataSourceManager(IEnumerable<IKdbndpDataSourceConfigurationPlugin> plugins)
        => _plugins = plugins.ToArray();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbDataSource? GetDataSource(KdbndpOptionsExtension? npgsqlOptionsExtension, IServiceProvider? applicationServiceProvider)
        => npgsqlOptionsExtension switch
        {
            // If the user has explicitly passed in a data source via UseKdbndp(), use that.
            // Note that in this case, the data source is scoped (not singleton), and so can change between different
            // DbContext instances using the same internal service provider.
            { DataSource: DbDataSource dataSource }
                => npgsqlOptionsExtension.DataSourceBuilderAction is null
                    ? dataSource
                    // If the user has explicitly passed in a data source via UseKdbndp(), but also supplied a data source configuration
                    // lambda, throw - we're unable to apply the configuration lambda to the externally-provided, already-built data source.
                    : throw new NotSupportedException(KdbndpStrings.DataSourceAndConfigNotSupported),

            // If the user has passed in a DbConnection, never use a data source - even if e.g. MapEnum() was called.
            // This is to avoid blocking and allow continuing using enums in conjunction with DbConnections (which
            // must be manually set up by the user for the enum, of course).
            { Connection: not null } => null,

            // If the user hasn't configured anything in UseKdbndp (no data source, no connection, no connection string), check the
            // application service provider to see if a data source is registered there, and return that.
            { ConnectionString: null } when applicationServiceProvider?.GetService<KdbndpDataSource>() is DbDataSource dataSource
                => dataSource,

            // Otherwise if there's no connection string, abort: a connection string is required to create a data source in any case.
            { ConnectionString: null } or null => null,

            // The following are features which require an KdbndpDataSource, since they require configuration on KdbndpDataSourceBuilder.
            { DataSourceBuilderAction: not null } => GetSingletonDataSource(npgsqlOptionsExtension),
            { EnumDefinitions.Count: > 0 } => GetSingletonDataSource(npgsqlOptionsExtension),
            _ when _plugins.Any() => GetSingletonDataSource(npgsqlOptionsExtension),

            // If there's no configured feature which requires us to use a data source internally, don't use one; this causes
            // KdbndpRelationalConnection to use the connection string as before (no data source), allowing switching connection strings
            // with the same service provider etc.
            _ => null
        };

    private DbDataSource GetSingletonDataSource(KdbndpOptionsExtension npgsqlOptionsExtension)
    {
        var connectionString = npgsqlOptionsExtension.ConnectionString;
        Check.DebugAssert(connectionString is not null, "Connection string can't be null");

        if (_dataSources.TryGetValue(connectionString, out var dataSource))
        {
            return dataSource;
        }

        var newDataSource = CreateDataSource(npgsqlOptionsExtension);

        var addedDataSource = _dataSources.GetOrAdd(connectionString, newDataSource);
        if (!ReferenceEquals(addedDataSource, newDataSource))
        {
            newDataSource.Dispose();
        }
        else if (_isDisposed == 1)
        {
            newDataSource.Dispose();
            throw new ObjectDisposedException(nameof(KdbndpDataSourceManager));
        }

        return addedDataSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual KdbndpDataSource CreateDataSource(KdbndpOptionsExtension npgsqlOptionsExtension)
    {
        var dataSourceBuilder = new KdbndpDataSourceBuilder(npgsqlOptionsExtension.ConnectionString);

        foreach (var enumDefinition in npgsqlOptionsExtension.EnumDefinitions)
        {
            dataSourceBuilder.MapEnum(
                enumDefinition.ClrType,
                enumDefinition.StoreTypeSchema is null
                    ? enumDefinition.StoreTypeName
                    : enumDefinition.StoreTypeSchema + "." + enumDefinition.StoreTypeName,
                enumDefinition.NameTranslator);
        }

        foreach (var plugin in _plugins)
        {
            plugin.Configure(dataSourceBuilder);
        }

        // Finally, if the user has provided a data source builder configuration action, invoke it.
        // Do this last, to allow the user to override anything set above.
        npgsqlOptionsExtension.DataSourceBuilderAction?.Invoke(dataSourceBuilder);

        return dataSourceBuilder.Build();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            foreach (var dataSource in _dataSources.Values)
            {
                dataSource.Dispose();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            foreach (var dataSource in _dataSources.Values)
            {
                await dataSource.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
