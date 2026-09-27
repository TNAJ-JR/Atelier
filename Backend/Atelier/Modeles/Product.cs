using Atelier.Enums;

namespace Atelier.Modeles
{
    public class Product
    {
        public int Id { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }

        /// <summary>Prix en centimes. Jamais de decimal/double pour de l'argent ici :
        /// l'entier évite toute question d'arrondi et de sérialisation JSON.</summary>
        public int PriceCents { get; set; }

        public int Stock { get; set; }
        public ProductCategory Category { get; set; }
        public bool Active { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
