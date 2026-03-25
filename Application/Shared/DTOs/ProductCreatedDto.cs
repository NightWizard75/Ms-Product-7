namespace Application.Shared.DTOs;

/// <summary>
/// Минимальный ответ на успешное создание продукта.
/// Содержит только сгенерированный идентификатор.
/// </summary>
public record ProductCreatedDto(
    Guid Id
    );
