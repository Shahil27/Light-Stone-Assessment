using System.ComponentModel.DataAnnotations.Schema;

namespace Light_Stone_Assessment.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ExternalOrderId { get; set; } = null!;
        public DateTime PlacedAt { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
