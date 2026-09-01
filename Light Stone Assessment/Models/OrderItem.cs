namespace Light_Stone_Assessment.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Sku { get; set; } = null!;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
