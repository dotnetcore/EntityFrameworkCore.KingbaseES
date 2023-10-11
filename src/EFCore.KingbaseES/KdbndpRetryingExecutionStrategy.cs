using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES;

public class KdbndpRetryingExecutionStrategy : ExecutionStrategy
{
    private readonly ICollection<string>? _additionalErrorCodes;

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="context"> The context on which the operations will be invoked. </param>
    /// <remarks>
    ///     The default retry limit is 6, which means that the total amount of time spent before failing is about a minute.
    /// </remarks>
    public KdbndpRetryingExecutionStrategy(
        DbContext context)
        : this(context, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="dependencies"> Parameter object containing service dependencies. </param>
    public KdbndpRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies)
        : this(dependencies, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="context"> The context on which the operations will be invoked. </param>
    /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
    public KdbndpRetryingExecutionStrategy(
        DbContext context,
        int maxRetryCount)
        : this(context, maxRetryCount, DefaultMaxDelay, errorCodesToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="dependencies"> Parameter object containing service dependencies. </param>
    /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
    public KdbndpRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount)
        : this(dependencies, maxRetryCount, DefaultMaxDelay, errorCodesToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="context"> The context on which the operations will be invoked. </param>
    /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
    /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
    /// <param name="errorCodesToAdd"> Additional error codes that should be considered transient. </param>
    public KdbndpRetryingExecutionStrategy(
        DbContext context,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<string>? errorCodesToAdd)
        : base(context,
            maxRetryCount,
            maxRetryDelay)
        =>  _additionalErrorCodes = errorCodesToAdd;

    /// <summary>
    ///     Creates a new instance of <see cref="KdbndpRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="dependencies"> Parameter object containing service dependencies. </param>
    /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
    /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
    /// <param name="errorCodesToAdd"> Additional SQL error numbers that should be considered transient. </param>
    public KdbndpRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<string>? errorCodesToAdd)
        : base(dependencies, maxRetryCount, maxRetryDelay)
        => _additionalErrorCodes = errorCodesToAdd;

    // TODO: Unlike SqlException, which seems to also wrap various transport/IO errors
    // and expose them via error codes, we have KdbndpException with an inner exception.
    // Would be good to provide a way to add these into the additional list.
    protected override bool ShouldRetryOn(Exception? exception)
        => exception is KingbaseException KingbaseException &&
            _additionalErrorCodes?.Contains(KingbaseException.SqlState) == true
            || KdbndpTransientExceptionDetector.ShouldRetryOn(exception);
}
