using Microsoft.EntityFrameworkCore;

namespace KingbaseES.BasicTest
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options):base(options) 
        {
            
        }
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence("tests_id_seq")
                .HasMin(1).IncrementsBy(1).HasMax(long.MaxValue).StartsAt(1);

            modelBuilder.Entity<Blog>(b =>
            {
                b.Property(p => p.Id).HasDefaultValueSql("nextval('tests_id_seq'::regclass)");
            });
        }
    }
}
