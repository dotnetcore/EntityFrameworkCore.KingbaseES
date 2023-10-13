using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KingbaseES.BasicTest
{
    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogContext>
    {
        public BlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            optionsBuilder.UseKdbndp("host=localhost;port=54321;database=test;username=system;password=123456;");

            return new BlogContext(optionsBuilder.Options);
        }
    }
}
