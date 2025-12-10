using golden_fork.core.DTOs.Order;
using golden_fork.core.DTOs.Purchase;
using golden_fork.core.Entities.AppUser;
using golden_fork.Core;
using golden_fork.Core.IServices.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public OrderController(IOrderService orderService) => _orderService = orderService;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<OrderResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = "orderdate",
        [FromQuery] bool ascending = false)
    {
        var result = await _orderService.GetAllAsync(pageNumber, pageSize, searchTerm, status, sortBy, ascending);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        // Optional: only allow user to see their own order
        if (order.UserId != UserId && !User.IsInRole("Admin")) return Forbid();
        return Ok(order);
    }

    [HttpPost("create-from-cart")]
    [Authorize]
    public async Task<ActionResult> CreateFromCart()
    {
        var (success, message, orderId) = await _orderService.CreateFromCartAsync(UserId);
        return success ? Ok(new { message, orderId }) : BadRequest(new { message });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] OrderUpdateRequest request)
    {
        var (success, message) = await _orderService.UpdateStatusAsync(id, request);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult> Cancel(int id)
    {
        var (success, message) = await _orderService.CancelOrderAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}