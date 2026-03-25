using Domain.Entities;
using Infrastructure.Database.Context;  // Ссылка на контекст
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seeders;  // ✅ Новый namespace

/// <summary>
/// Сидер для начального наполнения таблицы продуктов.
/// </summary>
public static class ProductSeeder
{
    public static async Task SeedAsync(ProductDbContext context, CancellationToken ct = default)
    {
        // 🔍 Проверяем, есть ли уже данные (идемпотентность)
        if (await context.Products.AnyAsync(ct))
            return;

        var products = new[]
        {
            Product.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "Ноутбук Gaming Pro",
                "Мощный игровой ноутбук с видеокартой RTX 4070, 32 ГБ ОЗУ и SSD 1 ТБ",
                14999000,
                15),

            Product.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "Смартфон Ultra X",
                "Флагманский смартфон с AMOLED-экраном 6.7\", камерой 200 МП и батареей 5000 мАч",
                8999000,
                42),

            Product.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000003"),
                "Беспроводные наушники SoundMax",
                "Наушники с активным шумоподавлением, временем работы до 30 часов и поддержкой Hi-Res Audio",
                2499000,
                100),

            Product.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000004"),
                "Умные часы FitTrack Pro",
                "Фитнес-трекер с GPS, мониторингом сна, пульса и SpO2, водонепроницаемость 5 ATM",
                1999000,
                67),

            Product.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                "Планшет WorkTab 12",
                "Планшет для работы и творчества: экран 12.4\", стилус в комплекте, 256 ГБ памяти",
                5499000,
                23)
        };

        await context.Products.AddRangeAsync(products, ct);
        await context.SaveChangesAsync(ct);
    }
}
