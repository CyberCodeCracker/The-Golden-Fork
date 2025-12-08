// golden_fork.Core/Services/Kitchen/ItemService.cs
using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;

namespace golden_fork.Core.Services.Kitchen
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedResult<ItemResponse>> GetAllItemsAsync(
            int pageNumber = 1,
            int pageSize = 12,
            string? searchTerm = null,
            string? sortBy = "name",
            bool ascending = true)
        {
            var query = _unitOfWork.ItemRepository.GetQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(searchTerm) ||
                    (i.Description != null && i.Description.ToLower().Contains(searchTerm)));
            }


            query = sortBy?.ToLower() switch
            {
                "price" => ascending ? query.OrderBy(i => i.Price) : query.OrderByDescending(i => i.Price),
                "createdat" => ascending ? query.OrderBy(i => i.CreatedAt) : query.OrderByDescending(i => i.CreatedAt),
                _ => ascending ? query.OrderBy(i => i.Name) : query.OrderByDescending(i => i.Name)
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Include(i => i.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = _mapper.Map<List<ItemResponse>>(items);

            return new PagedResult<ItemResponse>(response, pageNumber, pageSize, totalItems);
        }

        public async Task<ItemResponse?> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.ItemRepository.GetQueryable()
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            return item == null ? null : _mapper.Map<ItemResponse>(item);
        }

        public async Task<(bool success, string message, int? itemId)> CreateAsync(ItemRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return (false, "Item name is required", null);

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                return (false, "Category not found", null);

            // THIS IS THE FIX — EF Core can translate this!
            var nameNormalized = request.Name.Trim();
            var exists = await _unitOfWork.ItemRepository.GetQueryable()
                .AnyAsync(i => i.Name.Trim().ToLower() == nameNormalized.ToLower());

            if (exists)
                return (false, "An item with this name already exists", null);

            var itemEntity = _mapper.Map<Item>(request);
            itemEntity.IsAvailable = true;
            itemEntity.CreatedAt = DateTime.UtcNow;
            itemEntity.ImageUrl ??= "/images/default-item.jpg";

            await _unitOfWork.ItemRepository.AddAsync(itemEntity);
            await _unitOfWork.ItemImageRepository.SaveChangesAsync();

            return (true, "Item created successfully", itemEntity.Id);
        }
        public async Task<(bool success, string message)> UpdateAsync(int id, ItemUpdate request)
        {
            var existingItem = await _unitOfWork.ItemRepository.GetByIdAsync(id);
            if (existingItem == null)
                return (false, "Item not found");

            // Prevent duplicate name (only if name is being changed)
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim() != existingItem.Name.Trim())
            {
                var nameToCheck = request.Name.Trim().ToLower();
                var nameExists = await _unitOfWork.ItemRepository.GetQueryable()
                    .AnyAsync(i => i.Name.Trim().ToLower() == nameToCheck && i.Id != id);

                if (nameExists)
                    return (false, "An item with this name already exists");
            }

            // Map updates
            _mapper.Map(request, existingItem);
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ItemRepository.UpdateAsync(existingItem);
            await _unitOfWork.ItemRepository.SaveChangesAsync();

            return (true, "Item updated successfully");
        }
        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            try
            {
                await _unitOfWork.ItemRepository.DeleteByIdAsync(id);
                await _unitOfWork.ItemRepository.SaveChangesAsync();
                return (true, "Item deleted successfully");
            }
            catch
            {
                return (false, "Item not found or could not be deleted");
            }
        }

        public async Task<(bool success, string message, string? imageUrl)> UploadPhotoAsync(int itemId, IFormFile photo, bool isMain = false)
        {
            var item = await _unitOfWork.ItemRepository.GetQueryable()
                .Include(i => i.ItemImages)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return (false, "Item not found", null);
            if (photo == null || photo.Length == 0) return (false, "No photo uploaded", null);

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            var newUrl = $"/images/{fileName}";

            // If this is marked as main → demote all others
            if (isMain)
            {
                foreach (var img in item.ItemImages)
                    img.IsMain = false;
            }

            // Add new image
            item.ItemImages.Add(new ItemImage
            {
                Url = newUrl,
                IsMain = isMain
            });

            // SYNC MAIN IMAGE AUTOMATICALLY
            SyncMainImage(item);

            await _unitOfWork.ItemRepository.SaveChangesAsync();

            return (true, "Photo uploaded successfully", newUrl);
        }
        private void SyncMainImage(Item item)
        {
            // Find the current main image
            var mainImage = item.ItemImages.FirstOrDefault(i => i.IsMain);

            // If there's a main image → update ImageUrl
            if (mainImage != null)
            {
                item.ImageUrl = mainImage.Url;
            }
            // If no main image but there are images → use the first one
            else if (item.ItemImages.Any())
            {
                item.ImageUrl = item.ItemImages.First().Url;
            }
            // Fallback
            else
            {
                item.ImageUrl = "/images/default-item.jpg";
            }

            item.UpdatedAt = DateTime.UtcNow;
        }
    }

}