using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

public class KdbndpCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
{
    public KdbndpCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override object GenerateCacheKey(Expression query, bool async)
        => new KdbndpCompiledQueryCacheKey(
            GenerateCacheKeyCore(query, async),
            RelationalDependencies.ContextOptions.FindExtension<KdbndpOptionsExtension>()?.ReverseNullOrdering ?? false);

    private struct KdbndpCompiledQueryCacheKey
    {
        private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
        private readonly bool _reverseNullOrdering;

        public KdbndpCompiledQueryCacheKey(
            RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey, bool reverseNullOrdering)
        {
            _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
            _reverseNullOrdering = reverseNullOrdering;
        }

        public override bool Equals(object? obj)
            => !(obj is null)
                && obj is KdbndpCompiledQueryCacheKey key
                && Equals(key);

        private bool Equals(KdbndpCompiledQueryCacheKey other)
            => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                && _reverseNullOrdering == other._reverseNullOrdering;

        public override int GetHashCode() => HashCode.Combine(_relationalCompiledQueryCacheKey, _reverseNullOrdering);
    }
}
