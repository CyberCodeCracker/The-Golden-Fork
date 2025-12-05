using golden_fork.core.Entities.AppUser;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {

        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            return user != null;
        }
    }
}
