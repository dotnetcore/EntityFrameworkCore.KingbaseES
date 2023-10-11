using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpQueryCompilationContextFactory : IQueryCompilationContextFactory
{
    private readonly QueryCompilationContextDependencies _dependencies;
    private readonly RelationalQueryCompilationContextDependencies _relationalDependencies;

    public KdbndpQueryCompilationContextFactory(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies)
    {
        Check.NotNull(dependencies, nameof(dependencies));
        Check.NotNull(relationalDependencies, nameof(relationalDependencies));

        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryCompilationContext Create(bool async)
        => new KdbndpQueryCompilationContext(_dependencies, _relationalDependencies, async);
}
