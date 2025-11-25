namespace BlazorBday.Models

{
    public abstract class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public int Points { get; set; }
        public int Inventory { get; set; }

        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public int CategoryId { get; set; }
        public string SubCategory { get; set; } = string.Empty;
    }
}
