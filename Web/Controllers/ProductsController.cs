using Application.Features.Products.CreateProduct;
using Application.Features.Products.GetProductById;
using Application.Features.Stock.ReserveStock;
using Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Requests;
using Web.Responses;

namespace Web.Controllers;

/// <summary>
/// Контроллер для управления продуктами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создание нового продукта.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        // 🔁 Маппинг HTTP DTO → Application Command
        var command = new CreateProductCommand(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        var productId = await mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = productId },
            new ApiResponse<Guid>(true, StatusCodes.Status201Created, productId, "Продукт успешно создан"));
    }

    /// <summary>
    /// Получение продукта по ID.
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
    /// Резервирование стока для заказа.
    /// </summary>
    [HttpPost("reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveStock(
        [FromBody] ReserveStockRequest request,
        CancellationToken ct)
    {
        var command = new ReserveStockCommand(
            request.ProductId,
            request.Quantity,
            request.CorrelationId);

        await mediator.Send(command, ct);

        return NoContent();
    }
}
