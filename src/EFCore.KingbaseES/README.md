# Entity Framework Core provider for KingbaseES

DotNetCore.EntityFrameworkCore.KingbaseES is the open source EF Core provider for KingbaseES. It allows you to interact with KingbaseES via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries. It's built on top of [KingbaseES](https://github.com/dotnetcore/EntityFrameworkCore.KingbaseES).

The provider looks and feels just like any other Entity Framework Core provider. Here's a quick sample to get you started:

```csharp
await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Insert a Blog
ctx.Blogs.Add(new() { Name = "FooBlog" });
await ctx.SaveChangesAsync();

// Query all blogs who's name starts with F
var fBlogs = await ctx.Blogs.Where(b => b.Name.StartsWith("F")).ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseKdbndp(@"host={host};port={port};database={database};username={username};password={password};");
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```