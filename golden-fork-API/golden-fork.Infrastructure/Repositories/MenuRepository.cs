using golden_fork.core.Entities.Kitchen;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class MenuRepository : GenericRepository<Menu>, IMenuRepository
    {
        public MenuRepository(AppDbContext context) : base(context)
        {
        }
    }
}
