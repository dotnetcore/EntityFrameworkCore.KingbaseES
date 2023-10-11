using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;

/// <summary>
/// The validator that enforces rules for Kdbndp.
/// </summary>
public class KdbndpModelValidator : RelationalModelValidator
{
    /// <summary>
    /// The backend version to target.
    /// </summary>
    private readonly Version _postgresVersion;

    /// <inheritdoc />
    public KdbndpModelValidator(
        ModelValidatorDependencies dependencies,
        RelationalModelValidatorDependencies relationalDependencies,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
        : base(dependencies, relationalDependencies)
        => _postgresVersion = KdbndpSingletonOptions.PostgresVersion;

    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateIdentityVersionCompatibility(model);
        ValidateIndexIncludeProperties(model);
    }

    /// <summary>
    /// Validates that identity columns are used only with KingbaseES 10.0 or later.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    protected virtual void ValidateIdentityVersionCompatibility(IModel model)
    {
        if (_postgresVersion.AtLeast(10))
        {
            return;
        }

        var strategy = model.GetValueGenerationStrategy();

        if (strategy is KdbndpValueGenerationStrategy.IdentityAlwaysColumn or KdbndpValueGenerationStrategy.IdentityByDefaultColumn)
        {
            throw new InvalidOperationException(
                $"'{strategy}' requires KingbaseES 10.0 or later. " +
                "If you're using an older version, set KingbaseES compatibility mode by calling " +
                $"'optionsBuilder.{nameof(KdbndpDbContextOptionsBuilder.SetPostgresVersion)}()' in your model's OnConfiguring. " +
                "See the docs for more info.");
        }

        foreach (var property in model.GetEntityTypes().SelectMany(e => e.GetProperties()))
        {
            var propertyStrategy = property.GetValueGenerationStrategy();

            if (propertyStrategy is KdbndpValueGenerationStrategy.IdentityAlwaysColumn
                or KdbndpValueGenerationStrategy.IdentityByDefaultColumn)
            {
                throw new InvalidOperationException(
                    $"{property.DeclaringEntityType}.{property.Name}: '{propertyStrategy}' requires KingbaseES 10.0 or later.");
            }
        }
    }

    protected virtual void ValidateIndexIncludeProperties(IModel model)
    {
        foreach (var index in model.GetEntityTypes().SelectMany(t => t.GetDeclaredIndexes()))
        {
            var includeProperties = index.GetIncludeProperties();
            if (includeProperties?.Count > 0)
            {
                var notFound = includeProperties
                    .FirstOrDefault(i => index.DeclaringEntityType.FindProperty(i) is null);

                if (notFound is not null)
                {
                    throw new InvalidOperationException(
                        KdbndpStrings.IncludePropertyNotFound(index.DeclaringEntityType.DisplayName(), notFound));
                }

                var duplicate = includeProperties
                    .GroupBy(i => i)
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key)
                    .FirstOrDefault();

                if (duplicate is not null)
                {
                    throw new InvalidOperationException(
                        KdbndpStrings.IncludePropertyDuplicated(index.DeclaringEntityType.DisplayName(), duplicate));
                }

                var inIndex = includeProperties
                    .FirstOrDefault(i => index.Properties.Any(p => i == p.Name));

                if (inIndex is not null)
                {
                    throw new InvalidOperationException(
                        KdbndpStrings.IncludePropertyInIndex(index.DeclaringEntityType.DisplayName(), inIndex));
                }
            }
        }
    }

    /// <inheritdoc />
    protected override void ValidateCompatible(
        IProperty property,
        IProperty duplicateProperty,
        string columnName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

        if (property.GetCompressionMethod(storeObject) != duplicateProperty.GetCompressionMethod(storeObject))
        {
            throw new InvalidOperationException(
                KdbndpStrings.DuplicateColumnCompressionMethodMismatch(
                    duplicateProperty.DeclaringEntityType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringEntityType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
        }
    }
}
