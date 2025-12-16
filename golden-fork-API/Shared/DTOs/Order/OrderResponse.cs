using golden_fork.core.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs.Purchase
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "En cours";
        public int ItemCount { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public PaymentResponse? Payment { get; set; }
    }

}
