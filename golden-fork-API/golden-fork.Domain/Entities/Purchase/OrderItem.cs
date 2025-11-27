using golden_fork.core.Entities.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Purchase
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
