using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.Services.Kitchen
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private readonly string _webRootPath;
        private readonly string _apiBaseUrl; // Added field for API 

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _env = env;
            _webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot"); // ← Use ContentRootPath
            _apiBaseUrl = "http://localhost:5128"; // Your API port
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
        public async Task<CategoryWithItemsResponse?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetQueryable()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return null;

            var result = _mapper.Map<CategoryWithItemsResponse>(category);
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

        public async Task<(bool success, string message, string? imageUrl)> UploadPhotoAsync(int categoryId, IFormFile photo)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return (false, "Category not found", null);

            if (photo == null || photo.Length == 0)
                return (false, "No photo uploaded", null);

            var uploadsFolder = Path.Combine(_webRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var newUrl = $"http://localhost:5128/images/{fileName}";  // ← FULL API URL
            category.ImageUrl = newUrl;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CategoryRepository.UpdateAsync(category);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Photo uploaded successfully", newUrl);
        }

        public async Task<(bool success, string message)> AddItemToCategoryAsync(int categoryId, int itemId)
        {
            // Check if category exists
            var categoryExists = await _unitOfWork.CategoryRepository.GetQueryable()
                .AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
                return (false, "Category not found");

            // Check if item exists
            var itemExists = await _unitOfWork.ItemRepository.GetQueryable()
                .AnyAsync(i => i.Id == itemId);
            if (!itemExists)
                return (false, "Item not found");

            // Check if already linked
            var alreadyLinked = await _unitOfWork.ItemRepository.GetQueryable()
                .AnyAsync(i => i.Id == itemId && i.CategoryId == categoryId);
            if (alreadyLinked)
                return (false, "Item is already in this category");

            // Update the item's CategoryId
            var item = await _unitOfWork.ItemRepository.GetByIdAsync(itemId);
            if (item == null)
                return (false, "Item not found");

            item.CategoryId = categoryId;
            item.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ItemRepository.UpdateAsync(item);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Item added to category successfully");
        }

        public async Task<(bool success, string message)> RemoveItemFromCategoryAsync(int categoryId, int itemId)
        {
            var item = await _unitOfWork.ItemRepository.GetQueryable()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.CategoryId == categoryId);

            if (item == null)
                return (false, "Item not found in this category");

            // Remove from category by setting CategoryId to null or a default
            item.CategoryId = 0; // or null if your entity allows nullable CategoryId
            item.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ItemRepository.UpdateAsync(item);
            await _unitOfWork.CategoryRepository.SaveChangesAsync();

            return (true, "Item removed from category successfully");
        }
    }
}