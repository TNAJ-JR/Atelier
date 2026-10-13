using Atelier.Enums;

namespace Atelier.Modeles
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Projection en lecture seule de la vue SQL `order_totals`.
        /// Aucune colonne `total_cents` n'existe sur la table : le total est
        /// dérivé, le stocker créerait une redondance qui peut diverger.
        /// </summary>
        public OrderTotal? Total { get; set; }

        /// <summary>
        /// Calcul en mémoire, utilisable quand les lignes sont déjà chargées
        /// (Include(o => o.Items)). Sur une requête, préfère la vue ou une
        /// projection LINQ : ce getter force le chargement de la collection.
        /// </summary>
        public int ComputeTotalCents() => Items.Sum(i => i.Quantity * i.UnitPriceCents);
    }
}
