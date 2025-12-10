// golden_fork.API/Controllers/PaymentController.cs
using golden_fork.core.DTOs.Purchase;
using golden_fork.Core.IServices.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace golden_fork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        // Customer creates cash payment when placing order
        [HttpPost("cash/{orderId}")]
        [Authorize]
        public async Task<ActionResult> CreateCashPayment(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            var result = await _paymentService.CreateCashPaymentAsync(orderId, userId, isAdmin);

            return result.success
                ? Ok(new { result.message, payment = result.payment })
                : BadRequest(new { result.message });
        }

        [HttpPost("{orderId}/confirm")]
        [Authorize(Roles = "Admin,Kitchen")]
        public async Task<ActionResult> ConfirmPayment(int orderId)
        {
            var result = await _paymentService.MarkAsPaidAsync(orderId);
            return result.success ? Ok(new { result.message }) : BadRequest(new { result.message });
        }

        // Payment gateway callback (no auth — public endpoint)
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<ActionResult> Callback([FromBody] PaymentCallbackDto dto)
        {
            var (success, message) = await _paymentService.ProcessPaymentCallbackAsync(dto);
            return Ok(new { message });
        }
    }
}