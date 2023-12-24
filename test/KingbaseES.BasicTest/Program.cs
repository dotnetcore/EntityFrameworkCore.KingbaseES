namespace KingbaseES.BasicTest;

internal static class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddDbContext<BlogContext>(options =>
        {
            options.UseKdbndp(@"host=localhost;port=54321;database=test;user id=system;password=123456;");   
        });

        var serviceProvider = services.BuildServiceProvider();

        var context = serviceProvider.GetRequiredService<BlogContext>();

        context.Database.EnsureCreated();

        // get list
        var blogs = context.Blogs.ToList();
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "当前 Blogs 表中条目数：" + blogs.Count);
        if (blogs.Any())
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "当前 Blogs 表中条目数大于1，进行清理...");
            context.Blogs.RemoveRange(blogs);
            context.SaveChanges();
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "清理完成！");
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "当前 Blogs 表中条目数：" + blogs.Count);
        }
        //add
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "新增 Blog 实体 值为I love EFCore!");
        context.Add(new Blog() { Name = "I love EFCore!" });
        var result = context.SaveChanges();

        //update
        var first = context.Blogs.FirstOrDefault();
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "查询结果:" + first.Name);

        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "变更 Blog 实体 值为I love EFCore too!");
        first.Name = "I love EFCore too!";
        context.SaveChanges();

        // get
        var testselect = context.Blogs.FirstOrDefault();
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "查询结果:" + testselect.Name);
        Console.ReadKey();
    }
}
