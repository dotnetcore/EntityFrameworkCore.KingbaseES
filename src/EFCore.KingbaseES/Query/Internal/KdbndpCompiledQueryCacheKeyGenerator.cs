using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KdbndpCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object GenerateCacheKey(Expression query, bool async)
        => new KdbndpCompiledQueryCacheKey(
            GenerateCacheKeyCore(query, async),
            RelationalDependencies.ContextOptions.FindExtension<KdbndpOptionsExtension>()?.ReverseNullOrdering ?? false);

    private struct KdbndpCompiledQueryCacheKey
    {
        private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
        private readonly bool _reverseNullOrdering;

        public KdbndpCompiledQueryCacheKey(
            RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
            bool reverseNullOrdering)
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

        public override int GetHashCode()
            => HashCode.Combine(_relationalCompiledQueryCacheKey, _reverseNullOrdering);
    }
}
