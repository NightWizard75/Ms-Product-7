using Application.Features.Products.CreateProduct;

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
)
{
public CreateProductCommand ToCommand() => new CreateProductCommand(
    Id,
    Name,
    Description,
    Price,
    StockQuantity);
}
