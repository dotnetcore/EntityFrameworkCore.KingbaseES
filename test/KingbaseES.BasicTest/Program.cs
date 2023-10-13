using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KingbaseES.BasicTest
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddDbContext<BlogContext>(options => options.UseKdbndp(@"host=localhost;port=54321;database=test;user id=system;password=123456;"));

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetRequiredService<BlogContext>();

            // get list
            var blogs = context.Blogs.ToList();

            //add
            context.Add(new Blog() { Name = "jeffcky" });
            var result = context.SaveChanges();

            //update
            var first = context.Blogs.FirstOrDefault();
            first.Name = "jeffcky2";
            context.SaveChanges();

            Console.ReadKey();
        }
    }
}
