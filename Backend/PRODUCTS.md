# Création de produit

`POST /api/products`, avec un jeton JWT portant le rôle `Admin` ou `Staff`.

```json
{
  "sku": "BOI-001",
  "name": "Planche à découper",
  "priceCents": 2500,
  "stock": 3,
  "category": 0,
  "active": true
}
```

Les champs `sku`, `name`, `priceCents`, `stock` et `category` sont obligatoires. Le prix est exprimé en cents entiers et doit être positif ou nul, comme le stock. Les catégories correspondent à l’énumération existante : `0` = Bois, `1` = Metal, `2` = Textile. `active` vaut `true` par défaut.

Les espaces au début et à la fin du nom et du SKU sont supprimés. Le SKU est stocké en majuscules, avec une recherche de doublon insensible à la casse. Les longueurs maximales sont 450 caractères pour le SKU et 200 pour le nom. L’identifiant et la date UTC de création sont générés côté serveur ; les lignes de commande ne font pas partie de la demande.

## Réponses

- `201 Created` : le produit enregistré, avec `id`, `sku`, `name`, `priceCents`, `stock`, `category` (texte en minuscules), `active`, `createdAt`.
- `400 Bad Request` : demande manquante ou invalide.
- `401 Unauthorized` : utilisateur non authentifié.
- `403 Forbidden` : rôle autre que Admin ou Staff.
- `422 Unprocessable Entity` : SKU déjà utilisé, avec un message dans `errors.sku`, conformément à la maquette.

L’index unique existant protège également les créations concurrentes. Les violations SQL Server 2601/2627 sont traduites en erreur de SKU ; les autres erreurs de base remontent au middleware d’erreurs existant. Aucune migration ni donnée de démonstration n’est nécessaire.

Le frontend n’est pas encore branché sur cette création. L’exemple ci-dessus décrit le contrat et n’insère aucune donnée.

## Tests

`dotnet test Atelier.Products.Tests/Atelier.Products.Tests.csproj`

Les tests HTTP utilisent le vrai contrôleur et le vrai service dans un serveur ASP.NET Core en mémoire, une base SQLite temporaire et un schéma d’authentification de test. Ils vérifient la création, la persistance, les droits, les champs invalides ou omis, les doublons et la gestion serveur des champs sensibles. Ils ne constituent pas un test de l’authentification JWT réelle ni de la concurrence sur SQL Server.
