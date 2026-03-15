using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IntegrationTests.Helpers;

/// <summary>
/// Extension-методы для упрощения HTTP-запросов в тестах.
/// </summary>
public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostJsonAsync<TRequest>(
        this HttpClient client, 
        string requestUri, 
        TRequest content, 
        CancellationToken ct = default)
    {
        return await System.Net.Http.Json.HttpClientJsonExtensions
        .PostAsJsonAsync<TRequest>(client, requestUri, content, ct);
    }

    public static async Task<TResponse?> ReadFromJsonAsync<TResponse>(
        this HttpResponseMessage response, 
        CancellationToken ct = default) where TResponse : class
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    public static async Task<TestProblemDetails?> ReadProblemDetailsAsync(
        this HttpResponseMessage response, 
        CancellationToken ct = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // 👇 Важно для корректной обработки кириллицы
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        return await response.Content.ReadFromJsonAsync<TestProblemDetails>(options, ct);
    }
}
