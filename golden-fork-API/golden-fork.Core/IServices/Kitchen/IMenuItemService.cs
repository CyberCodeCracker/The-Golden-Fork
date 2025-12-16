// golden_fork.Core/IServices/Kitchen/IMenuItemService.cs
using golden_fork.core.DTOs.Kitchen;

namespace golden_fork.Core.IServices.Kitchen
{
    public interface IMenuItemService
    {
        Task<(bool success, string message, int itemId)> AddItemToMenuAsync(int menuId, MenuItemRequest request);
        Task<(bool success, string message)> RemoveItemFromMenuAsync(int menuId, int itemId);
    }
}