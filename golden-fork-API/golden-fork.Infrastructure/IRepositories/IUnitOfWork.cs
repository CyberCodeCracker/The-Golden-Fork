using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.IRepositories
{
    public interface IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; }
        public ICartRepository CartRepository { get; }
        public ICartItemRepository CartItemRepository { get; }
        public IItemImageRepository ItemImageRepository { get; }
        public IItemRepository ItemRepository { get; }
        public IMenuRepository MenuRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IOrderItemRepository OrderItemRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public ITokenRepository TokenRepository { get; }
        public IUserRepository UserRepository { get; }
        public IUserRoleRepository UserRoleRepository { get; }


    }
}
