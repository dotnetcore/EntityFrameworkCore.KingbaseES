using Microsoft.EntityFrameworkCore.Query;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
{
    private readonly RelationalSqlTranslatingExpressionVisitorDependencies _dependencies;

    public KdbndpSqlTranslatingExpressionVisitorFactory(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new KdbndpSqlTranslatingExpressionVisitor(
            _dependencies,
            queryCompilationContext,
            queryableMethodTranslatingExpressionVisitor);
}
