// golden_fork.Core/Services/Purchase/PaymentService.cs
using AutoMapper;
using golden_fork.core.DTOs.Purchase;
using golden_fork.core.Entities.Purchase;
using golden_fork.core.Enums;
using golden_fork.Core.IServices.Purchase;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace golden_fork.Core.Services.Purchase
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentResponse?> GetByOrderIdAsync(int orderId)
        {
            var payment = await _unitOfWork.PaymentRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            return payment == null ? null : _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<(bool success, string message, PaymentResponse? payment)> CreateCashPaymentAsync(
            int orderId,
            int userId,
            bool isAdmin)
        {
            // 1. Get order
            var order = await _unitOfWork.OrderRepository.GetQueryable()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return (false, "Order not found", null);

            // 2. Authorization
            if (order.UserId != userId && !isAdmin)
                return (false, "You are not authorized to pay for this order", null);

            // 3. Order status check
            if (order.Status != OrderStatus.Pending)
                return (false, "Order cannot be paid in its current state", null);

            // 4. Prevent double payment
            var alreadyPaid = await _unitOfWork.PaymentRepository.GetQueryable()
                .AnyAsync(p => p.OrderId == orderId && p.IsPaid);

            if (alreadyPaid)
                return (false, "This order has already been paid", null);

            // 5. Create payment
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalPrice,
                Method = "Cash",
                Status = "Pending",
                IsPaid = false,
                PaidAt = DateTime.UtcNow
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.PaymentRepository.SaveChangesAsync();

            return (true, "Cash payment registered. Please pay on delivery.", _mapper.Map<PaymentResponse>(payment));
        }

        public async Task<(bool success, string message)> MarkAsPaidAsync(int orderId)
        {
            var payment = await _unitOfWork.PaymentRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                return (false, "Payment not found");

            if (payment.IsPaid)
                return (false, "Payment already confirmed");

            payment.IsPaid = true;
            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;

            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.PaymentRepository.SaveChangesAsync();

            // Update order status
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Paid;
                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.OrderRepository.SaveChangesAsync();
            }

            return (true, "Payment confirmed — order is now paid");
        }

        public async Task<(bool success, string message)> ProcessPaymentCallbackAsync(PaymentCallbackDto dto)
        {
            // 1. Validate signature — NEVER skip this in production
            if (!ValidateGatewaySignature(dto))
                return (false, "Invalid signature — possible attack");

            // 2. Find payment
            var payment = await _unitOfWork.PaymentRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.OrderId == dto.OrderId);

            if (payment == null)
                return (false, "Payment not found");

            // 3. Prevent replay attacks
            if (payment.IsPaid)
                return (true, "Payment already processed");

            // 4. Process result
            if (dto.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                payment.IsPaid = true;
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = dto.TransactionId;

                // Update order
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(dto.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Paid;
                    await _unitOfWork.OrderRepository.UpdateAsync(order);
                    await _unitOfWork.OrderRepository.SaveChangesAsync();
                }
            }
            else
            {
                payment.Status = "Failed";
            }

            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.PaymentRepository.SaveChangesAsync();

            return (true, $"Payment {payment.Status.ToLower()}");
        }

        // PASTE THIS METHOD IN THE SAME SERVICE CLASS
        private bool ValidateGatewaySignature(PaymentCallbackDto dto)
        {
            // Replace with your real gateway secret from appsettings.json or env var
            var secret = Environment.GetEnvironmentVariable("PAYMENT_GATEWAY_SECRET")
                         ?? "your-fallback-secret-for-dev-only";

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(dto.Signature))
                return false;

            var data = dto.TransactionId + dto.OrderId + dto.Status;
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(dataBytes);
            var computedSignature = Convert.ToBase64String(computedHash);

            // Fixed-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(dto.Signature));
        }
    }
}