namespace Atelier.Modeles
{
    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
