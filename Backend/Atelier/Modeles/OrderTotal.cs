namespace Atelier.Modeles
{
    /// <summary>
    /// Mappée sur la vue SQL `order_totals`. Lecture seule : EF Core exclut
    /// automatiquement les entités configurées avec ToView() des migrations.
    /// </summary>
    public class OrderTotal
    {
        public int OrderId { get; set; }
        public int TotalCents { get; set; }
        public int LineCount { get; set; }
    }
}
