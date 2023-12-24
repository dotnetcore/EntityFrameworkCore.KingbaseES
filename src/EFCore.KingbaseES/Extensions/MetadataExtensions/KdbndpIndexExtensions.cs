using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IIndex" /> for Kdbndp-specific metadata.
/// </summary>
public static class KdbndpIndexExtensions
{
    #region Method

    /// <summary>
    ///     Returns the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static string? GetMethod(this IReadOnlyIndex index)
        => (string?)index[KdbndpAnnotationNames.IndexMethod];

    /// <summary>
    ///     Sets the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static void SetMethod(this IMutableIndex index, string? method)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexMethod, method);

    /// <summary>
    ///     Sets the index method to be used, or <c>null</c> if it hasn't been specified.
    ///     <c>null</c> selects the default (currently <c>btree</c>).
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static string? SetMethod(
        this IConventionIndex index,
        string? method,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(method, nameof(method));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexMethod, method, fromDataAnnotation);

        return method;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index method.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index method.</returns>
    public static ConfigurationSource? GetMethodConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.IndexMethod)?.GetConfigurationSource();

    #endregion Method

    #region Operators

    /// <summary>
    ///     Returns the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static IReadOnlyList<string>? GetOperators(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)index[KdbndpAnnotationNames.IndexOperators];

    /// <summary>
    ///     Sets the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static void SetOperators(this IMutableIndex index, IReadOnlyList<string>? operators)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexOperators, operators);

    /// <summary>
    ///     Sets the column operators to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-opclass.html
    /// </remarks>
    public static IReadOnlyList<string>? SetOperators(
        this IConventionIndex index,
        IReadOnlyList<string>? operators,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(operators, nameof(operators));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexOperators, operators, fromDataAnnotation);

        return operators;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index operators.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index operators.</returns>
    public static ConfigurationSource? GetOperatorsConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.IndexOperators)?.GetConfigurationSource();

    #endregion Operators

    #region Collation

    /// <summary>
    ///     Returns the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-collations.html
    /// </remarks>
#pragma warning disable 618
    public static IReadOnlyList<string>? GetCollation(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)(
            index[RelationalAnnotationNames.Collation] ?? index[KdbndpAnnotationNames.IndexCollation]);
