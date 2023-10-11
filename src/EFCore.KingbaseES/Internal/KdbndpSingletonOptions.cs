using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Internal;

/// <inheritdoc />
public class KdbndpSingletonOptions : IKdbndpSingletonOptions
{
    public static readonly Version DefaultPostgresVersion = new(12, 0);

    /// <inheritdoc />
    public virtual Version PostgresVersion { get; private set; } = null!;

    /// <inheritdoc />
    public virtual Version? PostgresVersionWithoutDefault { get; private set; }

    /// <inheritdoc />
    public virtual bool UseRedshift { get; private set; }

    /// <inheritdoc />
    public virtual bool ReverseNullOrderingEnabled { get; private set; }

    /// <inheritdoc />
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; private set; }

    public KdbndpSingletonOptions()
        => UserRangeDefinitions = new UserRangeDefinition[0];

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var KdbndpOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();

        PostgresVersionWithoutDefault = KdbndpOptions.PostgresVersion;
        PostgresVersion = KdbndpOptions.PostgresVersion ?? DefaultPostgresVersion;
        UseRedshift = KdbndpOptions.UseRedshift;
        ReverseNullOrderingEnabled = KdbndpOptions.ReverseNullOrdering;
        UserRangeDefinitions = KdbndpOptions.UserRangeDefinitions;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var KdbndpOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();

        if (PostgresVersionWithoutDefault != KdbndpOptions.PostgresVersion)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.SetPostgresVersion),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UseRedshift != KdbndpOptions.UseRedshift)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.UseRedshift),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (ReverseNullOrderingEnabled != KdbndpOptions.ReverseNullOrdering)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.ReverseNullOrdering),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UserRangeDefinitions.Count != KdbndpOptions.UserRangeDefinitions.Count
            || UserRangeDefinitions.Zip(KdbndpOptions.UserRangeDefinitions).Any(t => t.First != t.Second))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.MapRange),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}
