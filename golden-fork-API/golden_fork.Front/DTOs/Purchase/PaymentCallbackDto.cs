using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Front.DTOs.Purchase
{
    public class PaymentCallbackDto
    {
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Failed"; // "Success" or "Failed"
        public string Signature { get; set; } = string.Empty; // ← from gateway
    }
}
