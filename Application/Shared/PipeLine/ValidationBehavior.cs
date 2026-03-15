using FluentValidation;
using MediatR;

namespace Application.Shared.Pipeline;

/// <summary>
/// Поведение пайплайна для автоматической валидации команд MediatR.
/// Перехватывает запрос до хендлера и запускает соответствующие валидаторы.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        // Если для этого типа запроса нет валидаторов — пропускаем
        if (!validators.Any()) 
            return await next();

        var context = new ValidationContext<TRequest>(request);
        
        // Запускаем все валидаторы параллельно
        var failures = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct))
        );

        // Собираем ошибки
        var errors = failures
            .Where(f => f.Errors.Count > 0)
            .SelectMany(f => f.Errors)
            .ToList();

        // Если есть ошибки — выбрасываем исключение (его поймает ExceptionHandler)
        if (errors.Count > 0)
            throw new ValidationException(errors);

        return await next();
    }
}
