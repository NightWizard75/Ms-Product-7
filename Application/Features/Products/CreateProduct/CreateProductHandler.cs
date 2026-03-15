using Application.Shared.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.CreateProduct;

public class CreateProductHandler(IProductRepository repository) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity
        );

        await repository.AddAsync(product, ct);

        return product.Id;
    }
}
