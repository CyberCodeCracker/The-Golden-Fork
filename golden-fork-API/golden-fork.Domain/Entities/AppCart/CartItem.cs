using golden_fork.core.Entities.AppUser;
using golden_fork.core.Entities.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.AppCart
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        [ForeignKey(nameof(CartId))]
        public Cart Cart { get; set; } = null!;
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; } = 1;
    }
}