#pragma warning restore 618

    /// <summary>
    ///     Sets the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-collations.html
    /// </remarks>
    public static void SetCollation(this IMutableIndex index, IReadOnlyList<string>? collations)
        => index.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collations);

    /// <summary>
    ///     Sets the column collations to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-collations.html
    /// </remarks>
    public static IReadOnlyList<string>? SetCollation(
        this IConventionIndex index,
        IReadOnlyList<string>? collations,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(collations, nameof(collations));

        index.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collations, fromDataAnnotation);

        return collations;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index collations.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index collations.</returns>
    public static ConfigurationSource? GetCollationConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();

    #endregion Collation

    #region Null sort order

    /// <summary>
    ///     Returns the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static IReadOnlyList<NullSortOrder>? GetNullSortOrder(this IReadOnlyIndex index)
        => (IReadOnlyList<NullSortOrder>?)index[KdbndpAnnotationNames.IndexNullSortOrder];

    /// <summary>
    ///     Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static void SetNullSortOrder(this IMutableIndex index, IReadOnlyList<NullSortOrder>? nullSortOrder)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexNullSortOrder, nullSortOrder);

    /// <summary>
    ///     Sets the column NULL sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    public static IReadOnlyList<NullSortOrder>? SetNullSortOrder(
        this IConventionIndex index,
        IReadOnlyList<NullSortOrder>? nullSortOrder,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(nullSortOrder, nameof(nullSortOrder));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexNullSortOrder, nullSortOrder, fromDataAnnotation);

        return nullSortOrder;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index null sort orders.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index null sort orders.</returns>
    public static ConfigurationSource? GetNullSortOrderConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.IndexNullSortOrder)?.GetConfigurationSource();

    #endregion

    #region Included properties

    /// <summary>
    ///     Returns included property names, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The included property names, or <c>null</c> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index)
        => (IReadOnlyList<string>?)index[KdbndpAnnotationNames.IndexInclude];

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="properties">The value to set.</param>
    public static void SetIncludeProperties(this IMutableIndex index, IReadOnlyList<string>? properties)
        => index.SetOrRemoveAnnotation(
            KdbndpAnnotationNames.IndexInclude,
            properties);

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="properties">The value to set.</param>
    public static IReadOnlyList<string>? SetIncludeProperties(
        this IConventionIndex index,
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(properties, nameof(properties));

        index.SetOrRemoveAnnotation(
            KdbndpAnnotationNames.IndexInclude,
            properties,
            fromDataAnnotation);

        return properties;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the included property names.</returns>
    public static ConfigurationSource? GetIncludePropertiesConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.IndexInclude)?.GetConfigurationSource();

    #endregion Included properties

    #region Created concurrently

    /// <summary>
    ///     Returns a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><c>true</c> if the index is created concurrently.</returns>
    public static bool? IsCreatedConcurrently(this IReadOnlyIndex index)
        => (bool?)index[KdbndpAnnotationNames.CreatedConcurrently];

    /// <summary>
    ///     Sets a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdConcurrently">The value to set.</param>
    public static void SetIsCreatedConcurrently(this IMutableIndex index, bool? createdConcurrently)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.CreatedConcurrently, createdConcurrently);

    /// <summary>
    ///     Sets a value indicating whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdConcurrently">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static bool? SetIsCreatedConcurrently(
        this IConventionIndex index,
        bool? createdConcurrently,
        bool fromDataAnnotation = false)
    {
        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.CreatedConcurrently, createdConcurrently, fromDataAnnotation);

        return createdConcurrently;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index is created concurrently.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is created concurrently.</returns>
    public static ConfigurationSource? GetIsCreatedConcurrentlyConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.CreatedConcurrently)?.GetConfigurationSource();

    #endregion Created concurrently

    #region NULLS distinct

    /// <summary>
    ///     Returns whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static bool? GetAreNullsDistinct(this IReadOnlyIndex index)
        => (bool?)index[KdbndpAnnotationNames.NullsDistinct];

    /// <summary>
    ///     Sets whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static void SetAreNullsDistinct(this IMutableIndex index, bool? nullsDistinct)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.NullsDistinct, nullsDistinct);

    /// <summary>
    ///     Sets whether for a unique index, null values should be considered distinct (not equal).
    ///     The default is that they are distinct, so that a unique index could contain multiple null values in a column.
    /// </summary>
    /// <remarks>
    ///     http://www.KingbaseES.org/docs/current/static/sql-createindex.html
    /// </remarks>
    public static bool? SetAreNullsDistinct(this IConventionIndex index, bool? nullsDistinct, bool fromDataAnnotation = false)
    {
        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.NullsDistinct, nullsDistinct, fromDataAnnotation);

        return nullsDistinct;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether nulls are considered distinct.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetAreNullsDistinctConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.NullsDistinct)?.GetConfigurationSource();

    #endregion NULLS distinct

    #region ToTsVector

    /// <summary>
    ///     Returns the text search configuration for this tsvector expression index, or <c>null</c> if this is not a
    ///     tsvector expression index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static string? GetTsVectorConfig(this IReadOnlyIndex index)
        => (string?)index[KdbndpAnnotationNames.TsVectorConfig];

    /// <summary>
    ///     Sets the text search configuration for this tsvector expression index, or <c>null</c> if this is not a
    ///     tsvector expression index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.KingbaseES.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static void SetTsVectorConfig(this IMutableIndex index, string? config)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.TsVectorConfig, config);

    /// <summary>
    ///     Sets the index to tsvector config name to be used.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="config">
    ///     <para>
    ///         The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    ///         generated tsvector property.
    ///     </para>
    ///     <para>
    ///         See https://www.KingbaseES.org/docs/current/textsearch-controls.html for more information.
    ///     </para>
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/textsearch-tables.html#TEXTSEARCH-TABLES-INDEX
    /// </remarks>
    public static string? SetTsVectorConfig(
        this IConventionIndex index,
        string? config,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(config, nameof(config));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.TsVectorConfig, config, fromDataAnnotation);

        return config;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the tsvector config.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the tsvector config.</returns>
    public static ConfigurationSource? GetTsVectorConfigConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.TsVectorConfig)?.GetConfigurationSource();

    #endregion ToTsVector

    #region Storage parameters

    /// <summary>
    ///     Gets all storage parameters for the index.
    /// </summary>
    public static Dictionary<string, object?> GetStorageParameters(this IReadOnlyIndex index)
        => index.GetAnnotations()
            .Where(a => a.Name.StartsWith(KdbndpAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(KdbndpAnnotationNames.StorageParameterPrefix.Length),
                a => a.Value);

    /// <summary>
    ///     Gets a storage parameter for the index.
    /// </summary>
    public static string? GetStorageParameter(this IIndex index, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return (string?)index[KdbndpAnnotationNames.StorageParameterPrefix + parameterName];
    }

    /// <summary>
    ///     Sets a storage parameter on the index.
    /// </summary>
    public static void SetStorageParameter(this IMutableIndex index, string parameterName, object? parameterValue)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);
    }

    /// <summary>
    ///     Sets a storage parameter on the index.
    /// </summary>
    public static object SetStorageParameter(
        this IConventionIndex index,
        string parameterName,
        object? parameterValue,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);

        return parameterName;
    }

    /// <summary>
    ///     Gets the configuration source for a storage parameter for the table mapped to the entity type.
    /// </summary>
    public static ConfigurationSource? GetStorageParameterConfigurationSource(this IConventionIndex index, string parameterName)
    {
        Check.NotEmpty(parameterName, nameof(parameterName));

        return index.FindAnnotation(KdbndpAnnotationNames.StorageParameterPrefix + parameterName)?.GetConfigurationSource();
    }

    #endregion Storage parameters

    #region Sort order (legacy)

    /// <summary>
    ///     Returns the column sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    [Obsolete("Use IsDescending instead")]
    public static IReadOnlyList<SortOrder>? GetSortOrder(this IReadOnlyIndex index)
        => (IReadOnlyList<SortOrder>?)index[KdbndpAnnotationNames.IndexSortOrder];

    /// <summary>
    ///     Sets the column sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    [Obsolete("Use IsDescending instead")]
    public static void SetSortOrder(this IMutableIndex index, IReadOnlyList<SortOrder>? sortOrder)
        => index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexSortOrder, sortOrder);

    /// <summary>
    ///     Sets the column sort orders to be used, or <c>null</c> if they have not been specified.
    /// </summary>
    /// <remarks>
    ///     https://www.KingbaseES.org/docs/current/static/indexes-ordering.html
    /// </remarks>
    [Obsolete("Use IsDescending instead")]
    public static IReadOnlyList<SortOrder>? SetSortOrder(
        this IConventionIndex index,
        IReadOnlyList<SortOrder>? sortOrder,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(sortOrder, nameof(sortOrder));

        index.SetOrRemoveAnnotation(KdbndpAnnotationNames.IndexSortOrder, sortOrder, fromDataAnnotation);

        return sortOrder;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the index sort orders.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index sort orders.</returns>
    [Obsolete("Use IsDescending instead")]
    public static ConfigurationSource? GetSortOrderConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(KdbndpAnnotationNames.IndexSortOrder)?.GetConfigurationSource();

    #endregion Sort order (legacy)
}
