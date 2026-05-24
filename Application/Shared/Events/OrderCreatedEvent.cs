using MediatR;

namespace Application.Shared.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    string CorrelationId) : IRequest;
