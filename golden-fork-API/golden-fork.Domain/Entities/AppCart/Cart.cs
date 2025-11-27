using golden_fork.core.Entities.AppUser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.AppCart
{
    public class Cart : BaseEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>(); 
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
