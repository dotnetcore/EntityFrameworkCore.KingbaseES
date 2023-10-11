using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Design.Internal;

public class KdbndpAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo _modelHasPostgresExtensionMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string));

    private static readonly MethodInfo _modelHasPostgresExtensionMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string));

    private static readonly MethodInfo _modelHasPostgresEnumMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string[]));
        
    private static readonly MethodInfo _modelHasPostgresEnumMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string[]));

    private static readonly MethodInfo _modelHasPostgresRangeMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string));
        
    private static readonly MethodInfo _modelHasPostgresRangeMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string),typeof(string), typeof(string),typeof(string), typeof(string),typeof(string));

    private static readonly MethodInfo _modelUseSerialColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseSerialColumns), typeof(ModelBuilder));

    private static readonly MethodInfo _modelUseIdentityAlwaysColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseIdentityAlwaysColumns), typeof(ModelBuilder));

    private static readonly MethodInfo _modelUseIdentityByDefaultColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseIdentityByDefaultColumns), typeof(ModelBuilder));

    private static readonly MethodInfo _modelUseHiLoMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo _modelHasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRequiredRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

    private static readonly MethodInfo _entityTypeIsUnloggedMethodInfo
        = typeof(KdbndpEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpEntityTypeBuilderExtensions.IsUnlogged), typeof(EntityTypeBuilder), typeof(bool));

    private static readonly MethodInfo _propertyUseSerialColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseSerialColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo _propertyUseIdentityAlwaysColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseIdentityAlwaysColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo _propertyUseIdentityByDefaultColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseIdentityByDefaultColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo _propertyUseHiLoMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo _propertyHasIdentityOptionsMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.HasIdentityOptions), typeof(PropertyBuilder), typeof(long?), typeof(long?),
            typeof(long?), typeof(long?), typeof(bool?), typeof(long?));

    private static readonly MethodInfo _indexUseCollationMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.UseCollation), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo _indexHasMethodMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasMethod), typeof(IndexBuilder), typeof(string));

    private static readonly MethodInfo _indexHasOperatorsMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasOperators), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo _indexHasSortOrderMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasSortOrder), typeof(IndexBuilder), typeof(SortOrder[]));

    private static readonly MethodInfo _indexHasNullSortOrderMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasNullSortOrder), typeof(IndexBuilder), typeof(NullSortOrder[]));

    private static readonly MethodInfo _indexIncludePropertiesMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

    #endregion MethodInfos

    public KdbndpAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies) {}

    protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
    {
        Check.NotNull(model, nameof(model));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == RelationalAnnotationNames.DefaultSchema
            && (string?)annotation.Value == "public")
        {
            return true;
        }

        return false;
    }

    protected override bool IsHandledByConvention(IIndex index, IAnnotation annotation)
    {
        Check.NotNull(index, nameof(index));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == KdbndpAnnotationNames.IndexMethod
            && (string?)annotation.Value == "btree")
        {
            return true;
        }

        return false;
    }

    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(annotation, nameof(annotation));

        // The default by-convention value generation strategy is serial in pre-10 KingbaseES,
        // and IdentityByDefault otherwise.
        if (annotation.Name == KdbndpAnnotationNames.ValueGenerationStrategy)
        {
            // Note: both serial and identity-by-default columns are considered by-convention - we don't want
            // to assume that the KingbaseES version of the scaffolded database necessarily determines the
            // version of the database that the scaffolded model will target. This makes life difficult for
            // models with mixed strategies but that's an edge case.
            return (KdbndpValueGenerationStrategy?)annotation.Value switch
            {
                KdbndpValueGenerationStrategy.SerialColumn => true,
                KdbndpValueGenerationStrategy.IdentityByDefaultColumn => true,
                _ => false
            };
        }

        return false;
    }

    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
        => base.GenerateFluentApiCalls(model, annotations)
            .Concat(GenerateValueGenerationStrategy(annotations, onModel: true))
            .ToList();

    protected override MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
    {
        Check.NotNull(model, nameof(model));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
        {
            var extension = new PostgresExtension(model, annotation.Name);

            return extension.Schema is "public" or null
                ? new MethodCallCodeFragment(_modelHasPostgresExtensionMethodInfo1, extension.Name)
                : new MethodCallCodeFragment(_modelHasPostgresExtensionMethodInfo2, extension.Schema, extension.Name);
        }

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.EnumPrefix, StringComparison.Ordinal))
        {
            var enumTypeDef = new PostgresEnum(model, annotation.Name);

            return enumTypeDef.Schema is null
                ? new MethodCallCodeFragment(_modelHasPostgresEnumMethodInfo1, enumTypeDef.Name, enumTypeDef.Labels)
                : new MethodCallCodeFragment(_modelHasPostgresEnumMethodInfo2, enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
        }

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.RangePrefix, StringComparison.Ordinal))
        {
            var rangeTypeDef = new PostgresRange(model, annotation.Name);

            if (rangeTypeDef.Schema is null &&
                rangeTypeDef.CanonicalFunction is null &&
                rangeTypeDef.SubtypeOpClass is null &&
                rangeTypeDef.Collation is null &&
                rangeTypeDef.SubtypeDiff is null)
            {
                return new MethodCallCodeFragment(_modelHasPostgresRangeMethodInfo1, rangeTypeDef.Name, rangeTypeDef.Subtype);
            }

            return new MethodCallCodeFragment(_modelHasPostgresRangeMethodInfo2,
                rangeTypeDef.Schema,
                rangeTypeDef.Name,
                rangeTypeDef.Subtype,
                rangeTypeDef.CanonicalFunction,
                rangeTypeDef.SubtypeOpClass,
                rangeTypeDef.Collation,
                rangeTypeDef.SubtypeDiff);
        }

        return null;
    }

    protected override MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
    {
        Check.NotNull(entityType, nameof(entityType));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == KdbndpAnnotationNames.UnloggedTable)
        {
            return new MethodCallCodeFragment(_entityTypeIsUnloggedMethodInfo, annotation.Value);
        }

        return null;
    }

    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
        => base.GenerateFluentApiCalls(property, annotations)
            .Concat(GenerateValueGenerationStrategy(annotations, onModel: false))
            .Concat(GenerateIdentityOptions(annotations))
            .ToList();

    private IReadOnlyList<MethodCallCodeFragment> GenerateValueGenerationStrategy(
        IDictionary<string, IAnnotation> annotations,
        bool onModel)
    {
        if (!TryGetAndRemove(annotations, KdbndpAnnotationNames.ValueGenerationStrategy,
                out KdbndpValueGenerationStrategy strategy))
        {
            return Array.Empty<MethodCallCodeFragment>();
        }

        switch (strategy)
        {
            case KdbndpValueGenerationStrategy.SerialColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseSerialColumnsMethodInfo : _propertyUseSerialColumnMethodInfo)
                };

            case KdbndpValueGenerationStrategy.IdentityAlwaysColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseIdentityAlwaysColumnsMethodInfo : _propertyUseIdentityAlwaysColumnMethodInfo)
                };

            case KdbndpValueGenerationStrategy.IdentityByDefaultColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseIdentityByDefaultColumnsMethodInfo : _propertyUseIdentityByDefaultColumnMethodInfo)
                };

            case KdbndpValueGenerationStrategy.SequenceHiLo:
                var name = GetAndRemove<string>(KdbndpAnnotationNames.HiLoSequenceName)!;
                var schema = GetAndRemove<string>(KdbndpAnnotationNames.HiLoSequenceSchema);
                return new List<MethodCallCodeFragment>
                {
                    new(
                        onModel ? _modelUseHiLoMethodInfo : _propertyUseHiLoMethodInfo,
                        (name, schema) switch
                        {
                            (null, null) => Array.Empty<object>(),
                            (_, null) => new object[] { name },
                            _ => new object?[] { name!, schema }
                        })
                };

            case KdbndpValueGenerationStrategy.None:
                return new List<MethodCallCodeFragment>
                {
                    new(_modelHasAnnotationMethodInfo, KdbndpAnnotationNames.ValueGenerationStrategy, KdbndpValueGenerationStrategy.None)
                };

            default:
                throw new ArgumentOutOfRangeException(strategy.ToString());
        }

        T? GetAndRemove<T>(string annotationName)
            => TryGetAndRemove(annotations, annotationName, out T? annotationValue)
                ? annotationValue
                : default;
    }

    private IReadOnlyList<MethodCallCodeFragment> GenerateIdentityOptions(IDictionary<string, IAnnotation> annotations)
    {
        if (!TryGetAndRemove(annotations, KdbndpAnnotationNames.IdentityOptions,
                out string? annotationValue))
        {
            return Array.Empty<MethodCallCodeFragment>();
        }

        var identityOptions = IdentitySequenceOptionsData.Deserialize(annotationValue);
        return new List<MethodCallCodeFragment>
        {
            new(
                _propertyHasIdentityOptionsMethodInfo,
                identityOptions.StartValue,
                identityOptions.IncrementBy == 1 ? null : (long?) identityOptions.IncrementBy,
                identityOptions.MinValue,
                identityOptions.MaxValue,
                identityOptions.IsCyclic ? true : (bool?) null,
                identityOptions.NumbersToCache == 1 ? null : (long?) identityOptions.NumbersToCache)
        };
    }

    protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        => annotation.Name switch
        {
            RelationalAnnotationNames.Collation
                => new MethodCallCodeFragment(_indexUseCollationMethodInfo, annotation.Value),

            KdbndpAnnotationNames.IndexMethod
                => new MethodCallCodeFragment(_indexHasMethodMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexOperators
                => new MethodCallCodeFragment(_indexHasOperatorsMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexSortOrder
                => new MethodCallCodeFragment(_indexHasSortOrderMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexNullSortOrder
                => new MethodCallCodeFragment(_indexHasNullSortOrderMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexInclude
                => new MethodCallCodeFragment(_indexIncludePropertiesMethodInfo, annotation.Value),
            _ => null
        };

    private static bool TryGetAndRemove<T>(
        IDictionary<string, IAnnotation> annotations,
        string annotationName,
        [NotNullWhen(true)] out T? annotationValue)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value is not null)
        {
            annotations.Remove(annotationName);
            annotationValue = (T)annotation.Value;
            return true;
        }

        annotationValue = default;
        return false;
    }
}
