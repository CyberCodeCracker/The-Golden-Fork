using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Menu
{
    public class Category : BaseEntity
    {
        public String Name { get; set; }
        public String? Description { get; set; }
        public ICollection<Item> Items { get; set; } = new HashSet<Item>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
}
}
