using golden_fork.core.Entities.AppCart;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositorie;
using golden_fork.Infrastructure.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context)
        { 
        }
    }
}
