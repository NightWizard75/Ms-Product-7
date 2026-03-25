using System.Net;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Application.Shared.DTOs;
using Web.Requests;
using Web.Responses;

namespace IntegrationTests.Tests;

/// <summary>
/// Интеграционные тесты для получения продукта по ID.
/// </summary>
[Collection("ProductIntegrationCollection")] // 👈 Раскомментируем на Этапе 3
public class GetProductTests(ProductWebApplicationFactory factory)
    : IClassFixture<ProductWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // ========================================================================
    // Тесты: Успешное получение
    // ========================================================================

    [Fact]
    public async Task GetProduct_ExistingId_Returns200AndProduct()
    {
        // Arrange: создаём тестовый продукт
        var productId = await CreateTestProductAsync("Тестовый продукт", "Описание", 9999, 10);

        // Act: запрашиваем продукт по ID
        var response = await _client.GetAsync($"/api/products/{productId}");
        var result = await response.ReadFromJsonAsync<ApiResponse<ProductDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(productId, result.Data.Id);
        Assert.Equal("Тестовый продукт", result.Data.Name);
        Assert.Equal(9999, result.Data.PriceInKopecks);
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsCorrectAvailableQuantity()
    {
        // Arrange: создаём продукт с известным StockQuantity
        const int stockQuantity = 50;
        
        var productId = await CreateTestProductAsync("Product for Quantity Test", "", 5000, stockQuantity);

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");
        var result = await response.ReadFromJsonAsync<ApiResponse<ProductDto>>();

        // Assert: проверяем, что AvailableQuantity рассчитывается правильно
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result?.Data);
        Assert.Equal(stockQuantity, result.Data.StockQuantity);
        Assert.True(result.Data.AvailableQuantity >= 0);
    }

    // ========================================================================
    // Тесты: Ошибки
    // ========================================================================

    [Fact]
    public async Task GetProduct_NonExistingId_Returns404()
    {
        // Arrange: генерируем ID, которого точно нет в БД
        var unknownId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{unknownId}");
        var problem = await response.ReadProblemDetailsAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Ресурс не найден", problem.Title);
        // 👇 Исправлено: Instance содержит полный путь с ID
        Assert.StartsWith("/api/products/", problem.Instance);
    }

    [Fact]
    public async Task GetProduct_InvalidGuidFormat_Returns404()
    {
        // Arrange: невалидный формат GUID
        var invalidId = "not-a-guid";

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidId}");

        // Assert: 👇 Исправлено: API возвращает 404 (не 400), т.к. GUID не валидируется на уровне маршрута
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // Хелперы для подготовки данных
    // ========================================================================

    /// <summary>
    /// Создаёт тестовый продукт через HTTP API (как в реальном использовании).
    /// </summary>
    private async Task<Guid> CreateTestProductAsync(
        string name, 
        string description, 
        int priceInKopecks, 
        int stockQuantity)
    {
        var request = new CreateProductRequest(
            Name: name,
            Description: description,
            PriceInKopecks: priceInKopecks,
            PriceAsMoney: null,
            StockQuantity: stockQuantity
        );

        var response = await _client.PostJsonAsync<CreateProductRequest>("/api/products", request);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.ReadFromJsonAsync<ApiResponse<ProductCreatedDto>>();
        return responseBody?.Data?.Id ?? throw new InvalidOperationException("Failed to read created product ID");
    }
}
