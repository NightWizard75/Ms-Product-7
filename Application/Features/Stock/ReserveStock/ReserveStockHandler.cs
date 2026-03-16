using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Stock.ReserveStock;

public class ReserveStockHandler(
    IProductRepository repository,
    ILogger<ReserveStockHandler> logger)
    : IRequestHandler<ReserveStockCommand>
{
    public async Task Handle(ReserveStockCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.ProductId, ct);

        if (product is null)
        {
            logger.LogWarning("Продукт не найден для резервирования: ProductId={ProductId}, CorrelationId={CorrelationId}",
                request.ProductId, request.CorrelationId);
            
            throw new EntityNotFoundException(
                "Product", 
                new Dictionary<string, object?> { ["id"] = request.ProductId });
        }

        product.ReserveStock(request.Quantity);

        await repository.UpdateAsync(product, ct);

        logger.LogInformation("Сток зарезервирован: ProductId={ProductId}, Quantity={Quantity}, CorrelationId={CorrelationId}",
            request.ProductId, request.Quantity, request.CorrelationId);
    }
}
