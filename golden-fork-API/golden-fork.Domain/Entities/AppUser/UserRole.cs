using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.AppUser
{
    public class UserRole : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<User> Users { get; set; } = new HashSet<User>();
    }
}
