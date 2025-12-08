using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.Core;

namespace golden_fork.Core.IServices.Kitchen
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryResponse>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 12,
            string? searchTerm = null,
            string? sortBy = "name",
            bool ascending = true);

        Task<CategoryResponse?> GetByIdAsync(int id);

        Task<(bool success, string message, int? categoryId)> CreateAsync(CategoryRequest request);

        Task<(bool success, string message)> UpdateAsync(int id, CategoryUpdate request);

        Task<(bool success, string message)> DeleteAsync(int id);
    }
}