using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.Seeders;

public class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider, 
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        if (configuration.GetValue<bool>("Database:AutoMigrate", true))
            await context.Database.MigrateAsync(ct);

        if (configuration.GetValue<bool>("Database:AutoSeed", true))
            await ProductSeeder.SeedAsync(context, ct);
    }
}
