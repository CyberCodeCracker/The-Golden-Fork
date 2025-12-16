using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs.Cart
{
    public class AddToCartRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
