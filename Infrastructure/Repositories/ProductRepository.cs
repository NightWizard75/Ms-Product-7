using Application.Shared.Interfaces;
using Domain.Entities;
using Infrastructure.Database.Context;
using Microsoft.Extensions.Logging; 

namespace Infrastructure.Repositories;

public class ProductRepository(
    ProductDbContext context,
    ILogger<ProductRepository> logger)
    : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogDebug("Запрос продукта с Id={ProductId}", id);
        
        var product = await context.Products.FindAsync([id], ct);
        
        if (product is null)
            logger.LogWarning("Продукт с Id={ProductId} не найден", id);
        
        return product;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        logger.LogInformation("Создание продукта: {ProductId}, Name={Name}", 
            product.Id, product.Name);
        
        await context.Products.AddAsync(product, ct);
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Продукт {ProductId} успешно создан", product.Id);
        
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        logger.LogDebug("Обновление продукта: {ProductId}", product.Id);
        
        context.Products.Update(product);
        await context.SaveChangesAsync(ct);
    }
}
