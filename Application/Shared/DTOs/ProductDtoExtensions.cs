using Application.Shared.DTOs;

namespace Application.Shared.DTOs;

/// <summary>
/// Extension-методы для конвертации сущностей Product в DTO.
/// </summary>
public static class ProductDtoExtensions
{
    /// <summary>
    /// Конвертирует сущность продукта в DTO.
    /// </summary>
    public static ProductDto ToDto(this Domain.Entities.Product product) => new ProductDto(
        Id: product.Id,
        Name: product.Name,
        Description: product.Description,
        PriceInKopecks: product.PriceInKopecks,
        PriceAsMoney: product.GetPriceAsMoney(),
        StockQuantity: product.StockQuantity,
        ReservedQuantity: product.ReservedQuantity,
        AvailableQuantity: product.AvailableQuantity
    );
    
    public static ProductCreatedDto ToCreatedDto(this Domain.Entities.Product product) => new ProductCreatedDto(
        Id: product.Id
    );
}
