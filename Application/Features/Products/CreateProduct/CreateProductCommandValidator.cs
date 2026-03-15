using FluentValidation;

namespace Application.Features.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID продукта обязателен");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название продукта обязательно")
            .MaximumLength(200).WithMessage("Название продукта не должно превышать 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Цена не может быть отрицательной");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Количество на складе не может быть отрицательным");
    }
}
