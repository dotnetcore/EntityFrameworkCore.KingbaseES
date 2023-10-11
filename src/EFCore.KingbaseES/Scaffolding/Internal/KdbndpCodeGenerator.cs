using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Scaffolding.Internal;

/// <summary>
/// The default code generator for Kdbndp.
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
    /// Constructs an instance of the <see cref="KdbndpCodeGenerator"/> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public KdbndpCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies) {}

    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            _useKdbndpMethodInfo,
            providerOptions is null
                ? new object[] { connectionString }
                : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
}
