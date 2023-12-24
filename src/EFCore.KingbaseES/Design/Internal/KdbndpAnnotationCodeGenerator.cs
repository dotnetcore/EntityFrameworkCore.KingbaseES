using System.Diagnostics.CodeAnalysis;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo ModelHasPostgresExtensionMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string));

    private static readonly MethodInfo ModelHasPostgresExtensionMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string), typeof(string),
            typeof(string));

    private static readonly MethodInfo ModelHasPostgresEnumMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string[]));

    private static readonly MethodInfo ModelHasPostgresEnumMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string[]));

    private static readonly MethodInfo ModelHasPostgresRangeMethodInfo1
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo ModelHasPostgresRangeMethodInfo2
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string),
            typeof(string), typeof(string), typeof(string), typeof(string));

    private static readonly MethodInfo ModelUseSerialColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseSerialColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseIdentityAlwaysColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseIdentityAlwaysColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseIdentityByDefaultColumnsMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseIdentityByDefaultColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseHiLoMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo ModelHasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRequiredRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

    private static readonly MethodInfo ModelUseKeySequencesMethodInfo
        = typeof(KdbndpModelBuilderExtensions).GetRuntimeMethod(
            nameof(KdbndpModelBuilderExtensions.UseKeySequences), new[] { typeof(ModelBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo EntityTypeIsUnloggedMethodInfo
        = typeof(KdbndpEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpEntityTypeBuilderExtensions.IsUnlogged), typeof(EntityTypeBuilder), typeof(bool));

    private static readonly MethodInfo PropertyUseSerialColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseSerialColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseIdentityAlwaysColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseIdentityAlwaysColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseIdentityByDefaultColumnMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseIdentityByDefaultColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseHiLoMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo PropertyHasIdentityOptionsMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.HasIdentityOptions), typeof(PropertyBuilder), typeof(long?), typeof(long?),
            typeof(long?), typeof(long?), typeof(bool?), typeof(long?));

    private static readonly MethodInfo PropertyUseSequenceMethodInfo
        = typeof(KdbndpPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(KdbndpPropertyBuilderExtensions.UseSequence), new[] { typeof(PropertyBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo IndexUseCollationMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.UseCollation), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo IndexHasMethodMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasMethod), typeof(IndexBuilder), typeof(string));

    private static readonly MethodInfo IndexHasOperatorsMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasOperators), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo IndexHasSortOrderMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasSortOrder), typeof(IndexBuilder), typeof(SortOrder[]));

    private static readonly MethodInfo IndexHasNullSortOrderMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.HasNullSortOrder), typeof(IndexBuilder), typeof(NullSortOrder[]));

    private static readonly MethodInfo IndexIncludePropertiesMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo IndexAreNullsDistinctMethodInfo
        = typeof(KdbndpIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpIndexBuilderExtensions.AreNullsDistinct), typeof(IndexBuilder), typeof(bool));

    #endregion MethodInfos

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: true) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
    {
        Check.NotNull(model, nameof(model));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
        {
            var extension = new PostgresExtension(model, annotation.Name);

            return extension.Schema is "public" or null
                ? new MethodCallCodeFragment(ModelHasPostgresExtensionMethodInfo1, extension.Name)
                : new MethodCallCodeFragment(ModelHasPostgresExtensionMethodInfo2, extension.Schema, extension.Name);
        }

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.EnumPrefix, StringComparison.Ordinal))
        {
            var enumTypeDef = new PostgresEnum(model, annotation.Name);

            return enumTypeDef.Schema is null
                ? new MethodCallCodeFragment(ModelHasPostgresEnumMethodInfo1, enumTypeDef.Name, enumTypeDef.Labels)
                : new MethodCallCodeFragment(ModelHasPostgresEnumMethodInfo2, enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
        }

        if (annotation.Name.StartsWith(KdbndpAnnotationNames.RangePrefix, StringComparison.Ordinal))
        {
            var rangeTypeDef = new PostgresRange(model, annotation.Name);

            if (rangeTypeDef.Schema is null
                && rangeTypeDef.CanonicalFunction is null
                && rangeTypeDef.SubtypeOpClass is null
                && rangeTypeDef.Collation is null
                && rangeTypeDef.SubtypeDiff is null)
            {
                return new MethodCallCodeFragment(ModelHasPostgresRangeMethodInfo1, rangeTypeDef.Name, rangeTypeDef.Subtype);
            }

            return new MethodCallCodeFragment(
                ModelHasPostgresRangeMethodInfo2,
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
    {
        Check.NotNull(entityType, nameof(entityType));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == KdbndpAnnotationNames.UnloggedTable)
        {
            return new MethodCallCodeFragment(EntityTypeIsUnloggedMethodInfo, annotation.Value);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: false) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        if (GenerateIdentityOptions(annotations) is { } identityOptionsFragment)
        {
            fragments.Add(identityOptionsFragment);
        }

        return fragments;
    }

    private MethodCallCodeFragment? GenerateValueGenerationStrategy(IDictionary<string, IAnnotation> annotations, bool onModel)
    {
        if (!TryGetAndRemove(annotations, KdbndpAnnotationNames.ValueGenerationStrategy, out KdbndpValueGenerationStrategy strategy))
        {
            return null;
        }

        switch (strategy)
        {
            case KdbndpValueGenerationStrategy.SerialColumn:
                return new MethodCallCodeFragment(onModel ? ModelUseSerialColumnsMethodInfo : PropertyUseSerialColumnMethodInfo);

            case KdbndpValueGenerationStrategy.IdentityAlwaysColumn:
                return new MethodCallCodeFragment(
                    onModel ? ModelUseIdentityAlwaysColumnsMethodInfo : PropertyUseIdentityAlwaysColumnMethodInfo);

            case KdbndpValueGenerationStrategy.IdentityByDefaultColumn:
                return new MethodCallCodeFragment(
                    onModel ? ModelUseIdentityByDefaultColumnsMethodInfo : PropertyUseIdentityByDefaultColumnMethodInfo);

            case KdbndpValueGenerationStrategy.SequenceHiLo:
            {
                var name = GetAndRemove<string>(KdbndpAnnotationNames.HiLoSequenceName)!;
                var schema = GetAndRemove<string>(KdbndpAnnotationNames.HiLoSequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseHiLoMethodInfo : PropertyUseHiLoMethodInfo,
                    (name, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { name },
                        _ => new object?[] { name!, schema }
                    });
            }

            case KdbndpValueGenerationStrategy.Sequence:
            {
                var nameOrSuffix = GetAndRemove<string>(
                    onModel ? KdbndpAnnotationNames.SequenceNameSuffix : KdbndpAnnotationNames.SequenceName);

                var schema = GetAndRemove<string>(KdbndpAnnotationNames.SequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseKeySequencesMethodInfo : PropertyUseSequenceMethodInfo,
                    (name: nameOrSuffix, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { nameOrSuffix },
                        _ => new object[] { nameOrSuffix!, schema }
                    });
            }
            case KdbndpValueGenerationStrategy.None:
                return new MethodCallCodeFragment(
                    ModelHasAnnotationMethodInfo, KdbndpAnnotationNames.ValueGenerationStrategy, KdbndpValueGenerationStrategy.None);

            default:
                throw new ArgumentOutOfRangeException(strategy.ToString());
        }

        T? GetAndRemove<T>(string annotationName)
            => TryGetAndRemove(annotations, annotationName, out T? annotationValue)
                ? annotationValue
                : default;
    }

    private MethodCallCodeFragment? GenerateIdentityOptions(IDictionary<string, IAnnotation> annotations)
    {
        if (!TryGetAndRemove(
                annotations, KdbndpAnnotationNames.IdentityOptions,
                out string? annotationValue))
        {
            return null;
        }

        var identityOptions = IdentitySequenceOptionsData.Deserialize(annotationValue);
        return new MethodCallCodeFragment(
            PropertyHasIdentityOptionsMethodInfo,
            identityOptions.StartValue,
            identityOptions.IncrementBy == 1 ? null : (long?)identityOptions.IncrementBy,
            identityOptions.MinValue,
            identityOptions.MaxValue,
            identityOptions.IsCyclic ? true : null,
            identityOptions.NumbersToCache == 1 ? null : (long?)identityOptions.NumbersToCache);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        => annotation.Name switch
        {
            RelationalAnnotationNames.Collation
                => new MethodCallCodeFragment(IndexUseCollationMethodInfo, annotation.Value),

            KdbndpAnnotationNames.IndexMethod
                => new MethodCallCodeFragment(IndexHasMethodMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexOperators
                => new MethodCallCodeFragment(IndexHasOperatorsMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexSortOrder
                => new MethodCallCodeFragment(IndexHasSortOrderMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexNullSortOrder
                => new MethodCallCodeFragment(IndexHasNullSortOrderMethodInfo, annotation.Value),
            KdbndpAnnotationNames.IndexInclude
                => new MethodCallCodeFragment(IndexIncludePropertiesMethodInfo, annotation.Value),
            KdbndpAnnotationNames.NullsDistinct
                => new MethodCallCodeFragment(IndexAreNullsDistinctMethodInfo, annotation.Value),
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
