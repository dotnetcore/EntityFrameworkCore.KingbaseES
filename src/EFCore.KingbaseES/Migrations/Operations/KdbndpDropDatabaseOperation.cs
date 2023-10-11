using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Migrations.Operations;

public class KdbndpDropDatabaseOperation : MigrationOperation
{
    public virtual string Name { get; set; } = null!;
}
