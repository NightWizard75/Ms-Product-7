using FluentValidation;

namespace Application.Features.Stock.ReserveStock;

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID продукта обязателен");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Количество должно быть положительным");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("Correlation ID обязателен");
    }
}
