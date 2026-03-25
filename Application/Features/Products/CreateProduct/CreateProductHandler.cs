using Application.Shared.DTOs;
using Application.Shared.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.CreateProduct;

public class CreateProductHandler(IProductRepository repository) 
    : IRequestHandler<CreateProductCommand, ProductCreatedDto>
{
    public async Task<ProductCreatedDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.PriceInKopecks,
            request.StockQuantity
        );

        await repository.AddAsync(product, ct);

        return product.ToCreatedDto();
    }
}
