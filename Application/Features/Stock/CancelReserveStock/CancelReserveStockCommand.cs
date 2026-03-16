using MediatR;

namespace Application.Features.Stock.CancelReserveStock;

/// <summary>
/// Команда для отмены резервирования стока (компенсирующее действие Saga).
/// </summary>
public record CancelReserveStockCommand(
    Guid ProductId,
    int Quantity,
    string CorrelationId
) : IRequest;
