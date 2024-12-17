// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
public class KdbndpCSharpRuntimeAnnotationCodeGenerator
    : RelationalCSharpRuntimeAnnotationCodeGenerator, ICSharpRuntimeAnnotationCodeGenerator
{
    private int _typeMappingNestingCount;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpCSharpRuntimeAnnotationCodeGenerator(
        CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
        RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null)
    {
        _typeMappingNestingCount++;

        try
        {
            var result = base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);
            AddKdbndpTypeMappingTweaks(typeMapping, parameters);
            return result;
        }
        finally
        {
            _typeMappingNestingCount--;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual void AddKdbndpTypeMappingTweaks(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;

        var npgsqlDbTypeBasedDefaultInstance = typeMapping switch
        {
            KdbndpStringTypeMapping => KdbndpStringTypeMapping.Default,
            KdbndpUIntTypeMapping => KdbndpUIntTypeMapping.Default,
            KdbndpULongTypeMapping => KdbndpULongTypeMapping.Default,
            // KdbndpMultirangeTypeMapping => KdbndpMultirangeTypeMapping.Default,
            _ => (IKdbndpTypeMapping?)null
        };

        if (npgsqlDbTypeBasedDefaultInstance is not null)
        {
            CheckElementTypeMapping();

            var npgsqlDbType = ((IKdbndpTypeMapping)typeMapping).KdbndpDbType;

            if (npgsqlDbType != npgsqlDbTypeBasedDefaultInstance.KdbndpDbType)
            {
                mainBuilder.AppendLine(";");

                mainBuilder.Append(
                    $"{parameters.TargetName}.TypeMapping = (({typeMapping.GetType().Name}){parameters.TargetName}.TypeMapping).Clone(npgsqlDbType: ");

                mainBuilder
                    .Append(nameof(KdbndpTypes))
                    .Append(".")
                    .Append(nameof(KdbndpDbType))
                    .Append(".")
                    .Append(npgsqlDbType.ToString());

                mainBuilder
                    .Append(")")
                    .DecrementIndent();
            }

        }

        switch (typeMapping)
        {
            case KdbndpEnumTypeMapping enumTypeMapping:
                CheckElementTypeMapping();

                var code = Dependencies.CSharpHelper;
                mainBuilder.AppendLine(";");

                mainBuilder.AppendLine(
                        $"{parameters.TargetName}.TypeMapping = ((KdbndpEnumTypeMapping){parameters.TargetName}.TypeMapping).Clone(")
                    .IncrementIndent();

                mainBuilder
                    .Append("unquotedStoreType: ")
                    .Append(code.Literal(enumTypeMapping.UnquotedStoreType))
                    .AppendLine(",")
                    .AppendLine("labels: new Dictionary<object, string>()")
                    .AppendLine("{")
                    .IncrementIndent();

                foreach (var (enumValue, label) in enumTypeMapping.Labels)
                {
                    mainBuilder
                        .Append('[')
                        .Append(code.UnknownLiteral(enumValue))
                        .Append(']')
                        .Append(" = ")
                        .Append(code.Literal(label))
                        .AppendLine(",");
                }

                mainBuilder
                    .Append("}")
                    .DecrementIndent()
                    .Append(")")
                    .DecrementIndent();

                break;

            case KdbndpRangeTypeMapping rangeTypeMapping:
            {
                CheckElementTypeMapping();

                var defaultInstance = KdbndpRangeTypeMapping.Default;

                var npgsqlDbTypeDifferent = rangeTypeMapping.KdbndpDbType != defaultInstance.KdbndpDbType;
                var subtypeTypeMappingIsDifferent = rangeTypeMapping.SubtypeMapping != defaultInstance.SubtypeMapping;

                if (npgsqlDbTypeDifferent || subtypeTypeMappingIsDifferent)
                {
                    mainBuilder.AppendLine(";");

                    mainBuilder.AppendLine(
                        $"{parameters.TargetName}.TypeMapping = ((KdbndpRangeTypeMapping){parameters.TargetName}.TypeMapping).Clone(")
                        .IncrementIndent();

                    mainBuilder
                        .Append("npgsqlDbType: ")
                        .Append(nameof(KdbndpTypes))
                        .Append(".")
                        .Append(nameof(KdbndpDbType))
                        .Append(".")
                        .Append(rangeTypeMapping.KdbndpDbType.ToString())
                        .AppendLine(",");

                    mainBuilder.Append("subtypeTypeMapping: ");

                    Create(rangeTypeMapping.SubtypeMapping, parameters);

                    mainBuilder
                        .Append(")")
                        .DecrementIndent();
                }

                break;
            }
        }

        void CheckElementTypeMapping()
        {
            if (_typeMappingNestingCount > 1)
            {
                throw new NotSupportedException(
                    $"Non-default Kdbndp type mappings ('{typeMapping.GetType().Name}' with store type '{(typeMapping as RelationalTypeMapping)?.StoreType}') aren't currently supported as element type mappings, see https://github.com/npgsql/efcore.pg/issues/3366.");
            }
        }
    }

    /// <inheritdoc />
    public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
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

        base.Generate(model, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IRelationalModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
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

        base.Generate(model, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(KdbndpAnnotationNames.IdentityOptions);
            annotations.Remove(KdbndpAnnotationNames.TsVectorConfig);
            annotations.Remove(KdbndpAnnotationNames.TsVectorProperties);
            annotations.Remove(KdbndpAnnotationNames.CompressionMethod);

            if (!annotations.ContainsKey(KdbndpAnnotationNames.ValueGenerationStrategy))
            {
                annotations[KdbndpAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }

        base.Generate(property, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(KdbndpAnnotationNames.IdentityOptions);
            annotations.Remove(KdbndpAnnotationNames.TsVectorConfig);
            annotations.Remove(KdbndpAnnotationNames.TsVectorProperties);
            annotations.Remove(KdbndpAnnotationNames.CompressionMethod);
        }

        base.Generate(column, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

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

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(ITableIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

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

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(KdbndpAnnotationNames.UnloggedTable);
            annotations.Remove(CockroachDbAnnotationNames.InterleaveInParent);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(entityType, parameters);
    }

    /// <inheritdoc />
    public override void Generate(ITable table, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(KdbndpAnnotationNames.UnloggedTable);
            annotations.Remove(CockroachDbAnnotationNames.InterleaveInParent);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(table, parameters);
    }
}
