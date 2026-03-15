namespace Web.Requests;

/// <summary>
/// HTTP-запрос на резервирование стока.
/// </summary>
public record ReserveStockRequest(
    Guid ProductId,
    int Quantity,
    string CorrelationId
);
