# Normalisation du modèle

Les configurations sont chargées après celles d'Identity par ApplyConfigurationsFromAssembly.
Les noms SQL des migrations existantes sont conservés ; cette extraction ne change pas le schéma.

## Tables métier

- products : clé Id ; SKU unique. Les caractéristiques et le prix courant dépendent du produit.
- customers : clé Id ; email unique. Les commandes référencent le client sans recopier ses coordonnées.
- orders : clé Id ; CustomerId référence customers. Le statut et les dates dépendent de la commande.
- order_items : clé (OrderId, ProductId). Quantité et prix de vente historique dépendent de la ligne entière. Une commande ne peut contenir qu'une ligne par produit.
- LineTotalCents n'est pas stocké. OrderTotal est une projection sur order_totals, pas une table de totaux dupliqués.

Ces dépendances sont compatibles avec la troisième forme normale pour les règles métier actuelles. Le prix historique est un fait différent du prix courant du produit.
Les enums représentent des valeurs simples ; une table de référence devient utile si une catégorie possède ses propres attributs.

## Limites de la vérification

Cette revue porte sur le modèle EF et les migrations, pas sur une inspection de la base SQL déployée.
ToView ne crée pas la vue : la migration CreateOrderTotalsView crée order_totals avec un total et un nombre de lignes nuls pour les commandes vides.
ApplicationUser.Role et les associations AspNetUserRoles ne doivent pas devenir deux sources concurrentes pour le même rôle. Le modèle conserve ici le comportement existant ; l'authentification devra choisir une seule source de vérité.
Les propriétés NormalizedEmail et NormalizedUserName sont des valeurs dérivées maintenues par Identity pour les recherches, une redondance technique du framework.