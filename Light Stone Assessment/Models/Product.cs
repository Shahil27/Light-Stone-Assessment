namespace Light_Stone_Assessment.Models
{
    public class Product
    {
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
