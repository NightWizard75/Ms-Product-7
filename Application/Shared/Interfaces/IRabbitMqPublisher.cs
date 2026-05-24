using Application.Shared.Events;

namespace Application.Shared.Interfaces;

// Один интерфейс — один файл ✅
public interface IRabbitMqPublisher
{
    Task PublishStockReservedAsync(StockReservedEvent @event, CancellationToken ct = default);
    Task PublishStockReservationFailedAsync(StockReservationFailedEvent @event, CancellationToken ct = default);
}
