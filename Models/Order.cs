namespace BlazorBday.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        public List<Product> Products { get; set; } = new();

        public int getTotalPoint()
        {
            return Products.Sum(p => p.Points);
        }
    }
}