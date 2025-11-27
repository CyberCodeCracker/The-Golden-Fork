using golden_fork.core.Entities.Purchase;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
