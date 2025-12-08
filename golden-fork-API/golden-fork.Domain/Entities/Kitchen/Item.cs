using golden_fork.core.Entities.Kitchen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Menu
{
    public class Item : BaseEntity
    {
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; }
        public decimal Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public String ImageUrl { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public ICollection<MenuItem> MenuItems = new HashSet<MenuItem>();
        public ICollection<ItemImage> ItemImages { get; set; } = new HashSet<ItemImage>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
