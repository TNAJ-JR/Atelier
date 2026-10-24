using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atelier.Data;

public class AtelierDbContextFactory : IDesignTimeDbContextFactory<AtelierDbContext>
{
    public AtelierDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<AtelierDbContextFactory>(optional: true)
            .AddEnvironmentVariables().AddCommandLine(args).Build();
        var connection = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection manquante.");
        return new(new DbContextOptionsBuilder<AtelierDbContext>().UseSqlServer(connection).Options);
    }
}
