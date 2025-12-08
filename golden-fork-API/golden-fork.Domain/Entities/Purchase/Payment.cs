using golden_fork.core.Entities.Purchase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Purchase
{
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;   
        public Decimal Amount { get; set; }
        // Cast Method to Empty to avoid being null
        public String Method { get; set; } = String.Empty;
        public String Status { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; } 

    }
}
