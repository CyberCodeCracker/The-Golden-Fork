using golden_fork.core.DTOs.Kitchen;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.IServices.Kitchen
{
    public interface IItemService
    {
        Task<PagedResult<ItemResponse>> GetAllItemsAsync(
            int pageNumber,
            int pageSize,
            String? SearchTerm,
            string? sortBy,
            bool ascending); 
        Task<ItemResponse?> GetByIdAsync(int id);
        Task<(bool success, string message, int? itemId)> CreateAsync(ItemRequest dto);
        Task<(bool success, string message)> UpdateAsync(int id, ItemUpdate dto);
        Task<(bool success, string message)> DeleteAsync(int id);
        Task<(bool success, string message, string? imageUrl)> UploadPhotoAsync(int itemId, IFormFile photo, bool isMain);
    }
}
