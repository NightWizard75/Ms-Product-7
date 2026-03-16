using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Requests;

namespace Web.Controllers;

/// <summary>
/// Отмена резервирования стока (компенсирующее действие Saga).
/// </summary>
[ApiController]
[Route("api/products")]
public class CancelReserveStockController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/products/cancel-reserve
    /// </summary>
    [HttpPost("cancel-reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelReserve(
        [FromBody] CancelReserveStockRequest request,
        CancellationToken ct)
    {
        // 👇 Используем ToCommand() для маппинга
        await mediator.Send(request.ToCommand(), ct);
        
        return NoContent();
    }
}
