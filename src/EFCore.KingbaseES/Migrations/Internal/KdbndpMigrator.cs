using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Migrations.Internal;

// Migrator is EF-pubternal, but overriding it is the only way to force Kdbndp to ReloadTypes() after executing a migration which adds
// types to the database
#pragma warning disable EF1001

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpMigrator : Migrator
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IRelationalConnection _connection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpMigrator(
        IMigrationsAssembly migrationsAssembly,
        IHistoryRepository historyRepository,
        IDatabaseCreator databaseCreator,
        IMigrationsSqlGenerator migrationsSqlGenerator,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IMigrationCommandExecutor migrationCommandExecutor,
        IRelationalConnection connection,
        ISqlGenerationHelper sqlGenerationHelper,
        ICurrentDbContext currentContext,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        IRelationalCommandDiagnosticsLogger commandLogger,
        IDatabaseProvider databaseProvider)
        : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder,
            migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger,
            commandLogger, databaseProvider)
    {
        _historyRepository = historyRepository;
        _connection = connection;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Migrate(string? targetMigration = null)
    {
        var appliedMigrations = _historyRepository.GetAppliedMigrations();

        base.Migrate(targetMigration);

        PopulateMigrations(
            appliedMigrations.Select(t => t.MigrationId),
            targetMigration,
            out var migrationsToApply,
            out var migrationsToRevert,
            out _);

        if (migrationsToRevert.Count + migrationsToApply.Count == 0)
        {
            return;
        }

        // If a KingbaseES extension, enum or range was added, we want Kdbndp to reload all types at the ADO.NET level.
        var migrations = migrationsToApply.Count > 0 ? migrationsToApply : migrationsToRevert;
        var reloadTypes = migrations
            .SelectMany(m => m.UpOperations)
            .OfType<AlterDatabaseOperation>()
            .Any(o => o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        if (reloadTypes && _connection.DbConnection is KdbndpConnection KdbndpConnection)
        {
            KdbndpConnection.Open();
            try
            {
                KdbndpConnection.ReloadTypes();
            }
            catch
            {
                KdbndpConnection.Close();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task MigrateAsync(
        string? targetMigration = null,
        CancellationToken cancellationToken = default)
    {
        var appliedMigrations = await _historyRepository.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);

        await base.MigrateAsync(targetMigration, cancellationToken).ConfigureAwait(false);

        PopulateMigrations(
            appliedMigrations.Select(t => t.MigrationId),
            targetMigration,
            out var migrationsToApply,
            out var migrationsToRevert,
            out _);

        if (migrationsToRevert.Count + migrationsToApply.Count == 0)
        {
            return;
        }

        // If a KingbaseES extension, enum or range was added, we want Kdbndp to reload all types at the ADO.NET level.
        var migrations = migrationsToApply.Count > 0 ? migrationsToApply : migrationsToRevert;
        var reloadTypes = migrations
            .SelectMany(m => m.UpOperations)
            .OfType<AlterDatabaseOperation>()
            .Any(o => o.GetPostgresExtensions().Any() || o.GetPostgresEnums().Any() || o.GetPostgresRanges().Any());

        if (reloadTypes && _connection.DbConnection is KdbndpConnection kdbndpConnection)
        {
            await kdbndpConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                kdbndpConnection.ReloadTypes();
            }
            catch
            {
                await kdbndpConnection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
