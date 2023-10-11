using Microsoft.EntityFrameworkCore.Query;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpQueryCompilationContext : RelationalQueryCompilationContext
{
    public KdbndpQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies, bool async)
        : base(dependencies, relationalDependencies, async)
    {
    }

    public override bool IsBuffering
        => base.IsBuffering ||
            QuerySplittingBehavior == Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery;
}
