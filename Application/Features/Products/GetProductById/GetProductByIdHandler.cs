using Application.Shared.DTOs;
using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Products.GetProductById;

public class GetProductByIdHandler(IProductRepository repository) 
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct);

        if (product is null)
            throw new EntityNotFoundException(
                "Product", 
                new Dictionary<string, object?> { ["id"] = request.Id });

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.ReservedQuantity,
            product.AvailableQuantity
        );
    }
}
