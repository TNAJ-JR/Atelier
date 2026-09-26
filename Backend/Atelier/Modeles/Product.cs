namespace Atelier.Modeles
{
    public class Product
    {
        public int Id { get; set; }
        public int SKU { get; set; }
        public string Name { get; set; }
        public int PriceCents { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public Boolean Active { get; set; }

    }
}
