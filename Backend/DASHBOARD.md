# API du dashboard

Trois requêtes GET indépendantes, protégées par l'authentification JWT existante :

| Endpoint | Réponse |
| --- | --- |
| `/api/dashboard/revenue` | `year`, `month`, `timeZone`, `revenueCents`, `orderCount` |
| `/api/dashboard/low-stock` | `threshold` et `products` (`id`, `name`, `sku`, `stock`, `active`) |
| `/api/dashboard/recent-orders` | Au plus cinq commandes : `id`, `customerId`, `customerName`, `createdAt`, `status`, `totalCents`, `lineCount` |

Le chiffre d'affaires porte sur les commandes **créées pendant le mois courant** et dont le statut actuel est `confirmed` ou `shipped`. Le modèle ne possède pas de date de confirmation ou d'expédition. Les limites du mois sont calculées dans le fuseau `America/Toronto`, puis converties en UTC, avec une fin exclusive. Le fuseau peut être changé avec `Dashboard:TimeZone` (ou `Dashboard__TimeZone`).

Tous les montants sont des entiers en cents CAD. Ils utilisent `OrderItem.UnitPriceCents`, figé à la création, et jamais le prix actuel du produit. Les agrégats utilisent des entiers 64 bits.

Le seuil de stock est strictement inférieur à 5. Les produits inactifs sont inclus, conformément à la maquette, triés par stock, nom, puis identifiant. Les cinq dernières commandes incluent tous les statuts (`draft`, `confirmed`, `shipped`, `cancelled`), triés par date de création décroissante puis identifiant décroissant.

Une base vide produit un chiffre d'affaires et un compteur à zéro et des listes vides. Une panne de base reste une erreur serveur, traitée par le middleware ProblemDetails existant ; elle n'est pas convertie en résultat vide. Aucun endpoint ne modifie les données. Aucune migration et aucune donnée de démonstration ne sont ajoutées.

Le frontend n'est pas modifié par cette fonctionnalité. Il pourra demander ces trois ressources en parallèle.

## Vérification

`dotnet test Atelier.Dashboard.Tests/Atelier.Dashboard.Tests.csproj`

Les tests isolent le dashboard en compilant ses sources et les modèles/configurations EF réels. Ils utilisent SQLite en mémoire pour vérifier les filtres, agrégats et limites temporelles sans toucher à la base configurée. Ils ne remplacent pas une vérification sur SQL Server avec l'authentification réelle.
