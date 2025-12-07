using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.Kitchen;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.Services.Kitchen
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MenuService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Menu> CreateMenuAsync(MenuRequest request)
        {
            if (request is null || request.Name is null || request.Description is null)
            {
                throw new ArgumentNullException("Le menu ne peut pas etre vide");
            }

            var existingMenu = _unitOfWork.MenuRepository.GetAllAsync().Result
                .FirstOrDefault(m => m.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
            if (existingMenu != null)
            {
                throw new InvalidOperationException("Un menu avec le meme nom existe deja.");
            }
            var menuEntity = _mapper.Map<Menu>(request);
            await _unitOfWork.MenuRepository.AddAsync(menuEntity);
            await _unitOfWork.MenuRepository.SaveChangesAsync();
            return menuEntity;
        }

        public async Task<bool> DeleteMenuByIdAsync(int id)
        {
            try
            {
                await _unitOfWork.MenuRepository.DeleteByIdAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur: Menu '{id}' non trouvé");
            }
        }

        public async Task<PagedResult<MenuCardResponse>> GetAllMenusAsync(
            int pageNumber = 1,
            int pageSize = 12,
            string? searchTerm = null,
            string? sortBy = "name",
            bool ascending = true)
        {
            var query = _unitOfWork.MenuRepository.GetQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m =>
                    m.Name.Contains(searchTerm) ||
                    (m.Description != null && m.Description.Contains(searchTerm)));
            }

            // Sort
            query = sortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name),
                "createdat" => ascending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt),
                "itemcount" => ascending ? query.OrderBy(m => m.MenuItems.Count) : query.OrderByDescending(m => m.MenuItems.Count),
                _ => query.OrderBy(m => m.Name)
            };

            var totalItems = await query.CountAsync();

            var menus = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper here!
            var items = _mapper.Map<List<MenuCardResponse>>(menus);

            return new PagedResult<MenuCardResponse>(items, pageNumber, pageSize, totalItems);
        }

        public async Task<MenuWithItemsResponse?> GetMenuByIdAsync(int id)
        {
            var menu = await _unitOfWork.MenuRepository
                .GetQueryable()
                .Include(m => m.MenuItems)
                    .ThenInclude(mi => mi.Item)
                        .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) return null;

            // Use AutoMapper here!
            return _mapper.Map<MenuWithItemsResponse>(menu);
        }

        public async Task<MenuCardResponse> UpdateMenuAsync(int id, MenuRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "Le menu ne peut pas etre vide");
            }

            Menu existingMenu = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                existingMenu = await _unitOfWork.MenuRepository.GetByIdAsync(id);
                if (existingMenu == null)
                {
                    throw new KeyNotFoundException($"Menu with ID {id} not found");
                }

                _mapper.Map(request, existingMenu);
            });

            var menuCard = _mapper.Map<MenuCardResponse>(existingMenu);
            return menuCard;
        }
    }
}
