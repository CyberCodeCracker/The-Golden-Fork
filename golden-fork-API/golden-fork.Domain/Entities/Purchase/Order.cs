using golden_fork.core.Entities.AppUser;
using golden_fork.core.Entities.Purchase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.Entities.Purchase
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public String Status { get; set; } = "En cours";
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
        public Payment? Payment { get; set; }

    }
}
