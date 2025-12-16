namespace golden_fork.Front.DTOs.Purchase
{
    public class PaymentAdminResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
