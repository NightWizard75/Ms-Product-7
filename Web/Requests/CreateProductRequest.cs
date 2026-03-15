namespace Web.Requests;

/// <summary>
/// HTTP-запрос на создание продукта.
/// </summary>
public record CreateProductRequest(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);
