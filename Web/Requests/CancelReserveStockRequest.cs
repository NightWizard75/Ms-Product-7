using Application.Features.Stock.CancelReserveStock;

namespace Web.Requests;

public record CancelReserveStockRequest(
    Guid ProductId,
    int Quantity,
    string CorrelationId
)
{
    /// <summary>
    /// Маппинг HTTP DTO → Application Command.
    /// </summary>
    public CancelReserveStockCommand ToCommand() => new CancelReserveStockCommand(
        ProductId,
        Quantity,
        CorrelationId
    );
}
