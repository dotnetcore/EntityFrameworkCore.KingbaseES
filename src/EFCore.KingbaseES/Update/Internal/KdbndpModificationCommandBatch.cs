using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Update.Internal;

/// <summary>
/// The Kdbndp-specific implementation for <see cref="ModificationCommandBatch" />.
/// </summary>
/// <remarks>
/// The usual ModificationCommandBatch implementation is <see cref="AffectedCountModificationCommandBatch"/>,
/// which selects the number of rows modified via a SQL query.
///
/// KingbaseES actually has no way of selecting the modified row count.
/// SQL defines GET DIAGNOSTICS which should provide this, but in KingbaseES it's only available
/// in PL/pgSQL. See http://www.KingbaseES.org/docs/9.4/static/unsupported-features-sql-standard.html,
/// identifier F121-01.
///
/// Instead, the affected row count can be accessed in the KingbaseES protocol itself, which seems
/// cleaner and more efficient anyway (no additional query).
/// </remarks>
public class KdbndpModificationCommandBatch : ReaderModificationCommandBatch
{
    private const int DefaultBatchSize = 1000;
    private readonly int _maxBatchSize;
    private int _parameterCount;

    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpModificationCommandBatch"/> class.
    /// </summary>
    /// <param name="commandBuilderFactory">The builder to build commands.</param>
    /// <param name="sqlGenerationHelper">A helper for SQL generation.</param>
    /// <param name="updateSqlGenerator">A SQL generator for insert, update, and delete commands.</param>
    /// <param name="valueBufferFactoryFactory">A factory for creating <see cref="ValueBuffer" /> factories.</param>
    /// <param name="maxBatchSize">The maximum count of commands to batch.</param>
    public KdbndpModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int? maxBatchSize)
        : base(dependencies)
    {
        if (maxBatchSize.HasValue && maxBatchSize.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize(maxBatchSize));
        }

        _maxBatchSize = maxBatchSize ?? DefaultBatchSize;
    }

    protected override int GetParameterCount() => _parameterCount;

    protected override bool CanAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (ModificationCommands.Count >= _maxBatchSize)
        {
            return false;
        }

        var newParamCount = (long)_parameterCount + modificationCommand.ColumnModifications.Count;
        if (newParamCount > int.MaxValue)
        {
            return false;
        }

        _parameterCount = (int)newParamCount;
        return true;
    }

    protected override bool IsCommandTextValid()
        => true;

    protected override void Consume(RelationalDataReader reader)
    {
        var KdbndpReader = (KdbndpDataReader)reader.DbDataReader;

#pragma warning disable 618
        Debug.Assert(KdbndpReader.Statements.Count == ModificationCommands.Count, $"Reader has {KdbndpReader.Statements.Count} statements, expected {ModificationCommands.Count}");
#pragma warning restore 618

        var commandIndex = 0;

        try
        {
            while (true)
            {
                // Find the next propagating command, if any
                int nextPropagating;
                for (nextPropagating = commandIndex;
                     nextPropagating < ModificationCommands.Count &&
                     !ModificationCommands[nextPropagating].RequiresResultPropagation;
                     nextPropagating++)
                {
                }

                // Go over all non-propagating commands before the next propagating one,
                // make sure they executed
                for (; commandIndex < nextPropagating; commandIndex++)
                {
#pragma warning disable 618
                    if (KdbndpReader.Statements[commandIndex].Rows == 0)
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            ModificationCommands[commandIndex].Entries
                        );
                    }
#pragma warning restore 618
                }

                if (nextPropagating == ModificationCommands.Count)
                {
                    Debug.Assert(!KdbndpReader.NextResult(), "Expected less resultsets");
                    break;
                }

                // Propagate to results from the reader to the ModificationCommand

                var modificationCommand = ModificationCommands[commandIndex++];

                if (!reader.Read())
                {
                    throw new DbUpdateConcurrencyException(
                        RelationalStrings.UpdateConcurrencyException(1, 0),
                        modificationCommand.Entries);
                }

                var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                modificationCommand.PropagateResults(valueBufferFactory.Create(KdbndpReader));

                KdbndpReader.NextResult();
            }
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }

    protected override async Task ConsumeAsync(
        RelationalDataReader reader,
        CancellationToken cancellationToken = default)
    {
        var KdbndpReader = (KdbndpDataReader)reader.DbDataReader;

#pragma warning disable 618
        Debug.Assert(KdbndpReader.Statements.Count == ModificationCommands.Count, $"Reader has {KdbndpReader.Statements.Count} statements, expected {ModificationCommands.Count}");
#pragma warning restore 618

        var commandIndex = 0;

        try
        {
            while (true)
            {
                // Find the next propagating command, if any
                int nextPropagating;
                for (nextPropagating = commandIndex;
                     nextPropagating < ModificationCommands.Count &&
                     !ModificationCommands[nextPropagating].RequiresResultPropagation;
                     nextPropagating++)
                {
                    ;
                }

                // Go over all non-propagating commands before the next propagating one,
                // make sure they executed
                for (; commandIndex < nextPropagating; commandIndex++)
                {
#pragma warning disable 618
                    if (KdbndpReader.Statements[commandIndex].Rows == 0)
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            ModificationCommands[commandIndex].Entries
                        );
                    }
#pragma warning restore 618
                }

                if (nextPropagating == ModificationCommands.Count)
                {
                    Debug.Assert(!(await KdbndpReader.NextResultAsync(cancellationToken).ConfigureAwait(false)), "Expected less resultsets");
                    break;
                }

                // Extract result from the command and propagate it

                var modificationCommand = ModificationCommands[commandIndex++];

                if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(false)))
                {
                    throw new DbUpdateConcurrencyException(
                        RelationalStrings.UpdateConcurrencyException(1, 0),
                        modificationCommand.Entries
                    );
                }

                var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                modificationCommand.PropagateResults(valueBufferFactory.Create(KdbndpReader));

                await KdbndpReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }
}
