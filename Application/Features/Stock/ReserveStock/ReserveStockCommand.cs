using MediatR;

namespace Application.Features.Stock.ReserveStock;

public record ReserveStockCommand(
    Guid ProductId,
    int Quantity,
    string CorrelationId
) : IRequest<Unit>;
