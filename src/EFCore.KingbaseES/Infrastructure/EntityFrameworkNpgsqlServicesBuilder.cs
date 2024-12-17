namespace Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;

/// <summary>
///     A builder API designed for Kdbndp when registering services.
/// </summary>
public class EntityFrameworkKdbndpServicesBuilder : EntityFrameworkRelationalServicesBuilder
{
    private static readonly IDictionary<Type, ServiceCharacteristics> KdbndpServices
        = new Dictionary<Type, ServiceCharacteristics>
        {
            {
                typeof(IKdbndpDataSourceConfigurationPlugin),
                new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true)
            }
        };

    /// <summary>
    ///     Used by relational database providers to create a new <see cref="EntityFrameworkRelationalServicesBuilder" /> for
    ///     registration of provider services.
    /// </summary>
    /// <param name="serviceCollection">The collection to which services will be registered.</param>
    public EntityFrameworkKdbndpServicesBuilder(IServiceCollection serviceCollection)
        : base(serviceCollection)
    {
    }

    /// <summary>
    ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
    /// </summary>
    /// <param name="serviceType">The type that defines the service API.</param>
    /// <returns>The <see cref="ServiceCharacteristics" /> for the type or <see langword="null" /> if it's not an EF service.</returns>
    protected override ServiceCharacteristics? TryGetServiceCharacteristics(Type serviceType)
        => KdbndpServices.TryGetValue(serviceType, out var characteristics)
            ? characteristics
            : base.TryGetServiceCharacteristics(serviceType);
}
