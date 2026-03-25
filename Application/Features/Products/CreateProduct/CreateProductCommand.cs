using Application.Shared.DTOs;
using MediatR;

namespace Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    int PriceInKopecks,
    int StockQuantity
) : IRequest<ProductCreatedDto>;
