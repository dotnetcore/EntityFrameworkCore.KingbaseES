using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IEntityType" /> for Kdbndp-specific metadata.
/// </summary>
public static class KdbndpEntityTypeExtensions
{
    #region Storage parameters

    /// <summary>
    ///     Gets all storage parameters for the table mapped to the entity type.
    /// </summary>
    public static Dictionary<string, object?> GetStorageParameters(this IReadOnlyEntityType entityType)
        => entityType.GetAnnotations()
            .Where(a => a.Name.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(KdbndpAnnotationNames.StorageParameterPrefix.Length),
                a => a.Value);

    /// <summary>
    ///     Gets a storage parameter for the table mapped to the entity type.
    /// </summary>
    public static string? GetStorageParameter(this IEntityType entityType, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return (string?)entityType[KdbndpAnnotationNames.StorageParameterPrefix + parameterName];
    }

    /// <summary>
    ///     Sets a storage parameter on the table mapped to the entity type.
    /// </summary>
    public static void SetStorageParameter(this IMutableEntityType entityType, string parameterName, object? parameterValue)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);
    }

    /// <summary>
    ///     Sets a storage parameter on the table mapped to the entity type.
    /// </summary>
    public static object SetStorageParameter(
        this IConventionEntityType entityType,
        string parameterName,
        object? parameterValue,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);

        return parameterName;
    }

    /// <summary>
    ///     Gets the configuration source for a storage parameter for the table mapped to the entity type.
    /// </summary>
    public static ConfigurationSource? GetStorageParameterConfigurationSource(
        this IConventionEntityType index,
        string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return index.FindAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName)?.GetConfigurationSource();
    }

    #endregion Storage parameters

    #region Unlogged

    /// <summary>
    ///     Gets whether the table to which the entity is mapped is unlogged.
    /// </summary>
    public static bool GetIsUnlogged(this IReadOnlyEntityType entityType)
        => entityType[KdbndpAnnotationNames.UnloggedTable] as bool? ?? false;

    /// <summary>
    ///     Sets whether the table to which the entity is mapped is unlogged.
    /// </summary>
    public static void SetIsUnlogged(this IMutableEntityType entityType, bool unlogged)
        => entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.UnloggedTable, unlogged);

    /// <summary>
    ///     Sets whether the table to which the entity is mapped is unlogged.
    /// </summary>
    public static bool SetIsUnlogged(
        this IConventionEntityType entityType,
        bool unlogged,
        bool fromDataAnnotation = false)
    {
        entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.UnloggedTable, unlogged, fromDataAnnotation);

        return unlogged;
    }

    /// <summary>
    ///     Gets the configuration source for whether the table to which the entity is mapped is unlogged.
    /// </summary>
    public static ConfigurationSource? GetIsUnloggedConfigurationSource(this IConventionEntityType index)
        => index.FindAnnotation(KdbndpAnnotationNames.UnloggedTable)?.GetConfigurationSource();

    #endregion Unlogged

    #region CockroachDb interleave in parent

    /// <summary>
    ///     Gets the CockroachDB-specific interleave-in-parent setting for the table to which the entity is mapped.
    /// </summary>
    public static CockroachDbInterleaveInParent GetCockroachDbInterleaveInParent(this IReadOnlyEntityType entityType)
        => new(entityType);

    #endregion CockroachDb interleave in parent
}
