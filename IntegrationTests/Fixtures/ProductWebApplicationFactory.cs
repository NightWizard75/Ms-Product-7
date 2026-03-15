using Infrastructure.Database.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Фабрика приложения для интеграционных тестов.
/// Запускает реальное приложение с тестовой БД в контейнере.
/// </summary>
public class ProductWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("product_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithCleanUp(true)
        .Build();

    public string TestConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 🔧 Переопределяем строку подключения на тестовую
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(TestConnectionString, 
                    b => b.MigrationsAssembly(typeof(ProductDbContext).Assembly.FullName)));
        });

        // 🔧 Отключаем авто-сидинг в тестах
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:AutoSeed"] = "false",
                ["Database:AutoMigrate"] = "true"
            });
        });
    }

    public async Task InitializeAsync()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    public ProductDbContext GetDbContext() => Services.GetRequiredService<ProductDbContext>();
}
