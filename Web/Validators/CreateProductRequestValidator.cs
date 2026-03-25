using System.Globalization;
using FluentValidation;
using Web.Requests;

namespace Web.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название продукта обязательно")
            .MaximumLength(200).WithMessage("Название не должно превышать 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Количество на складе не может быть отрицательным");

        // 👇 XOR-логика: ровно одно поле заполнено (валидация ОБЪЕКТА)
        RuleFor(x => x)
            .Must(request => 
                (request.PriceInKopecks.HasValue && string.IsNullOrWhiteSpace(request.PriceAsMoney)) ||
                (!request.PriceInKopecks.HasValue && !string.IsNullOrWhiteSpace(request.PriceAsMoney)))
            .WithMessage("Требуется только одно из полей: PriceInKopecks или PriceAsMoney");

        // 👇 PriceInKopecks: только неотрицательные значения
        RuleFor(x => x.PriceInKopecks)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceInKopecks.HasValue)
            .WithMessage("Цена не может быть отрицательной");

        // 👇 PriceAsMoney: формат + неотрицательность
        RuleFor(x => x.PriceAsMoney)
            .Matches(@"^\d+([.,]\d{1,2})?$")
            .When(x => !string.IsNullOrWhiteSpace(x.PriceAsMoney))
            .WithMessage("Цена должна быть в формате '125.50', '125,50' или '125'")
            .Must(BeNonNegative)
            .When(x => !string.IsNullOrWhiteSpace(x.PriceAsMoney))
            .WithMessage("Цена не может быть отрицательной");
    }

    private bool BeNonNegative(string? price)
    {
        if (string.IsNullOrWhiteSpace(price)) return true;
    
        var normalized = price.Replace(',', '.');
    
        return decimal.TryParse(
            normalized, 
            NumberStyles.Number | NumberStyles.AllowDecimalPoint, 
            CultureInfo.InvariantCulture, 
            out var amount
        ) && amount >= 0;
    }
}
