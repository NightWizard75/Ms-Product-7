using Application.Shared.Events;
using Application.Shared.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.OnOrderCreated;

public class OnOrderCreatedHandler(
    IProductRepository productRepository,
    IRabbitMqPublisher rabbitMqPublisher,
    ILogger<OnOrderCreatedHandler> logger)
    : IRequestHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        
        if (product is null)
        {
            logger.LogWarning("Продукт не найден: ProductId={ProductId}", request.ProductId);
            return;
        }

        try
        {
            product.ReserveStock(request.Quantity);
            await productRepository.UpdateAsync(product, ct);
            
            logger.LogInformation("Сток зарезервирован: OrderId={OrderId}, ProductId={ProductId}", 
                request.OrderId, request.ProductId);

            var stockReserved = new StockReservedEvent(
                OrderId: request.OrderId,
                ProductId: request.ProductId,
                ReservedQuantity: request.Quantity,
                CorrelationId: request.CorrelationId
            );
            await rabbitMqPublisher.PublishStockReservedAsync(stockReserved, ct);
        }
        catch (ConflictException ex)
        {
            logger.LogWarning("Не удалось зарезервировать: {Message}", ex.Message);
            
            var failed = new StockReservationFailedEvent(
                OrderId: request.OrderId,
                ProductId: request.ProductId,
                RequestedQuantity: request.Quantity,
                Reason: ex.Message,
                CorrelationId: request.CorrelationId
            );
            await rabbitMqPublisher.PublishStockReservationFailedAsync(failed, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Неожиданная ошибка: OrderId={OrderId}", request.OrderId);
            
            var failed = new StockReservationFailedEvent(
                OrderId: request.OrderId,
                ProductId: request.ProductId,
                RequestedQuantity: request.Quantity,
                Reason: "Internal error",
                CorrelationId: request.CorrelationId
            );
            await rabbitMqPublisher.PublishStockReservationFailedAsync(failed, ct);
        }
    }
}
