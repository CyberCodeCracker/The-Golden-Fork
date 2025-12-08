// golden_fork.API/Controllers/CartController.cs
using golden_fork.core.DTOs.Cart;
using golden_fork.Core.IServices.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace golden_fork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public CartController(ICartService cartService) => _cartService = cartService;

        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetCart()
        {
            var cart = await _cartService.GetCartAsync(UserId);
            return cart != null ? Ok(cart) : Ok(new CartResponse { UserId = UserId, UserName = User.Identity!.Name! });
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var (success, message) = await _cartService.AddToCartAsync(UserId, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPut("item/{itemId}")]
        public async Task<ActionResult> UpdateQuantity(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var (success, message) = await _cartService.UpdateQuantityAsync(UserId, itemId, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpDelete("item/{itemId}")]
        public async Task<ActionResult> RemoveItem(int itemId)
        {
            var (success, message) = await _cartService.RemoveFromCartAsync(UserId, itemId);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            var (success, message) = await _cartService.ClearCartAsync(UserId);
            return Ok(new { message });
        }
    }
}