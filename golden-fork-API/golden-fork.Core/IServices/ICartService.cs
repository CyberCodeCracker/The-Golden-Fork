using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using golden_fork.core.DTOs.Cart;

namespace golden_fork.Core.IServices.Cart
{
    public interface ICartService
    {
        Task<CartResponse?> GetCartAsync(int userId);
        Task<(bool success, string message)> AddToCartAsync(int userId, AddToCartRequest request);
        Task<(bool success, string message)> UpdateQuantityAsync(int userId, int itemId, UpdateCartItemRequest request);
        Task<(bool success, string message)> RemoveFromCartAsync(int userId, int itemId);
        Task<(bool success, string message)> ClearCartAsync(int userId);
    }
}
