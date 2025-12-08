using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace golden_fork.Core.Services.Kitchen
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<CategoryResponse>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 12,
            string? searchTerm = null,
            string? sortBy = "name",
            bool ascending = true)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            // Start with base query
            var query = _unitOfWork.CategoryRepository.GetQueryable();

            // 1. Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            // 2. Sort
            query = sortBy?.ToLower() switch
            {
                "itemcount" => ascending
                    ? query.OrderBy(c => c.Items.Count)
                    : query.OrderByDescending(c => c.Items.Count),
                "createdat" => ascending
                    ? query.OrderBy(c => c.CreatedAt)
                    : query.OrderByDescending(c => c.CreatedAt),
                _ => ascending
                    ? query.OrderBy(c => c.Name)
                    : query.OrderByDescending(c => c.Name)
            };

            // 3. Count total (before Include)
            var totalItems = await query.CountAsync();

            // 4. Include Items — MUST BE AFTER Where/OrderBy
            var categories = await query
                .Include(c => c.Items)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Map
            var response = _mapper.Map<List<CategoryResponse>>(categories);

            // 6. Add ItemCount manually
            for (int i = 0; i < categories.Count; i++)
            {
                response[i].ItemCount = categories[i].Items.Count;
            }

            return new PagedResult<CategoryResponse>(response, pageNumber, pageSize, totalItems);
        }
        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetQueryable()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return null;

            var result = _mapper.Map<CategoryResponse>(category);
            result.ItemCount = category.Items.Count;
            return result;
        }

        public async Task<(bool success, string message, int? categoryId)> CreateAsync(CategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return (false, "Category name is required", null);

            var nameToCheck = request.Name.Trim();
            var exists = await _unitOfWork.CategoryRepository.GetQueryable()
                .AnyAsync(c => c.Name.Trim().ToLower() == nameToCheck.ToLower());

            if (exists)
                return (false, "A category with this name already exists", null);

            var category = _mapper.Map<Category>(request);
            category.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Category created successfully", category.Id);
        }

        public async Task<(bool success, string message)> UpdateAsync(int id, CategoryUpdate request)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return (false, "Category not found");

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim() != category.Name.Trim())
            {
                var nameExists = await _unitOfWork.CategoryRepository.GetQueryable()
                    .AnyAsync(c => c.Name.Trim().ToLower() == request.Name.Trim().ToLower() && c.Id != id);

                if (nameExists)
                    return (false, "A category with this name already exists");
            }

            _mapper.Map(request, category);
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CategoryRepository.UpdateAsync(category);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Category updated successfully");
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return (false, "Category not found");

            // Optional: prevent delete if has items
            var hasItems = await _unitOfWork.ItemRepository.GetQueryable()
                .AnyAsync(i => i.CategoryId == id);

            if (hasItems)
                return (false, "Cannot delete category with items");

            await _unitOfWork.CategoryRepository.DeleteAsync(category);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Category deleted successfully");
        }
    }
}