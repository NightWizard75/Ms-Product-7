using System.Net;
using Application.Shared.DTOs;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Web.Requests;
using Web.Responses;

namespace IntegrationTests.Tests;

/// <summary>
/// Интеграционные тесты для резервирования и отмены резерва стока.
/// </summary>
[Collection("ProductIntegrationCollection")]
public class StockReservationTests(ProductWebApplicationFactory factory)
    : IClassFixture<ProductWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // ========================================================================
    // Тесты: Успешное резервирование
    // ========================================================================

    [Fact]
    public async Task ReserveStock_EnoughStock_Returns204AndDecrementsAvailable()
    {
        // Arrange: создаём продукт с известным стоком
        var productId = Guid.NewGuid();
        const int initialStock = 100;
        const int reserveQuantity = 30;
        
        await CreateTestProductAsync(productId, "Reserve Test Product", "", 50m, initialStock);

        var reserveRequest = new ReserveStockRequest(
            ProductId: productId,
            Quantity: reserveQuantity,
            CorrelationId: Guid.NewGuid().ToString()
        );

        // Act: резервируем сток
        var response = await _client.PostJsonAsync("/api/products/reserve", reserveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Проверяем, что сток уменьшился
        var getProductResponse = await _client.GetAsync($"/api/products/{productId}");
        var product = await getProductResponse.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        
        Assert.NotNull(product?.Data);
        Assert.Equal(initialStock, product.Data.StockQuantity);
        Assert.Equal(reserveQuantity, product.Data.ReservedQuantity);
        Assert.Equal(initialStock - reserveQuantity, product.Data.AvailableQuantity);
    }

    // ========================================================================
    // Тесты: Ошибки резервирования
    // ========================================================================

    [Fact]
    public async Task ReserveStock_InsufficientStock_Returns409Conflict()
    {
        // Arrange: продукт с малым стоком
        var productId = Guid.NewGuid();
        const int initialStock = 10;
        const int reserveQuantity = 50; // 👈 Больше, чем есть
        
        await CreateTestProductAsync(productId, "Low Stock Product", "", 50m, initialStock);

        var reserveRequest = new ReserveStockRequest(
            ProductId: productId,
            Quantity: reserveQuantity,
            CorrelationId: Guid.NewGuid().ToString()
        );

        // Act
        var response = await _client.PostJsonAsync("/api/products/reserve", reserveRequest);
        var problem = await response.ReadProblemDetailsAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Конфликт данных", problem.Title);
    }

    [Fact]
    public async Task ReserveStock_NonExistingProduct_Returns404()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        var reserveRequest = new ReserveStockRequest(
            ProductId: unknownId,
            Quantity: 10,
            CorrelationId: Guid.NewGuid().ToString()
        );

        // Act
        var response = await _client.PostJsonAsync("/api/products/reserve", reserveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // Тесты: Отмена резервирования (компенсирующее действие)
    // ========================================================================

    [Fact]
    public async Task CancelReserveStock_ValidRequest_Returns204AndRestoresStock()
    {
        // Arrange: создаём продукт и резервируем сток
        var productId = Guid.NewGuid();
        const int initialStock = 100;
        const int reserveQuantity = 30;
        var correlationId = Guid.NewGuid().ToString();
        
        await CreateTestProductAsync(productId, "Cancel Reserve Test", "", 50m, initialStock);
        
        // Сначала резервируем
        var reserveRequest = new ReserveStockRequest(productId, reserveQuantity, correlationId);
        await _client.PostJsonAsync("/api/products/reserve", reserveRequest);

        // Act: отменяем резерв
        var cancelRequest = new CancelReserveStockRequest(productId, reserveQuantity, correlationId);
        var response = await _client.PostJsonAsync("/api/products/cancel-reserve", cancelRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Проверяем, что сток восстановился
        var getProductResponse = await _client.GetAsync($"/api/products/{productId}");
        var product = await getProductResponse.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        
        Assert.NotNull(product?.Data);
        Assert.Equal(initialStock, product.Data.StockQuantity);
        Assert.Equal(0, product.Data.ReservedQuantity); // Резерв снят
        Assert.Equal(initialStock, product.Data.AvailableQuantity); // Весь сток снова доступен
    }

    [Fact]
    public async Task CancelReserveStock_MoreThanReserved_Returns409Conflict()
    {
        // Arrange: резервируем 10, пытаемся отменить 50
        var productId = Guid.NewGuid();
        await CreateTestProductAsync(productId, "Cancel Test", "", 50m, 100);
        
        // Резервируем 10
        var reserveRequest = new ReserveStockRequest(productId, 10, Guid.NewGuid().ToString());
        await _client.PostJsonAsync("/api/products/reserve", reserveRequest);

        // Act: пытаемся отменить больше, чем зарезервировано
        var cancelRequest = new CancelReserveStockRequest(productId, 50, Guid.NewGuid().ToString());
        var response = await _client.PostJsonAsync("/api/products/cancel-reserve", cancelRequest);
        var problem = await response.ReadProblemDetailsAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Конфликт данных", problem.Title);
    }

    [Fact]
    public async Task CancelReserveStock_NonExistingProduct_Returns404()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        var cancelRequest = new CancelReserveStockRequest(
            ProductId: unknownId,
            Quantity: 10,
            CorrelationId: Guid.NewGuid().ToString()
        );

        // Act
        var response = await _client.PostJsonAsync("/api/products/cancel-reserve", cancelRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // Хелперы
    // ========================================================================

    /// <summary>
    /// Создаёт тестовый продукт через HTTP API (как в реальном использовании).
    /// </summary>
    private async Task CreateTestProductAsync(
        Guid id, 
        string name, 
        string description, 
        decimal price, 
        int stockQuantity)
    {
        var request = new CreateProductRequest(
            Id: id,
            Name: name,
            Description: description,
            Price: price,
            StockQuantity: stockQuantity
        );

        var response = await _client.PostJsonAsync<CreateProductRequest>("/api/products", request);
        response.EnsureSuccessStatusCode();
    }
}
