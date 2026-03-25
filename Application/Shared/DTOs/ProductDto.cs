namespace Application.Shared.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    int PriceInKopecks,
    string PriceAsMoney, // ✅ Вычисляемое: форматированная строка для UI
    int StockQuantity,
    int ReservedQuantity,
    int AvailableQuantity
);
