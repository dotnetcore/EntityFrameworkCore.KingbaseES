using Microsoft.EntityFrameworkCore.Query;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

/// <summary>
/// The default factory for Kdbndp-specific query SQL generators.
/// </summary>
public class KdbndpQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly IKdbndpSingletonOptions _KdbndpSingletonOptions;

    public KdbndpQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
    {
        _dependencies = dependencies;
        _KdbndpSingletonOptions = KdbndpSingletonOptions;
    }

    public virtual QuerySqlGenerator Create()
        => new KdbndpQuerySqlGenerator(
            _dependencies,
            _KdbndpSingletonOptions.ReverseNullOrderingEnabled,
            _KdbndpSingletonOptions.PostgresVersion);
}
