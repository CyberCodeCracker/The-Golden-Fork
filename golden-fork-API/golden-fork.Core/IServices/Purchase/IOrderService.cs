using golden_fork.core.DTOs.Cart;
using golden_fork.core.DTOs.Order;
// golden_fork.Core.IServices.Purchase/IOrderService.cs
using golden_fork.core.DTOs.Purchase;
using golden_fork.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.IServices.Purchase
{
    public interface IOrderService
    {
        Task<PagedResult<OrderResponse>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            string? sortBy = "orderdate",
            bool ascending = false);

        Task<OrderResponse?> GetByIdAsync(int id);

        Task<(bool success, string message, int? orderId)> CreateFromCartAsync(int userId, CreateOrderFromCartRequest request);
        Task<(bool success, string message)> UpdateStatusAsync(int orderId, OrderUpdateRequest request);

        Task<(bool success, string message)> CancelOrderAsync(int orderId);
    }
}
