using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests.Helpers;

/// <summary>
/// DTO для десериализации ошибок в тестах.
/// Точно соответствует формату, который возвращает ExceptionHandler.
/// </summary>
public class TestProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    
    // Кастомные поля из ExceptionHandler
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }
    
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }
    
    // Для любых других неизвестных полей (на всякий случай)
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extensions { get; set; }
}
