using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.Seeders;

public class SeedCommand
{
    public static async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        Console.WriteLine("🌱 Запуск сидинга продуктов...");

        await ProductSeeder.SeedAsync(context, ct);

        var count = await context.Products.CountAsync(ct);
        Console.WriteLine($"✅ Сидинг продуктов завершён. Всего продуктов: {count}");
    }
}
