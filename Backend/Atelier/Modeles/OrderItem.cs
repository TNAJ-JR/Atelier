namespace Atelier.Modeles
{
    public class OrderItem
    {
        // Clé primaire composite (OrderId, ProductId) — configurée dans le DbContext.
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        /// <summary>
        /// Prix AU MOMENT DE LA COMMANDE, figé à la création. Ce n'est pas une
        /// duplication fautive de Product.PriceCents : c'est un fait distinct,
        /// qui dépend de la clé complète et non du seul ProductId. Un changement
        /// de tarif ne doit jamais réécrire l'historique de facturation.
        /// </summary>
        public int UnitPriceCents { get; set; }

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;

        public int LineTotalCents => Quantity * UnitPriceCents;

    }
}
