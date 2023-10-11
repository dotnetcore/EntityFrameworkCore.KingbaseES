using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for <see cref="IEntityType" /> for Kdbndp-specific metadata.
/// </summary>
public static class KdbndpEntityTypeExtensions
{
    #region Storage parameters

    public static Dictionary<string, object?> GetStorageParameters(this IReadOnlyEntityType entityType)
        => entityType.GetAnnotations()
            .Where(a => a.Name.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(KdbndpAnnotationNames.StorageParameterPrefix.Length),
                a => a.Value
            );

    public static string? GetStorageParameter(this IEntityType entityType, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return (string?)entityType[KdbndpAnnotationNames.StorageParameterPrefix + parameterName];
    }

    public static void SetStorageParameter(
        this IMutableEntityType entityType,
        string parameterName,
        object? parameterValue)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);
    }

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

    public static ConfigurationSource? GetStorageParameterConfigurationSource(
        this IConventionEntityType index,
        string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return index.FindAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName)?.GetConfigurationSource();
    }

    #endregion Storage parameters

    #region Unlogged

    public static bool GetIsUnlogged(this IReadOnlyEntityType entityType)
        => entityType[KdbndpAnnotationNames.UnloggedTable] as bool? ?? false;

    public static void SetIsUnlogged(this IMutableEntityType entityType, bool unlogged)
        => entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.UnloggedTable, unlogged);

    public static bool SetIsUnlogged(
        this IConventionEntityType entityType,
        bool unlogged,
        bool fromDataAnnotation = false)
    {
        entityType.SetOrRemoveAnnotation(KdbndpAnnotationNames.UnloggedTable, unlogged, fromDataAnnotation);

        return unlogged;
    }

    public static ConfigurationSource? GetIsUnloggedConfigurationSource(this IConventionEntityType index)
        => index.FindAnnotation(KdbndpAnnotationNames.UnloggedTable)?.GetConfigurationSource();

    #endregion Unlogged

    #region CockroachDb interleave in parent

    public static CockroachDbInterleaveInParent GetCockroachDbInterleaveInParent(this IReadOnlyEntityType entityType)
        => new(entityType);

    #endregion CockroachDb interleave in parent
}
