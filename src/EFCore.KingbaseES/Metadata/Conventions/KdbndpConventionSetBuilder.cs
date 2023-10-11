using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

[EntityFrameworkInternal]
public class KdbndpConventionSetBuilder : RelationalConventionSetBuilder
{
    private readonly Version _postgresVersion;

    [EntityFrameworkInternal]
    public KdbndpConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        IKdbndpSingletonOptions KdbndpOptions)
        : base(dependencies, relationalDependencies)
        => _postgresVersion = KdbndpOptions.PostgresVersion;

    [EntityFrameworkInternal]
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        var valueGenerationStrategyConvention = new KdbndpValueGenerationStrategyConvention(Dependencies, RelationalDependencies, _postgresVersion);
        conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(63, Dependencies, RelationalDependencies));

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
