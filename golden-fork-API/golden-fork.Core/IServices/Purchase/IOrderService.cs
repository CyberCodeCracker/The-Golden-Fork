using golden_fork.core.DTOs.Cart;
using golden_fork.core.DTOs.Order;
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

        // NEW METHODS FOR ADMIN PANEL
        Task<int> GetPendingOrderCountAsync();
        Task<PagedResult<AdminOrderResponse>> GetAllForAdminAsync(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<(bool success, string message)> UpdateOrderStatusAdminAsync(int orderId, string newStatus);
    }

    // NEW DTOs for admin panel
    public class AdminOrderResponse
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AdminOrderItemResponse> OrderItems { get; set; } = new();
    }

    public class AdminOrderItemResponse
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}