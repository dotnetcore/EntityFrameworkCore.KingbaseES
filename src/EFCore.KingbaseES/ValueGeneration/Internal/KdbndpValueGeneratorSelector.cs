using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.ValueGeneration.Internal;

public class KdbndpValueGeneratorSelector : RelationalValueGeneratorSelector
{
    private readonly IKdbndpSequenceValueGeneratorFactory _sequenceFactory;
    private readonly IKdbndpRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    public KdbndpValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        IKdbndpSequenceValueGeneratorFactory sequenceFactory,
        IKdbndpRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(dependencies)
    {
        _sequenceFactory = sequenceFactory;
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public new virtual IKdbndpValueGeneratorCache Cache => (IKdbndpValueGeneratorCache)base.Cache;

    public override ValueGenerator Select(IProperty property, IEntityType entityType)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(entityType, nameof(entityType));

        return property.GetValueGeneratorFactory() is null
            && property.GetValueGenerationStrategy() == KdbndpValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(
                    property,
                    Cache.GetOrAddSequenceState(property, _connection),
                    _connection,
                    _rawSqlCommandBuilder,
                    _commandLogger)
                : base.Select(property, entityType);
    }

    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(entityType, nameof(entityType));

        // Generate temporary values if the user specified a default value (to allow
        // generating server-side with uuid-ossp or whatever)
        return property.ClrType.UnwrapNullableType() == typeof(Guid)
            ? property.ValueGenerated == ValueGenerated.Never
            || property.GetDefaultValueSql() is not null
                ? new TemporaryGuidValueGenerator()
                : new GuidValueGenerator()
            : base.Create(property, entityType);
    }
}
