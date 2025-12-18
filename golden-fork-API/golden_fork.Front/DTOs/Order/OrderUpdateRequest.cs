using golden_fork.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Order
{
    public class OrderUpdateRequest
    {
        public OrderStatus Status { get; set; } 
    }
}
