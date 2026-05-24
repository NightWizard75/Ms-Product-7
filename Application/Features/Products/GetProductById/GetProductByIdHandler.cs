using Application.Shared.DTOs;
using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.Products.GetProductById;

public class GetProductByIdHandler(
    IProductRepository repository,
    IMemoryCache cache)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private const string CacheKeyPrefix = "product:";
    private const int CacheTtl = 5;
    
    // 👇 Настройки кэша: 5 минут абсолютного истечения
    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTtl)
    };

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var cacheKey = $"{CacheKeyPrefix}{request.Id}";

        // 🔹 1. Пробуем получить из кэша
        if (cache.TryGetValue(cacheKey, out ProductDto? cached) && cached != null)
        {
            return cached; 
        }

        // 🔹 2. Кэш промах — идём в репозиторий
        var product = await repository.GetByIdAsync(request.Id, ct);

        if (product is null)
            throw new EntityNotFoundException(
                "Product", 
                new Dictionary<string, object?> { ["id"] = request.Id });

        // 🔹 3. Конвертируем в DTO и сохраняем в кэш
        var dto = product.ToDto();
        cache.Set(cacheKey, dto, CacheOptions);

        return dto;
    }
}
