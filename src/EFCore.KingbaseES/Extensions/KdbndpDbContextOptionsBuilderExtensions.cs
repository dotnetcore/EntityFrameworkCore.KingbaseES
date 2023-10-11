using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods on <see cref="DbContextOptionsBuilder"/> and <see cref="DbContextOptionsBuilder{T}"/>
/// used to configure a <see cref="DbContext"/> to context to a KingbaseES database with Kdbndp.
/// </summary>
public static class KdbndpDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// <para>
    /// Configures the context to connect to a KingbaseES server with Kdbndp, but without initially setting any
    /// <see cref="DbConnection" /> or connection string.
    /// </para>
    /// <para>
    /// The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    /// to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    /// Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    /// </para>
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseKdbndp(
        this DbContextOptionsBuilder optionsBuilder,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

        ConfigureWarnings(optionsBuilder);

        KdbndpOptionsAction?.Invoke(new KdbndpDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the context to connect to a KingbaseES database with Kdbndp.
    /// </summary>
    /// <param name="optionsBuilder">A builder for setting options on the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-specific configuration.</param>
    /// <returns>
    /// The options builder so that further configuration can be chained.
    /// </returns>
    public static DbContextOptionsBuilder UseKdbndp(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotEmpty(connectionString, nameof(connectionString));

        var extension = (KdbndpOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        ConfigureWarnings(optionsBuilder);

        KdbndpOptionsAction?.Invoke(new KdbndpDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the context to connect to a KingbaseES database with Kdbndp.
    /// </summary>
    /// <param name="optionsBuilder">A builder for setting options on the context.</param>
    /// <param name="connection">
    /// An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    /// in the open state then EF will not open or close the connection. If the connection is in the closed
    /// state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-specific configuration.</param>
    /// <returns>
    /// The options builder so that further configuration can be chained.
    /// </returns>
    public static DbContextOptionsBuilder UseKdbndp(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotNull(connection, nameof(connection));

        var extension = (KdbndpOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        ConfigureWarnings(optionsBuilder);

        KdbndpOptionsAction?.Invoke(new KdbndpDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// <para>
    /// Configures the context to connect to a KingbaseES server with Kdbndp, but without initially setting any
    /// <see cref="DbConnection" /> or connection string.
    /// </para>
    /// <para>
    /// The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    /// to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    /// Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    /// </para>
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseKdbndp<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseKdbndp(
            (DbContextOptionsBuilder)optionsBuilder, KdbndpOptionsAction);

    /// <summary>
    /// Configures the context to connect to a KingbaseES database with Kdbndp.
    /// </summary>
    /// <param name="optionsBuilder">A builder for setting options on the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-configuration.</param>
    /// <returns>
    /// The options builder so that further configuration can be chained.
    /// </returns>
    public static DbContextOptionsBuilder<TContext> UseKdbndp<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseKdbndp(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, KdbndpOptionsAction);

    /// <summary>
    /// Configures the context to connect to a KingbaseES database with Kdbndp.
    /// </summary>
    /// <param name="optionsBuilder">A builder for setting options on the context.</param>
    /// <param name="connection">
    /// An existing <see cref="DbConnection" />to be used to connect to the database. If the connection is
    /// in the open state then EF will not open or close the connection. If the connection is in the closed
    /// state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="KdbndpOptionsAction">An optional action to allow additional Kdbndp-specific configuration.</param>
    /// <returns>
    /// The options builder so that further configuration can be chained.
    /// </returns>
    public static DbContextOptionsBuilder<TContext> UseKdbndp<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseKdbndp(
            (DbContextOptionsBuilder)optionsBuilder, connection, KdbndpOptionsAction);

    /// <summary>
    /// Returns an existing instance of <see cref="KdbndpOptionsExtension"/>, or a new instance if one does not exist.
    /// </summary>
    /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> to search.</param>
    /// <returns>
    /// An existing instance of <see cref="KdbndpOptionsExtension"/>, or a new instance if one does not exist.
    /// </returns>
    private static KdbndpOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<KdbndpOptionsExtension>() is KdbndpOptionsExtension existing
            ? new KdbndpOptionsExtension(existing)
            : new KdbndpOptionsExtension();

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsExtension = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
