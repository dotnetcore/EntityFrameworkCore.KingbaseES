using Microsoft.EntityFrameworkCore.Query;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
{
    private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

    public KdbndpParameterBasedSqlProcessorFactory(
        RelationalParameterBasedSqlProcessorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
        => new KdbndpParameterBasedSqlProcessor(_dependencies, useRelationalNulls);
}
