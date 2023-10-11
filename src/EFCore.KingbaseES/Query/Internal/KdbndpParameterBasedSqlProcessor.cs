using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
{
    public KdbndpParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
    {
    }

    /// <inheritdoc />
    protected override SelectExpression ProcessSqlNullability(
        SelectExpression selectExpression, IReadOnlyDictionary<string, object?> parametersValues, out bool canCache)
    {
        Check.NotNull(selectExpression, nameof(selectExpression));
        Check.NotNull(parametersValues, nameof(parametersValues));

        return new KdbndpSqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(selectExpression, parametersValues, out canCache);
    }
}
