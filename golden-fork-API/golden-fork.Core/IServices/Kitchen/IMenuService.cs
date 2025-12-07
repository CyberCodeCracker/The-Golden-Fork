using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.IServices.Kitchen
{
    public interface IMenuService
    {
        Task<Menu> CreateMenuAsync(MenuRequest request);
        Task<bool> DeleteMenuByIdAsync(int id);
        Task<PagedResult<MenuCardResponse>> GetAllMenusAsync(
            int pageNumber,
            int pageSize,
            String? SearchTerm,
            string? sortBy,
            bool ascending); 
        Task<MenuWithItemsResponse> GetMenuByIdAsync(int id);
        Task<MenuCardResponse> UpdateMenuAsync(int id, MenuRequest request);
    }
}
