using System.Data.Common;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Kdbndp specific extension methods for <see cref="DbContext.Database" />.
/// </summary>
public static class KdbndpDatabaseFacadeExtensions
{
    /// <summary>
    ///     <para>
    ///         Returns true if the database provider currently in use is the Kdbndp provider.
    ///     </para>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    /// </summary>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns>True if Kdbndp is being used; false otherwise.</returns>
    public static bool IsKdbndp(this DatabaseFacade database)
        => database.ProviderName == typeof(KdbndpOptionsExtension).GetTypeInfo().Assembly.GetName().Name;

    /// <summary>
    ///     Sets the underlying <see cref="DbDataSource" /> configured for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It may not be possible to change the data source if existing connection, if any, is open.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="dataSource">The connection string.</param>
    public static void SetDbDataSource(this DatabaseFacade databaseFacade, DbDataSource dataSource)
        => ((KdbndpRelationalConnection)GetFacadeDependencies(databaseFacade).RelationalConnection).DbDataSource = dataSource;

    private static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(DatabaseFacade databaseFacade)
    {
        var dependencies = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies;

        if (dependencies is IRelationalDatabaseFacadeDependencies relationalDependencies)
        {
            return relationalDependencies;
        }

        throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
    }
}
