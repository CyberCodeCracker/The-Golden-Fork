using golden_fork.core.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.IServices.Purchase
{
    public interface IPaymentService
    {
        Task<PaymentResponse?> GetByOrderIdAsync(int orderId);
        Task<(bool success, string message, PaymentResponse? payment)> CreateCashPaymentAsync(int orderId, int userId, bool isAdmin);
        Task<(bool success, string message)> MarkAsPaidAsync(int orderId); // Kitchen/Admin
        Task<(bool success, string message)> ProcessPaymentCallbackAsync(PaymentCallbackDto dto);
    }
}
