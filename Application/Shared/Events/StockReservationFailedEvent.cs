namespace Application.Shared.Events;

public record StockReservationFailedEvent(
    Guid OrderId,
    Guid ProductId,
    int RequestedQuantity,
    string Reason,
    string CorrelationId);
