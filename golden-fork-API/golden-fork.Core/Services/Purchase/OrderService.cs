using AutoMapper;
using global::golden_fork.core.DTOs.Order;
using global::golden_fork.core.DTOs.Purchase;
using global::golden_fork.core.Entities.Purchase;
using global::golden_fork.Infrastructure.IRepositories;
using golden_fork.core.DTOs.Cart;
using golden_fork.core.Enums;
using golden_fork.Core.IServices.Purchase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Status filter - now using enum
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Try to parse the status string as enum
                if (Enum.TryParse<OrderStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(o => o.Status == statusEnum);
                }
                // If not a valid enum, try to match by string name
                else
                {
                    query = query.Where(o => o.Status.ToString().ToLower() == status.ToLower());
                }
            }

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

        public async Task<(bool success, string message, int? orderId)> CreateFromCartAsync(int userId, CreateOrderFromCartRequest request)
        {
            // Validate request
            if (request == null || !request.Items.Any())
                return (false, "Cart is empty", null);

            // Get items from database to validate
            var itemIds = request.Items.Select(ci => ci.ItemId).ToList();
            var dbItems = await _unitOfWork.ItemRepository.GetQueryable()
                .Where(i => itemIds.Contains(i.Id))
                .ToListAsync();

            if (!dbItems.Any())
                return (false, "No valid items found", null);

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = request.TotalAmount,
                Status = OrderStatus.Pending,
                OrderItems = request.Items.Select(ci => new OrderItem
                {
                    ItemId = ci.ItemId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price
                }).ToList()
            };

            await _unitOfWork.OrderRepository.AddAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            // Clear user's cart from database if it exists
            var userCart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart != null)
            {
                foreach (var cartItem in userCart.CartItems.ToList())
                {
                    await _unitOfWork.CartItemRepository.DeleteAsync(cartItem);
                }
                await _unitOfWork.CartItemRepository.SaveChangesAsync();
            }

            return (true, "Order created successfully", order.Id);
        }
        public async Task<(bool success, string message)> UpdateStatusAsync(int orderId, OrderUpdateRequest request)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return (false, "Order not found");

            if (!string.IsNullOrWhiteSpace(request.Status.ToString()))
            {
                // Convert string to enum
                if (Enum.TryParse<OrderStatus>(request.Status.ToString(), true, out var statusEnum))
                {
                    order.Status = statusEnum;
                    order.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    return (false, $"Invalid status: {request.Status}");
                }
            }

            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            return (true, $"Status updated to: {request.Status}");
        }

        public async Task<(bool success, string message)> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return (false, "Order not found");

            // Check if order can be cancelled (only Pending or Confirmed orders)
            // NOTE: You need to convert enum to string for comparison if needed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                return (false, "Only pending or confirmed orders can be cancelled");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.OrderRepository.SaveChangesAsync();

            return (true, "Order cancelled successfully");
        }

        // Optional: Helper method to check if order can be cancelled
        public bool CanCancelOrder(OrderStatus status)
        {
            return status == OrderStatus.Pending || status == OrderStatus.Confirmed;
        }
    }
}