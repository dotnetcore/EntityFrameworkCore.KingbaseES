using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Update.Internal;

public class KdbndpModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private readonly ModificationCommandBatchFactoryDependencies _dependencies;
    private readonly IDbContextOptions _options;

    public KdbndpModificationCommandBatchFactory(
        ModificationCommandBatchFactoryDependencies dependencies,
        IDbContextOptions options)
    {
        Check.NotNull(dependencies, nameof(dependencies));
        Check.NotNull(options, nameof(options));

        _dependencies = dependencies;
        _options = options;
    }

    public virtual ModificationCommandBatch Create()
    {
        var optionsExtension = _options.Extensions.OfType<KdbndpOptionsExtension>().FirstOrDefault();

        return new KdbndpModificationCommandBatch(_dependencies, optionsExtension?.MaxBatchSize);
    }
}
