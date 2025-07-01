namespace AutoOtpad.Models
{
    public class ReturnRequest
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Denied
    }

}
