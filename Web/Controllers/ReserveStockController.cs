using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Requests;

namespace Web.Controllers;

/// <summary>
/// Резервирование стока для заказа (шаг Saga).
/// </summary>
[ApiController]
[Route("api/products")]
public class ReserveStockController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/products/reserve
    /// </summary>
    [HttpPost("reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reserve(
        [FromBody] ReserveStockRequest request,
        CancellationToken ct)
    {
        // 👇 Используем ToCommand() для маппинга
        await mediator.Send(request.ToCommand(), ct);
        
        return NoContent();
    }
}
