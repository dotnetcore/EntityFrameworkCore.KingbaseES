using Microsoft.EntityFrameworkCore.Storage;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

public interface IKdbndpRelationalConnection : IRelationalConnection
{
    IKdbndpRelationalConnection CreateMasterConnection();

    KdbndpRelationalConnection CloneWith(string connectionString);
}
