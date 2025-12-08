using golden_fork.core.Entities.Menu;
using golden_fork.Infrastructure.IRepositorie;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.IRepositories
{
    public interface IItemRepository : IGenericRepository<Item>
    {
    }
}
