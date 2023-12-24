using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Scaffolding.Internal;

/// <summary>
///     The default code generator for Kdbndp.
/// </summary>
public class KdbndpCodeGenerator : ProviderCodeGenerator
{
    private static readonly MethodInfo _useKdbndpMethodInfo
        = typeof(KdbndpDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(KdbndpDbContextOptionsBuilderExtensions.UseKdbndp),
            typeof(DbContextOptionsBuilder),
            typeof(string),
            typeof(Action<KdbndpDbContextOptionsBuilder>));

    /// <summary>
    ///     Constructs an instance of the <see cref="KdbndpCodeGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public KdbndpCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            _useKdbndpMethodInfo,
            providerOptions is null
                ? new object[] { connectionString }
                : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
}
