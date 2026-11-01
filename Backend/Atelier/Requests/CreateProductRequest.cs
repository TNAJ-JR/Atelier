using System.ComponentModel.DataAnnotations;
using Atelier.Enums;

namespace Atelier.Requests;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "Le SKU est obligatoire.")]
    [StringLength(450, ErrorMessage = "Le SKU ne peut pas dépasser 450 caractères.")]
    public required string Sku { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le nom ne peut pas dépasser 200 caractères.")]
    public required string Name { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Le prix doit être un montant positif ou nul en cents.")]
    public required int PriceCents { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Le stock doit être positif ou nul.")]
    public required int Stock { get; set; }

    [EnumDataType(typeof(ProductCategory), ErrorMessage = "La catégorie est invalide.")]
    public required ProductCategory Category { get; set; }

    public bool Active { get; set; } = true;
}
