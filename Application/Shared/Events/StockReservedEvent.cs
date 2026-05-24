namespace Application.Shared.Events;

public record StockReservedEvent(
    Guid OrderId,
    Guid ProductId,
    int ReservedQuantity,
    string CorrelationId);
