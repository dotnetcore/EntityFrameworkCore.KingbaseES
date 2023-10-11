using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Migrations.Operations;

public class KdbndpCreateDatabaseOperation : DatabaseOperation
{
    public virtual string Name { get; set; } = null!;
    public virtual string? Template { get; set; }
    public virtual string? Tablespace { get; set; }
}
