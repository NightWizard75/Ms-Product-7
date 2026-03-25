using Application.Shared.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Requests;
using Web.Responses;

namespace Web.Controllers;

/// <summary>
/// Создание нового продукта.
/// </summary>
[ApiController]
[Route("api/products")]
public class CreateProductController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        var validator = HttpContext.RequestServices
            .GetRequiredService<IValidator<CreateProductRequest>>();
    
        var validationResult = await validator.ValidateAsync(request, ct);
    
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        var productCreatedDto = await mediator.Send(request.ToCommand(), ct);

        return CreatedAtAction(
            nameof(ShowProductController.GetById),
            "ShowProduct",
            new { id = productCreatedDto.Id },
            new ApiResponse<ProductCreatedDto>(true, StatusCodes.Status201Created, productCreatedDto, "Продукт успешно создан"));
    }
}
