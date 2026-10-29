using Atelier.Enums;
using Atelier.Modeles;

namespace Atelier.Requests
{
    public class CreateProductRequest
    {
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public int PriceCents { get; set; }

        public int Stock { get; set; }
        public ProductCategory Category { get; set; }
        public bool Active { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
