using Domain.Exceptions;

namespace Domain.Entities;

public class Product(Guid id, string name, string description, decimal price, int stockQuantity)
{
    private Product() : this(Guid.Empty, string.Empty, string.Empty, 0, 0) { }

    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public decimal Price { get; private set; } = price;
    public int StockQuantity { get; private set; } = stockQuantity;
    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public static Product Create(
        Guid id,
        string name,
        string description,
        decimal price,
        int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название продукта не может быть пустым", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Цена не может быть отрицательной", nameof(price));
        
        if (stockQuantity < 0)
            throw new ArgumentException("Количество на складе не может быть отрицательным", nameof(stockQuantity));

        return new Product(id, name.Trim(), description?.Trim() ?? string.Empty, price, stockQuantity);
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new ConflictException(
                "Недостаточно товара на складе", 
                "INSUFFICIENT_STOCK",
                new Dictionary<string, object?> 
                { 
                    ["requested"] = quantity, 
                    ["available"] = AvailableQuantity 
                });

        ReservedQuantity += quantity;
    }

    public void CancelReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new ConflictException(
                "Невозможно отменить резервирование — количество превышает зарезервированное", 
                "INVALID_RESERVATION_CANCEL",
                new Dictionary<string, object?> 
                { 
                    ["requested"] = quantity, 
                    ["reserved"] = ReservedQuantity 
                });

        ReservedQuantity -= quantity;
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new ConflictException(
                "Невозможно подтвердить резервирование — количество превышает зарезервированное", 
                "INVALID_RESERVATION_CONFIRM",
                new Dictionary<string, object?> 
                { 
                    ["requested"] = quantity, 
                    ["reserved"] = ReservedQuantity 
                });

        ReservedQuantity -= quantity;
        StockQuantity -= quantity;
    }

    public void AdjustStock(int delta)
    {
        var newStock = StockQuantity + delta;
        
        if (newStock < 0)
            throw new ConflictException(
                "Невозможно изменить складской остаток до отрицательного значения", 
                "INVALID_STOCK_ADJUSTMENT",
                new Dictionary<string, object?> 
                { 
                    ["current"] = StockQuantity, 
                    ["delta"] = delta 
                });

        if (newStock < ReservedQuantity)
            throw new ConflictException(
                "Невозможно уменьшить складской остаток ниже зарезервированного количества", 
                "STOCK_BELOW_RESERVED",
                new Dictionary<string, object?> 
                { 
                    ["newStock"] = newStock, 
                    ["reserved"] = ReservedQuantity 
                });

        StockQuantity = newStock;
    }
}
