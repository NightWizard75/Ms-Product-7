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
        var productId = await mediator.Send(request.ToCommand(), ct);

        return CreatedAtAction(
            nameof(ShowProductController.GetById),
            "ShowProduct",
            new { id = productId },
            new ApiResponse<Guid>(true, StatusCodes.Status201Created, productId, "Продукт успешно создан"));
    }
}
