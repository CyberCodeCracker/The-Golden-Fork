using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.DTOs.Order
{
    public class OrderUpdateRequest
    {
        public string? Status { get; set; } // "En cours", "Préparée", "Livrée", "Annulée"
    }
}
