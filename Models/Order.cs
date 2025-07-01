namespace AutoOtpad.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // Pending, Shipped, Delivered, Cancelled
        public ICollection<OrderItem> Items { get; set; }

    }
}
