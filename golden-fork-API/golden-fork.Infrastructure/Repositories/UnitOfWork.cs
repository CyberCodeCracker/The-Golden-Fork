using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        private IDbContextTransaction? _transaction;

        private ICategoryRepository _categoryRepository;
        private ICartRepository _cartRepository;
        private ICartItemRepository _cartItemRepository;
        private IItemImageRepository _itemImageRepository;
        private IItemRepository _itemRepository;
        private IMenuRepository _menuRepository;
        private IOrderRepository _orderRepository;
        private IOrderItemRepository _orderItemRepository;
        private IPaymentRepository _paymentRepository;
        private ITokenRepository _tokenRepository;
        private IUserRepository _userRepository;
        private IUserRoleRepository _userRoleRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }   

        public ICategoryRepository CategoryRepository => _categoryRepository ?? = new CategoryRepository(_context);

        public ICartRepository CartRepository => _cartRepository ??= new CartRepository(_context);

        public ICartItemRepository CartItemRepository => _cartItemRepository ??= new CartItemRepository(_context);  

        public IItemImageRepository ItemImageRepository => _itemImageRepository ??= new ItemImageRepository(_context);

        public IItemRepository ItemRepository => _itemRepository ??= new ItemRepository(_context);

        public IMenuRepository MenuRepository => _menuRepository ??= new MenuRepository(_context);  

        public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_context);

        public IOrderItemRepository OrderItemRepository => _orderItemRepository ??= new OrderItemRepository(_context);

        public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(_context);

        public ITokenRepository TokenRepository => _tokenRepository ??= new TokenRepository(_context);

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);

        public IUserRoleRepository UserRoleRepository => _userRoleRepository ??= new UserRoleRepository(_context);  
    }
}
