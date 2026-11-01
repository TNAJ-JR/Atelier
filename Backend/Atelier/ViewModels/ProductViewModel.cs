namespace Atelier.ViewModels;

public sealed record ProductViewModel(
    int Id,
    string Sku,
    string Name,
    int PriceCents,
    int Stock,
    string Category,
    bool Active,
    DateTimeOffset CreatedAt);
