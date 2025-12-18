using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // Order placed, waiting for confirmation
        Confirmed = 1,    // Order confirmed, waiting for payment
        Preparing = 2,    // Payment confirmed, kitchen is preparing
        Ready = 3,        // Order is ready for pickup/delivery
        Delivered = 4,    // Order has been delivered
        Cancelled = 5,    // Order was cancelled
        Paid = 6          // Payment has been made
    }

    public static class OrderStatusExtensions
    {
        public static string ToFriendlyString(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.Confirmed => "Confirmed",
                OrderStatus.Preparing => "Preparing",
                OrderStatus.Ready => "Ready",
                OrderStatus.Delivered => "Delivered",
                OrderStatus.Cancelled => "Cancelled",
                OrderStatus.Paid => "Paid",
                _ => "Unknown"
            };
        }
    }
}