using Atelier.Enum;

namespace Atelier.Modeles
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public Status Status { get; set; }
        public List<Product> items { get; set; }
        public int TotalCents { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
