using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Kdbndp.EntityFrameworkCore.KingbaseES.Scaffolding.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Design.Internal;

public class KdbndpDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        serviceCollection.AddEntityFrameworkKdbndp();
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<IAnnotationCodeGenerator, KdbndpAnnotationCodeGenerator>()
            .TryAdd<IDatabaseModelFactory, KdbndpDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, KdbndpCodeGenerator>()
            .TryAddCoreServices();
    }
}
