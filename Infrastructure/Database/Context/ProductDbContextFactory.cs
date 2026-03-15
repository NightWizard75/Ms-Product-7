using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Database.Context;

/// <summary>
/// Фабрика для создания DbContext при дизайне (миграции, EF Core CLI).
/// Не используется в runtime — только для dotnet ef commands.
/// </summary>
public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        // 🔧 Читаем конфигурацию из Web проекта (appsettings.json)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        
        // 🔧 Используем ту же строку подключения, что и в runtime
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Host=localhost;Database=product_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, 
            b => b.MigrationsAssembly(typeof(ProductDbContext).Assembly.FullName));

        return new ProductDbContext(optionsBuilder.Options);
    }
}
