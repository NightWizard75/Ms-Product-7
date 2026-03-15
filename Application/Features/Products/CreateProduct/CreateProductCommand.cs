using MediatR;

namespace Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
) : IRequest<Guid>;
