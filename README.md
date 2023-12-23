# DotNetCore Entity Framework Core provider for KingbaseES for R6 V008R006C008B0014

[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore) 
[![nuget](https://img.shields.io/nuget/v/DotNetCore.EntityFrameworkCore.KingbaseES.svg?style=flat-square)](https://www.nuget.org/packages/DotNetCore.EntityFrameworkCore.KingbaseES) 
[![stats](https://img.shields.io/nuget/dt/DotNetCore.EntityFrameworkCore.KingbaseES.svg?style=flat-square)](https://www.nuget.org/stats/packages/DotNetCore.EntityFrameworkCore.KingbaseES?groupby=Version) 

`DotNetCore.EntityFrameworkCore.KingbaseES` is an open source Entity Framework Core provider for KingbaseES. It supports you to interact with KingbaseES via EFCore, the most widely-used .NET O/RM from Microsoft, up to its latest version, and use familiar LINQ syntax to express queries.

## Getting Started

### 1. Project Configuration

Ensure that your `.csproj` file contains the following reference:

```xml
<PackageReference Include="DotNetCore.EntityFrameworkCore.KingbaseES" Version="6.0.22" />
```

### 2. Services Configuration

Add `DotNetCore.EntityFrameworkCore.KingbaseES` to the services configuration in your the `Startup.cs` file of your ASP.NET Core project:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Replace with your connection string.
        var connectionString = "host={host};port={port};database={database};username={username};password={password};";

        // Replace 'YourDbContext' with the name of your own DbContext derived class.
        services.AddDbContext<YourDbContext>(
            dbContextOptions => dbContextOptions
                .UseKdbndp(connectionString)
        );
    }
}
```

### 3. Sample Application

Check out our [Basic Test](https://github.com/dotnetcore/EntityFrameworkCore.KingbaseES/tree/main/test/KingbaseES.BasicTest) for an example repository that includes an ASP.NET Core MVC Application.

There are also many complete and concise console application samples posted in the issue section (some of them can be found by searching for `Program.cs`).

### 4. Read the EF Core Documentation

Refer to Microsoft's [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/) for detailed instructions and examples on using EF Core.

## Scaffolding / Reverse Engineering

Use the [EF Core tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) to execute scaffolding commands:

```
dotnet ef dbcontext scaffold "Server=localhost;User=root;Password=1234;Database=ef" "DotNetCore.EntityFrameworkCore.KingbaseES"
```

## Contribute

One of the easiest ways to contribute is to report issues and participate in discussions. You can also contribute by submitting pull requests with code changes and supporting tests.

We are always looking for additional core contributors. If you got a couple of hours a week and know your way around EF Core and KingbaseES, give us a nudge.

## License

[PostgreSQL license](https://github.com/dotnetcore/EntityFrameworkCore.KingbaseES/blob/main/LICENSE)
