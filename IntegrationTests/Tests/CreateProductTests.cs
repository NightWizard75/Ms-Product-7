using System.Net;
using Application.Shared.DTOs;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Web.Requests;
using Web.Responses;

namespace IntegrationTests.Tests;

/// <summary>
/// Интеграционные тесты для создания продукта.
/// </summary>
public class CreateProductTests(ProductWebApplicationFactory factory) 
    : IClassFixture<ProductWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateProduct_ValidData_Returns201AndCreatedProduct()
    {
        // Arrange
        var request = new CreateProductRequest(
            Name: "Тестовый продукт",
            Description: "Описание для теста",
            PriceAsMoney: null,
            PriceInKopecks: 99999,
            StockQuantity: 50
        );

        // Act
        // 👇 Используем наше расширение + явный тип ответа
        var response = await _client.PostJsonAsync<CreateProductRequest>("/api/products", request);
        var responseBody = await response.ReadFromJsonAsync<ApiResponse<ProductCreatedDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);
        Assert.NotNull(responseBody.Data);
        Assert.NotEqual(Guid.Empty, responseBody.Data.Id);
        Assert.Equal("Продукт успешно создан", responseBody.Message);

        // 🔍 Проверяем, что продукт действительно сохранён в БД
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Database.Context.ProductDbContext>();
        var savedProduct = await context.Products.FindAsync(responseBody.Data.Id);
        
        Assert.NotNull(savedProduct);
        Assert.Equal("Тестовый продукт", savedProduct.Name);
        Assert.Equal(50, savedProduct.StockQuantity);
    }

    [Fact]
    public async Task CreateProduct_InvalidName_Returns400WithValidationErrors()
    {
        // Arrange
        var request = new CreateProductRequest(
            Name: "",  // ❌ Пустое имя — нарушение валидации
            Description: "Описание",
            PriceAsMoney: null,
            PriceInKopecks: 100,
            StockQuantity: 10
        );

        // Act
        var response = await _client.PostJsonAsync<CreateProductRequest>("/api/products", request);
        
        // 🔍 ДИАГНОСТИКА: читаем сырой контент
        var rawContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔍 Status: {response.StatusCode}");
        Console.WriteLine($"🔍 Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"🔍 Raw Body: {rawContent}");
        
        // 👇 Для ошибок НЕ используем ReadFromJsonAsync (он выбросит исключение на 400)
        // Используем ReadProblemDetailsAsync, который не вызывает EnsureSuccessStatusCode
        var problemDetails = await response.ReadProblemDetailsAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("Ошибка валидации данных", problemDetails.Title);
        
        Assert.NotNull(problemDetails.Errors);
        Assert.Contains("Name", problemDetails.Errors.Keys);
        Assert.Contains("обязательно", problemDetails.Errors["Name"].FirstOrDefault());
    }
}
