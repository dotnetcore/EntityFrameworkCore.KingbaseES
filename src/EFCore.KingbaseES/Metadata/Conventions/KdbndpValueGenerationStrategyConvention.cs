using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

/// <summary>
/// A convention that configures the default model <see cref="KdbndpValueGenerationStrategy"/> as
/// <see cref="KdbndpValueGenerationStrategy.IdentityByDefaultColumn"/> for newer KingbaseES versions,
/// and <see cref="KdbndpValueGenerationStrategy.SerialColumn"/> for pre-10.0 versions.
/// </summary>
public class KdbndpValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    private readonly Version? _postgresVersion;

    /// <summary>
    /// Creates a new instance of <see cref="KdbndpValueGenerationStrategyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    /// <param name="postgresVersion">The KingbaseES version being targeted. This affects the default value generation strategy.</param>
    public KdbndpValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        Version? postgresVersion)
    {
        Dependencies = dependencies;
        _postgresVersion = postgresVersion;
    }

    /// <summary>
    /// Parameter object containing service dependencies.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(
            _postgresVersion < new Version(10, 0)
                ? KdbndpValueGenerationStrategy.SerialColumn
                : KdbndpValueGenerationStrategy.IdentityByDefaultColumn);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                KdbndpValueGenerationStrategy? strategy = null;
                var table = entityType.GetTableName();
                if (table is not null)
                {
                    var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());
                    strategy = property.GetValueGenerationStrategy(storeObject, Dependencies.TypeMappingSource);
                    if (strategy == KdbndpValueGenerationStrategy.None
                        && !IsStrategyNoneNeeded(property, storeObject))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var view = entityType.GetViewName();
                    if (view is not null)
                    {
                        var storeObject = StoreObjectIdentifier.View(view, entityType.GetViewSchema());
                        strategy = property.GetValueGenerationStrategy(storeObject, Dependencies.TypeMappingSource);
                        if (strategy == KdbndpValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, storeObject))
                        {
                            strategy = null;
                        }
                    }
                }

                // Needed for the annotation to show up in the model snapshot
                if (strategy is not null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd
                && !property.TryGetDefaultValue(storeObject, out _)
                && property.GetDefaultValueSql(storeObject) is null
                && property.GetComputedColumnSql(storeObject) is null
                && property.DeclaringEntityType.Model.GetValueGenerationStrategy() != KdbndpValueGenerationStrategy.None)
            {
                var providerClrType = (property.GetValueConverter()
                        ?? (property.FindRelationalTypeMapping(storeObject)
                            ?? Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                    ?.ProviderClrType.UnwrapNullableType();

                return providerClrType is not null && (providerClrType.IsInteger());
            }

            return false;
        }
    }
}
