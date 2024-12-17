using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

/// <summary>
///     A convention that creates an optimized copy of the mutable model.
/// </summary>
public class KdbndpRuntimeModelConvention : RelationalRuntimeModelConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRuntimeModelConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public KdbndpRuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessModelAnnotations(
        Dictionary<string, object?> annotations,
        IModel model,
        RuntimeModel runtimeModel,
        bool runtime)
    {
        base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

        if (!runtime)
        {
            annotations.Remove(KdbndpAnnotationNames.DatabaseTemplate);
            annotations.Remove(KdbndpAnnotationNames.Tablespace);
            annotations.Remove(KdbndpAnnotationNames.CollationDefinitionPrefix);

#pragma warning disable CS0618
            annotations.Remove(KdbndpAnnotationNames.DefaultColumnCollation);
#pragma warning restore CS0618

            foreach (var annotationName in annotations.Keys.Where(
                         k =>
                             k.StartsWith(KdbndpAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                             || k.StartsWith(KdbndpAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                             || k.StartsWith(KdbndpAnnotationNames.RangePrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessEntityTypeAnnotations(
        Dictionary<string, object?> annotations,
        IEntityType entityType,
        RuntimeEntityType runtimeEntityType,
        bool runtime)
    {
        base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

        if (!runtime)
        {
            annotations.Remove(KdbndpAnnotationNames.UnloggedTable);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        if (!runtime)
        {
            annotations.Remove(KdbndpAnnotationNames.IdentityOptions);
            annotations.Remove(KdbndpAnnotationNames.TsVectorConfig);
            annotations.Remove(KdbndpAnnotationNames.TsVectorProperties);

            if (!annotations.ContainsKey(KdbndpAnnotationNames.ValueGenerationStrategy))
            {
                annotations[KdbndpAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

        if (!runtime)
        {
            annotations.Remove(KdbndpAnnotationNames.IndexMethod);
            annotations.Remove(KdbndpAnnotationNames.IndexOperators);
            annotations.Remove(KdbndpAnnotationNames.IndexSortOrder);
            annotations.Remove(KdbndpAnnotationNames.IndexNullSortOrder);
            annotations.Remove(KdbndpAnnotationNames.IndexInclude);
            annotations.Remove(KdbndpAnnotationNames.CreatedConcurrently);
            annotations.Remove(KdbndpAnnotationNames.NullsDistinct);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }
    }
}
