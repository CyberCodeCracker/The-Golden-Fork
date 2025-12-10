using golden_fork.Core.IServices.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using global::golden_fork.core.DTOs.Order;
using global::golden_fork.core.DTOs.Purchase;
using global::golden_fork.core.Entities.Purchase;
using global::golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace golden_fork.Core.Services.Purchase
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            string? sortBy = "orderdate",
            bool ascending = false)
        {
            var query = _unitOfWork.OrderRepository.GetQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                if (int.TryParse(searchTerm, out int id))
                    query = query.Where(o => o.Id == id);
                else
                    query = query.Where(o =>
                        o.User.Username.Contains(searchTerm) ||
                        o.User.Email.Contains(searchTerm));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            // Sort
            query = sortBy?.ToLower() switch
            {
                "total" => ascending ? query.OrderBy(o => o.TotalPrice) : query.OrderByDescending(o => o.TotalPrice),
                "status" => ascending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
                _ => ascending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate)
            };

            var total = await query.CountAsync();

            // Include MUST be last
            var orders = await query
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.Payment)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 100% AUTOMAPPER — NO MANUAL MAPPING
            var response = _mapper.Map<List<OrderResponse>>(orders);

            return new PagedResult<OrderResponse>(response, pageNumber, pageSize, total);
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.OrderRepository.GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            var result = _mapper.Map<OrderResponse>(order);
            result.Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                ItemId = oi.ItemId,
                ItemName = oi.Item.Name,
                ImageUrl = oi.Item.ImageUrl,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity
            }).ToList();

            return result;
        }

        public async Task<(bool success, string message, int? orderId)> CreateFromCartAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Item)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return (false, "Cart is empty", null);

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Item.Price),
                Status = "En cours",
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ItemId = ci.ItemId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Item.Price
                }).ToList()
            };

            await _unitOfWork.OrderRepository.AddAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            // Clear cart
            foreach (var ci in cart.CartItems.ToList())
            {
                await _unitOfWork.CartItemRepository.DeleteAsync(ci);
            }
            await _unitOfWork.CartItemRepository.SaveChangesAsync();

            return (true, "Order created successfully", order.Id);
        }

        public async Task<(bool success, string message)> UpdateStatusAsync(int orderId, OrderUpdateRequest request)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return (false, "Order not found");

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                order.Status = request.Status;
                order.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            return (true, $"Status updated to: {request.Status}");
        }

        public async Task<(bool success, string message)> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return (false, "Order not found");
            if (order.Status.ToLower() != "en cours") return (false, "Only pending orders can be canceled");

            order.Status = "Annulée";
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            return (true, "Order canceled");
        }
    }
}
