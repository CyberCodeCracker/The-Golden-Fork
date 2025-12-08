// golden_fork.Core/Services/Cart/CartService.cs
using AutoMapper;
using golden_fork.core.DTOs.Cart;
using golden_fork.core.Entities.AppCart;
using golden_fork.core.Entities.AppUser;
using golden_fork.Core.IServices.Cart;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace golden_fork.Core.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartResponse?> GetCartAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Item)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return null;

            var response = _mapper.Map<CartResponse>(cart);

            // Manual fields AutoMapper can't do
            response.TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Item.Price);
            response.TotalItems = cart.CartItems.Sum(ci => ci.Quantity);
            response.Items = _mapper.Map<List<CartItemResponse>>(cart.CartItems);

            return response;
        }

        public async Task<(bool success, string message)> AddToCartAsync(int userId, AddToCartRequest request)
        {
            // THIS IS THE FIX — use GetQueryable() + FirstOrDefaultAsync
            var item = await _unitOfWork.ItemRepository.GetQueryable()
                .FirstOrDefaultAsync(i => i.Id == request.ItemId);

            if (item == null)
                return (false, "Item not found");

            if (!item.IsAvailable)
                return (false, "Item is not available");

            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // Create cart if doesn't exist
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.CartRepository.AddAsync(cart);
                await _unitOfWork.CartRepository.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ItemId == request.ItemId);

            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ItemId = request.ItemId,
                    Quantity = request.Quantity
                });
            }

            await _unitOfWork.CartItemRepository.SaveChangesAsync();
            return (true, "Added to cart");
        }

        public async Task<(bool success, string message)> UpdateQuantityAsync(int userId, int itemId, UpdateCartItemRequest request)
        {
            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return (false, "Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ItemId == itemId);
            if (cartItem == null) return (false, "Item not in cart");

            if (request.Quantity <= 0)
            {
                cart.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = request.Quantity;
            }

            await _unitOfWork.CartItemRepository.SaveChangesAsync(); // ← Your way
            return (true, "Cart updated");
        }

        public async Task<(bool success, string message)> RemoveFromCartAsync(int userId, int itemId)
        {
            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return (false, "Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ItemId == itemId);
            if (cartItem == null) return (false, "Item not in cart");

            cart.CartItems.Remove(cartItem);
            await _unitOfWork.CartItemRepository.SaveChangesAsync(); // ← Your way

            return (true, "Item removed from cart");
        }

        public async Task<(bool success, string message)> ClearCartAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return (true, "Cart already empty");

            cart.CartItems.Clear();
            await _unitOfWork.CartRepository.SaveChangesAsync(); // ← Your way

            return (true, "Cart cleared");
        }
    }
}