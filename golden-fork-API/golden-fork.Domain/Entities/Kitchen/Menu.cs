using golden_fork.core.Entities.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Kitchen
{
    public class Menu : BaseEntity
    {
        public String Name { get; set; }
        public String? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<MenuItem> MenuItems { get; set; } = new HashSet<MenuItem>();
    }
}
