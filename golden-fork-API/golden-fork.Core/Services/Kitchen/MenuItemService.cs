using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace golden_fork.Core.Services.Kitchen
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MenuItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(bool success, string message, int itemId)> AddItemToMenuAsync(int menuId, MenuItemRequest request)
        {
            // Validate menu exists
            var menu = await _unitOfWork.MenuRepository.GetQueryable()
                .Include(m => m.MenuItems)
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (menu == null)
                return (false, "Menu not found", 0);

            // Map request to Item entity
            var itemEntity = _mapper.Map<Item>(request);

            // Set additional properties
            itemEntity.CreatedAt = DateTime.UtcNow;
            itemEntity.IsAvailable = request.IsAvailable;
            itemEntity.ImageUrl ??= "/images/default-item.jpg";

            // Save new item
            await _unitOfWork.ItemRepository.AddAsync(itemEntity);

            // Create link
            var menuItem = new MenuItem
            {
                MenuId = menuId,
                Item = itemEntity,
                Position = request.Position,
                IsAvailable = request.IsAvailable,
                IsSpecial = request.IsSpecial,
                SpecialPrice = request.IsSpecial ? request.SpecialPrice : null
            };

            menu.MenuItems.Add(menuItem);

            // Save everything
            await _unitOfWork.ItemRepository.SaveChangesAsync();

            // Return success with the new item's ID
            return (true, "Item created and added to menu successfully", itemEntity.Id);
        }

        public async Task<(bool success, string message)> RemoveItemFromMenuAsync(int menuId, int itemId)
        {
            // Load the menu with its MenuItems
            var menu = await _unitOfWork.MenuRepository.GetQueryable()
                .Include(m => m.MenuItems)
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (menu == null)
                return (false, "Menu not found");

            // Find the MenuItem to remove
            var menuItem = menu.MenuItems.FirstOrDefault(mi => mi.ItemId == itemId);

            if (menuItem == null)
                return (false, "Item not found in this menu");

            menu.MenuItems.Remove(menuItem);

            await _unitOfWork.ItemRepository.SaveChangesAsync();

            return (true, "Item removed from menu successfully");
        }
    }
}