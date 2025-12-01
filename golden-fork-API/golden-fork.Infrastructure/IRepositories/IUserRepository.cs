using golden_fork.core.Entities.AppUser;
using golden_fork.Infrastructure.IRepositorie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> ExistsByEmailAsync(string email;
    }
}
