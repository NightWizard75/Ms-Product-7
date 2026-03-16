using FluentValidation;

namespace Application.Features.Stock.CancelReserveStock;

public class CancelReserveStockCommandValidator : AbstractValidator<CancelReserveStockCommand>
{
    public CancelReserveStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID продукта обязателен");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Количество должно быть положительным");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("CorrelationId обязателен")
            .MustAsync(BeValidGuidFormat).WithMessage("CorrelationId должен быть валидным GUID");
    }

    private Task<bool> BeValidGuidFormat(string correlationId, CancellationToken ct) =>
        Task.FromResult(Guid.TryParse(correlationId, out _));
}
