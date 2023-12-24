using Kdbndp.EntityFrameworkCore.KingbaseES.Diagnostics.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Migrations;
using Kdbndp.EntityFrameworkCore.KingbaseES.Migrations.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Update.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides extension methods to configure Entity Framework Core for Kdbndp.
/// </summary>
// ReSharper disable once UnusedMember.Global
public static class KdbndpServiceCollectionExtensions
{
    /// <summary>
    ///     <para>
    ///         Registers the given Entity Framework context as a service in the <see cref="IServiceCollection" />
    ///         and configures it to connect to a KingbaseES database.
    ///     </para>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure the SQL Server provider and connection string.
    ///     </para>
    ///     <para>
    ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
    ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
    ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
    ///     </para>
    ///     <para>
    ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
    ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
    ///     </para>
    /// </summary>
    /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
    /// <param name="connectionString"> The connection string of the database to connect to. </param>
    /// <param name="KdbndpOptionsAction"> An optional action to allow additional SQL Server specific configuration. </param>
    /// <param name="optionsAction"> An optional action to configure the <see cref="DbContextOptions" /> for the context. </param>
    /// <returns> The same service collection so that multiple calls can be chained. </returns>
    public static IServiceCollection AddKdbndp<TContext>(
        this IServiceCollection serviceCollection,
        string? connectionString,
        Action<KdbndpDbContextOptionsBuilder>? KdbndpOptionsAction = null,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        return serviceCollection.AddDbContext<TContext>(
            (_, options) =>
            {
                optionsAction?.Invoke(options);
                options.UseKdbndp(connectionString, KdbndpOptionsAction);
            });
    }

    /// <summary>
    ///     <para>
    ///         Adds the services required by the Kdbndp database provider for Entity Framework
    ///         to an <see cref="IServiceCollection" />.
    ///     </para>
    ///     <para>
    ///         Calling this method is no longer necessary when building most applications, including those that
    ///         use dependency injection in ASP.NET or elsewhere.
    ///         It is only needed when building the internal service provider for use with
    ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///         This is not recommend other than for some advanced scenarios.
    ///     </para>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddEntityFrameworkKdbndp(this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, KdbndpLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<KdbndpOptionsExtension>>()
            .TryAdd<IValueGeneratorCache>(p => p.GetRequiredService<IKdbndpValueGeneratorCache>())
            .TryAdd<IRelationalTypeMappingSource, KdbndpTypeMappingSource>()
            .TryAdd<ISqlGenerationHelper, KdbndpSqlGenerationHelper>()
            .TryAdd<IRelationalAnnotationProvider, KdbndpAnnotationProvider>()
            .TryAdd<IModelValidator, KdbndpModelValidator>()
            .TryAdd<IMigrator, KdbndpMigrator>()
            .TryAdd<IProviderConventionSetBuilder, KdbndpConventionSetBuilder>()
            .TryAdd<IUpdateSqlGenerator, KdbndpUpdateSqlGenerator>()
            .TryAdd<IModificationCommandFactory, KdbndpModificationCommandFactory>()
            .TryAdd<IModificationCommandBatchFactory, KdbndpModificationCommandBatchFactory>()
            .TryAdd<IValueGeneratorSelector, KdbndpValueGeneratorSelector>()
            .TryAdd<IRelationalConnection>(p => p.GetRequiredService<IKdbndpRelationalConnection>())
            .TryAdd<IMigrationsSqlGenerator, KdbndpMigrationsSqlGenerator>()
            .TryAdd<IRelationalDatabaseCreator, KdbndpDatabaseCreator>()
            .TryAdd<IHistoryRepository, KdbndpHistoryRepository>()
            .TryAdd<ICompiledQueryCacheKeyGenerator, KdbndpCompiledQueryCacheKeyGenerator>()
            .TryAdd<IExecutionStrategyFactory, KdbndpExecutionStrategyFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, KdbndpQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IMethodCallTranslatorProvider, KdbndpMethodCallTranslatorProvider>()
            .TryAdd<IAggregateMethodCallTranslatorProvider, KdbndpAggregateMethodCallTranslatorProvider>()
            .TryAdd<IMemberTranslatorProvider, KdbndpMemberTranslatorProvider>()
            .TryAdd<IEvaluatableExpressionFilter, KdbndpEvaluatableExpressionFilter>()
            .TryAdd<IQuerySqlGeneratorFactory, KdbndpQuerySqlGeneratorFactory>()
            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, KdbndpSqlTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPreprocessorFactory, KdbndpQueryTranslationPreprocessorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, KdbndpQueryTranslationPostprocessorFactory>()
            .TryAdd<IRelationalParameterBasedSqlProcessorFactory, KdbndpParameterBasedSqlProcessorFactory>()
            .TryAdd<ISqlExpressionFactory, KdbndpSqlExpressionFactory>()
            .TryAdd<ISingletonOptions, IKdbndpSingletonOptions>(p => p.GetRequiredService<IKdbndpSingletonOptions>())
            .TryAdd<IQueryCompilationContextFactory, KdbndpQueryCompilationContextFactory>()
            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<IKdbndpValueGeneratorCache, KdbndpValueGeneratorCache>()
                    .TryAddSingleton<IKdbndpSingletonOptions, KdbndpSingletonOptions>()
                    .TryAddSingleton<IKdbndpSequenceValueGeneratorFactory, KdbndpSequenceValueGeneratorFactory>()
                    .TryAddScoped<IKdbndpRelationalConnection, KdbndpRelationalConnection>())
            .TryAddCoreServices();

        return serviceCollection;
    }
}
