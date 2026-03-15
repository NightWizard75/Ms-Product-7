using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Stock.ReserveStock;

public class ReserveStockHandler(IProductRepository repository) 
    : IRequestHandler<ReserveStockCommand, Unit>
{
    public async Task<Unit> Handle(ReserveStockCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.ProductId, ct);

        if (product is null)
            throw new EntityNotFoundException(
                "Product", 
                new Dictionary<string, object?> { ["id"] = request.ProductId });

        product.ReserveStock(request.Quantity);

        await repository.UpdateAsync(product, ct);

        return Unit.Value;
    }
}
