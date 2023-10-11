using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using RelationalPropertyExtensions = Microsoft.EntityFrameworkCore.RelationalPropertyExtensions;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

public class KdbndpAnnotationProvider : RelationalAnnotationProvider
{
    public KdbndpAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped entity types
        var entityType = table.EntityTypeMappings.First().EntityType;

        if (entityType.GetIsUnlogged())
        {
            yield return new Annotation(KdbndpAnnotationNames.UnloggedTable, entityType.GetIsUnlogged());
        }

        if (entityType[CockroachDbAnnotationNames.InterleaveInParent] is not null)
        {
            yield return new Annotation(CockroachDbAnnotationNames.InterleaveInParent, entityType[CockroachDbAnnotationNames.InterleaveInParent]);
        }

        foreach (var storageParamAnnotation in entityType.GetAnnotations()
                     .Where(a => a.Name.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
        {
            yield return storageParamAnnotation;
        }
    }

    public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);
        var valueGeneratedProperty = column.PropertyMappings.Where(
                m =>
                    m.TableMapping.IsSharedTablePrincipal && m.TableMapping.EntityType == m.Property.DeclaringEntityType)
            .Select(m => m.Property)
            .FirstOrDefault(
                p => p.GetValueGenerationStrategy(table) switch
                {
                    KdbndpValueGenerationStrategy.IdentityByDefaultColumn => true,
                    KdbndpValueGenerationStrategy.IdentityAlwaysColumn    => true,
                    KdbndpValueGenerationStrategy.SerialColumn            => true,
                    _                                                     => false
                });

        if (valueGeneratedProperty is not null)
        {
            var valueGenerationStrategy = valueGeneratedProperty.GetValueGenerationStrategy();
            yield return new Annotation(KdbndpAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy);

            if (valueGenerationStrategy == KdbndpValueGenerationStrategy.IdentityByDefaultColumn ||
                valueGenerationStrategy == KdbndpValueGenerationStrategy.IdentityAlwaysColumn)
            {
                if (valueGeneratedProperty[KdbndpAnnotationNames.IdentityOptions] is string identityOptions)
                {
                    yield return new Annotation(KdbndpAnnotationNames.IdentityOptions, identityOptions);
                }
            }
        }

        // If the property has a collation explicitly defined on it via the standard EF mechanism, it will get
        // passed on the Collation property (we don't need to do anything).
        // Otherwise, a model-wide default column collation exists, pass that through our custom annotation.
        if (column.PropertyMappings.All(m => RelationalPropertyExtensions.GetCollation(m.Property) is null) &&
            column.PropertyMappings.Select(m => m.Property.GetDefaultCollation())
                .FirstOrDefault(c => c is not null) is string defaultColumnCollation)
        {
            yield return new Annotation(KdbndpAnnotationNames.DefaultColumnCollation, defaultColumnCollation);
        }

        if (column.PropertyMappings.Select(m => m.Property.GetTsVectorConfig())
                .FirstOrDefault(c => c is not null) is string tsVectorConfig)
        {
            yield return new Annotation(KdbndpAnnotationNames.TsVectorConfig, tsVectorConfig);
        }

        valueGeneratedProperty = column.PropertyMappings.Select(m => m.Property)
            .FirstOrDefault(p => p.GetTsVectorProperties() is not null);
        if (valueGeneratedProperty is not null)
        {
            var tableIdentifier = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);

            yield return new Annotation(
                KdbndpAnnotationNames.TsVectorProperties,
                valueGeneratedProperty.GetTsVectorProperties()!
                    .Select(p2 => valueGeneratedProperty.DeclaringEntityType.FindProperty(p2)!.GetColumnName(tableIdentifier))
                    .ToArray());
        }

        // Model validation ensures that these facets are the same on all mapped properties
        var property = column.PropertyMappings.First().Property;

        if (property.GetCompressionMethod() is string compressionMethod)
        {
            yield return new Annotation(KdbndpAnnotationNames.CompressionMethod, compressionMethod);
        }
    }

    public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped indexes
        var modelIndex = index.MappedIndexes.First();

        if (modelIndex.GetCollation() is IReadOnlyList<string> collation)
        {
            yield return new Annotation(RelationalAnnotationNames.Collation, collation);
        }

        if (modelIndex.GetMethod() is string method)
        {
            yield return new Annotation(KdbndpAnnotationNames.IndexMethod, method);
        }

        if (modelIndex.GetOperators() is IReadOnlyList<string> operators)
        {
            yield return new Annotation(KdbndpAnnotationNames.IndexOperators, operators);
        }

        if (modelIndex.GetSortOrder() is IReadOnlyList<SortOrder> sortOrder)
        {
            yield return new Annotation(KdbndpAnnotationNames.IndexSortOrder, sortOrder);
        }

        if (modelIndex.GetNullSortOrder() is IReadOnlyList<NullSortOrder> nullSortOrder)
        {
            yield return new Annotation(KdbndpAnnotationNames.IndexNullSortOrder, nullSortOrder);
        }

        if (modelIndex.GetTsVectorConfig() is string configName)
        {
            yield return new Annotation(KdbndpAnnotationNames.TsVectorConfig, configName);
        }

        if (modelIndex.GetIncludeProperties() is IReadOnlyList<string> includeProperties)
        {
            var tableIdentifier = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

            yield return new Annotation(
                KdbndpAnnotationNames.IndexInclude,
                includeProperties
                    .Select(p => modelIndex.DeclaringEntityType.FindProperty(p)!.GetColumnName(tableIdentifier))
                    .ToArray());
        }

        var isCreatedConcurrently = modelIndex.IsCreatedConcurrently();
        if (isCreatedConcurrently.HasValue)
        {
            yield return new Annotation(
                KdbndpAnnotationNames.CreatedConcurrently,
                isCreatedConcurrently.Value);
        }
    }

    public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
    {
        if (!designTime)
        {
            return Array.Empty<IAnnotation>();
        }

        return model.Model.GetAnnotations().Where(
            a =>
                a.Name.StartsWith(KdbndpAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(KdbndpAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(KdbndpAnnotationNames.RangePrefix, StringComparison.Ordinal)
                || a.Name.StartsWith(KdbndpAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal));
    }
}
