using System.Globalization;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.ValueGeneration.Internal;

/// <summary>
///     This API supports the Entity Framework Core infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
public class KdbndpSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
{
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IUpdateSqlGenerator _sqlGenerator;
    private readonly IKdbndpRelationalConnection _connection;
    private readonly ISequence _sequence;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public KdbndpSequenceHiLoValueGenerator(
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IUpdateSqlGenerator sqlGenerator,
        KdbndpSequenceValueGeneratorState generatorState,
        IKdbndpRelationalConnection connection,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(generatorState)
    {
        _sequence = generatorState.Sequence;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _sqlGenerator = sqlGenerator;
        _connection = connection;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    protected override long GetNewLowValue()
        => (long)Convert.ChangeType(
            _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalar(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger)),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    protected override async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
        => (long)Convert.ChangeType(
            await _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger),
                    cancellationToken).ConfigureAwait(false),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public override bool GeneratesTemporaryValues
        => false;
}
