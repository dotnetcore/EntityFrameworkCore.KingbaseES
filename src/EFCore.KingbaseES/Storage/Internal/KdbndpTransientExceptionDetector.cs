using System;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

/// <summary>
///     Detects the exceptions caused by KingbaseES or network transient failures.
/// </summary>
public class KdbndpTransientExceptionDetector
{
    public static bool ShouldRetryOn(Exception? ex)
        => (ex as KdbndpException)?.IsTransient == true || ex is TimeoutException;
}
