namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

/// <summary>
///     A convention that configures the default model <see cref="KdbndpValueGenerationStrategy" /> as
///     <see cref="KdbndpValueGenerationStrategy.IdentityByDefaultColumn" /> for newer KingbaseES versions,
///     and <see cref="KdbndpValueGenerationStrategy.SerialColumn" /> for pre-10.0 versions.
/// </summary>
public class KdbndpValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    private readonly Version? _postgresVersion;

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpValueGenerationStrategyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    /// <param name="postgresVersion">The KingbaseES version being targeted. This affects the default value generation strategy.</param>
    public KdbndpValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        Version? postgresVersion)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
        _postgresVersion = postgresVersion;
    }

    /// <summary>
    ///     Parameter object containing service dependencies.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(
            _postgresVersion < new Version(10, 0)
                ? KdbndpValueGenerationStrategy.SerialColumn
                : KdbndpValueGenerationStrategy.IdentityByDefaultColumn);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                KdbndpValueGenerationStrategy? strategy = null;
                var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name != null!)
                {
                    strategy = property.GetValueGenerationStrategy(declaringTable, Dependencies.TypeMappingSource);
                    if (strategy == KdbndpValueGenerationStrategy.None
                        && !IsStrategyNoneNeeded(property, declaringTable))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var declaringView = property.GetMappedStoreObjects(StoreObjectType.View).FirstOrDefault();
                    if (declaringView.Name != null!)
                    {
                        strategy = property.GetValueGenerationStrategy(declaringView, Dependencies.TypeMappingSource);
                        if (strategy == KdbndpValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, declaringView))
                        {
                            strategy = null;
                        }
                    }
                }

                // Needed for the annotation to show up in the model snapshot
                if (strategy != null
                    && declaringTable.Name != null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);

                    if (strategy == KdbndpValueGenerationStrategy.Sequence)
                    {
                        var sequence = modelBuilder.HasSequence(
                            property.GetSequenceName(declaringTable)
                            ?? entityType.GetRootType().ShortName() + modelBuilder.Metadata.GetSequenceNameSuffix(),
                            property.GetSequenceSchema(declaringTable)
                            ?? modelBuilder.Metadata.GetSequenceSchema()).Metadata;

                        property.Builder.HasDefaultValueSql(
                            RelationalDependencies.UpdateSqlGenerator.GenerateObtainNextSequenceValueOperation(
                                sequence.Name, sequence.Schema));
                    }
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd
                && !property.TryGetDefaultValue(storeObject, out _)
                && property.GetDefaultValueSql(storeObject) is null
                && property.GetComputedColumnSql(storeObject) is null
                && property.DeclaringType.Model.GetValueGenerationStrategy() != KdbndpValueGenerationStrategy.None)
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
