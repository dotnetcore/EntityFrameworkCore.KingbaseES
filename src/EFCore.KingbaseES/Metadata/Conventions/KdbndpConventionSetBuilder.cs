using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

/// <summary>
///     A builder for building conventions for Kdbndp.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations are allowed. This means that each
///         <see cref="DbContext" /> instance will use its own set of instances of this service. The implementations may depend on other
///         services registered with any lifetime. The implementations do not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     </para>
/// </remarks>
public class KdbndpConventionSetBuilder : RelationalConventionSetBuilder
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly Version _postgresVersion;

    /// <summary>
    ///     Creates a new <see cref="KdbndpConventionSetBuilder" /> instance.
    /// </summary>
    /// <param name="dependencies">The core dependencies for this service.</param>
    /// <param name="relationalDependencies">The relational dependencies for this service.</param>
    /// <param name="typeMappingSource">The type mapping source to use.</param>
    /// <param name="KdbndpSingletonOptions">The singleton options to use.</param>
    public KdbndpConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        IRelationalTypeMappingSource typeMappingSource,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
        : base(dependencies, relationalDependencies)
    {
        _typeMappingSource = typeMappingSource;
        _postgresVersion = KdbndpSingletonOptions.PostgresVersion;
    }

    /// <inheritdoc />
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        var valueGenerationStrategyConvention =
            new KdbndpValueGenerationStrategyConvention(Dependencies, RelationalDependencies, _postgresVersion);
        conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelInitializedConventions.Add(
            new RelationalMaxIdentifierLengthConvention(63, Dependencies, RelationalDependencies));

        conventionSet.PropertyAddedConventions.Add(new KdbndpJsonElementHackConvention());

        ValueGenerationConvention valueGenerationConvention = new KdbndpValueGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);

        ReplaceConvention(
            conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

        ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);

        ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);

        ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);

        var storeGenerationConvention =
            new KdbndpStoreGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.PropertyAnnotationChangedConventions, storeGenerationConvention);
        ReplaceConvention(
            conventionSet.PropertyAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

        conventionSet.ModelFinalizingConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelFinalizingConventions.Add(new KdbndpPostgresModelFinalizingConvention(_typeMappingSource));
        ReplaceConvention(conventionSet.ModelFinalizingConventions, storeGenerationConvention);
        ReplaceConvention(
            conventionSet.ModelFinalizingConventions,
            (SharedTableConvention)new KdbndpSharedTableConvention(Dependencies, RelationalDependencies));

        ReplaceConvention(
            conventionSet.ModelFinalizedConventions,
            (RuntimeModelConvention)new KdbndpRuntimeModelConvention(Dependencies, RelationalDependencies));

        return conventionSet;
    }

    /// <summary>
    ///     <para>
    ///         Call this method to build a <see cref="ConventionSet" /> for Kdbndp when using
    ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
    ///     </para>
    ///     <para>
    ///         Note that it is unusual to use this method.
    ///         Consider using <see cref="DbContext" /> in the normal way instead.
    ///     </para>
    /// </summary>
    /// <returns> The convention set. </returns>
    public static ConventionSet Build()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return ConventionSet.CreateConventionSet(context);
    }

    /// <summary>
    ///     <para>
    ///         Call this method to build a <see cref="ModelBuilder" /> for Kdbndp outside of <see cref="DbContext.OnModelCreating" />.
    ///     </para>
    ///     <para>
    ///         Note that it is unusual to use this method.
    ///         Consider using <see cref="DbContext" /> in the normal way instead.
    ///     </para>
    /// </summary>
    /// <returns> The convention set. </returns>
    public static ModelBuilder CreateModelBuilder()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
    }

    private static IServiceScope CreateServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkKdbndp()
            .AddDbContext<DbContext>(
                (p, o) =>
                    o.UseKdbndp("Server=.")
                        .UseInternalServiceProvider(p))
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}
