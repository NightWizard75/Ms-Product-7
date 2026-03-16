using Application.Features.Products.GetProductById;
using Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Responses;

namespace Web.Controllers;

/// <summary>
/// Получение продукта по ID.
/// </summary>
[ApiController]
[Route("api/products")]
public class ShowProductController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/products/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var product = await mediator.Send(query, ct);

        return Ok(new ApiResponse<ProductDto>(true, StatusCodes.Status200OK, product));
    }

    /// <summary>
    /// Хелпер для генерации маршрута из других контроллеров.
    /// </summary>
    public static object GetRouteValues(Guid id) => new { id };
}
