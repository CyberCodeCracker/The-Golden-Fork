using golden_fork.core.Entities.AppUser;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Repositories
{
    public class TokenRepository : GenericRepository<Token>, ITokenRepository
    {
        public TokenRepository(AppDbContext context) : base(context)
        {
        }
    }
}
