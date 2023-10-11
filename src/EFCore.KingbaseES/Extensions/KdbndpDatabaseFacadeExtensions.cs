using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Kdbndp specific extension methods for <see cref="DbContext.Database" />.
/// </summary>
public static class KdbndpDatabaseFacadeExtensions
{
    /// <summary>
    /// <para>
    /// Returns true if the database provider currently in use is the Kdbndp provider.
    /// </para>
    /// <para>
    /// This method can only be used after the <see cref="DbContext" /> has been configured because
    /// it is only then that the provider is known. This means that this method cannot be used
    /// in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    /// provider to use as part of configuring the context.
    /// </para>
    /// </summary>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns>True if Kdbndp is being used; false otherwise.</returns>
    public static bool IsKdbndp(this DatabaseFacade database)
        => database.ProviderName == typeof(KdbndpOptionsExtension).GetTypeInfo().Assembly.GetName().Name;
}
