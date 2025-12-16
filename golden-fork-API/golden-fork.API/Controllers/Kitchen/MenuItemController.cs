using golden_fork.API;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.Core.IServices.Kitchen;
using Microsoft.AspNetCore.Mvc;

[Route("api/menu/{menuId}/items")]
[ApiController]
[AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
public class MenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddItem(int menuId, [FromBody] MenuItemRequest request)
    {
        var result = await _menuItemService.AddItemToMenuAsync(menuId, request);

        if (result.success)
        {
            return Ok(new
            {
                message = result.message,
                success = true,
                itemId = result.itemId  // ← Critical for frontend photo upload
            });
        }

        return BadRequest(new { message = result.message });
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> RemoveItem(int menuId, int itemId)
    {
        var result = await _menuItemService.RemoveItemFromMenuAsync(menuId, itemId);

        if (result.success)
        {
            return Ok(new { message = result.message });
        }

        return NotFound(new { message = result.message });
    }
}