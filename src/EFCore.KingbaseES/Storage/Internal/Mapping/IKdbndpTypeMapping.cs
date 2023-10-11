using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public interface IKdbndpTypeMapping
{
    /// <summary>
    /// The database type used by Kdbndp.
    /// </summary>
    KdbndpDbType KdbndpDbType { get; }
}
