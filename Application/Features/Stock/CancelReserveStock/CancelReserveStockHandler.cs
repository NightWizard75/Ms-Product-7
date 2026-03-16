using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Stock.CancelReserveStock;

public class CancelReserveStockHandler(
    IProductRepository productRepository,
    ILogger<CancelReserveStockHandler> logger)
    : IRequestHandler<CancelReserveStockCommand>
{
    public async Task Handle(CancelReserveStockCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        
        if (product is null)
        {
            throw new EntityNotFoundException(
                "Product", 
                new Dictionary<string, object?> { ["id"] = request.ProductId });
        }

        // Доменная логика отмены резерва
        product.CancelReservation(request.Quantity);
        
        await productRepository.UpdateAsync(product, ct);
        
        logger.LogInformation("Резерв отменён: ProductId={ProductId}, Quantity={Quantity}, CorrelationId={CorrelationId}",
            request.ProductId, request.Quantity, request.CorrelationId);
    }
}
