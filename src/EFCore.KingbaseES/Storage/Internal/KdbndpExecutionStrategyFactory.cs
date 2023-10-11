using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

public class KdbndpExecutionStrategyFactory : RelationalExecutionStrategyFactory
{
    public KdbndpExecutionStrategyFactory(
        ExecutionStrategyDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
        => new KdbndpExecutionStrategy(dependencies);
}
