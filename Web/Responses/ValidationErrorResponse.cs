namespace Web.Responses;

/// <summary>
/// Формат ответа при ошибке валидации (400 Bad Request).
/// </summary>
public record ValidationErrorResponse(
    string Message,
    Dictionary<string, string[]> Errors,
    int StatusCode = 400,
    string? CorrelationId = null
);
