using System.Globalization;
using Application.Features.Products.CreateProduct;

namespace Web.Requests;

/// <summary>
/// HTTP-запрос на создание продукта.
/// </summary>
public record CreateProductRequest(
    string Name,
    string Description,
    int? PriceInKopecks,      // ← Теперь необязательное (одно из двух)
    string? PriceAsMoney,     // ← Новое поле: "1999.99"
    int StockQuantity
)
{
    /// <summary>
    /// Конвертирует запрос в команду.
    /// Вызывается ПОСЛЕ валидации (гарантировано одно поле заполнено).
    /// </summary>
    public CreateProductCommand ToCommand()
    {
        var kopecks = PriceInKopecks ?? KopecksFromPriceAsMoney();
        
        return new CreateProductCommand(
            Name: Name,
            Description: Description,
            PriceInKopecks: kopecks,
            StockQuantity: StockQuantity
        );
    }

    /// <summary>
    /// Парсит PriceAsMoney в копейки.
    /// </summary>
    private int KopecksFromPriceAsMoney()
    {
        var normalized = PriceAsMoney!.Replace(',', '.');
        var amount = decimal.Parse(normalized, CultureInfo.InvariantCulture);
        
        return (int)Math.Round(amount * 100);
    }
}
