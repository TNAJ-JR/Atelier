-- ============================================================================
-- Atelier — schéma relationnel normalisé (BCNF)
-- Cible : SQLite 3.35+ (compatible better-sqlite3, Prisma, EF Core)
--
-- Décisions de conception :
--   1. Aucun attribut dérivé stocké. Le total d'une commande est calculé par
--      la vue `order_totals`. Voir en fin de fichier l'alternative
--      dénormalisée si le profilage montre que la vue est trop lente.
--   2. `order_items.unit_price_cents` duplique volontairement le prix produit :
--      c'est le prix AU MOMENT DE LA COMMANDE, un fait distinct du prix
--      courant. La dépendance porte sur la clé complète (order_id, product_id),
--      donc pas de violation de 2NF.
--   3. Les montants sont en entiers de centimes. Jamais de REAL pour de
--      l'argent : 0.1 + 0.2 != 0.3 en virgule flottante.
--   4. Les dates sont en TEXT ISO 8601 UTC. SQLite n'a pas de type date natif ;
--      ce format se trie lexicographiquement dans le bon ordre.
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------------------
-- users
-- Clés candidates : id, email. Aucun attribut non-clé n'en détermine un autre.
-- ---------------------------------------------------------------------------
CREATE TABLE users (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  email         TEXT    NOT NULL UNIQUE COLLATE NOCASE,
  password_hash TEXT    NOT NULL,
  role          TEXT    NOT NULL CHECK (role IN ('admin', 'staff')),
  created_at    TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

-- ---------------------------------------------------------------------------
-- products
-- `category` reste une colonne texte contrainte : tant qu'une catégorie n'a
-- aucun attribut propre, une table dédiée n'apporte rien. Dès qu'elle gagne
-- un libellé traduit, un ordre d'affichage ou un taux de taxe, extraire vers
-- une table `categories` et remplacer le CHECK par une clé étrangère.
-- ---------------------------------------------------------------------------
CREATE TABLE products (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  sku         TEXT    NOT NULL UNIQUE COLLATE NOCASE,
  name        TEXT    NOT NULL CHECK (length(trim(name)) > 0),
  price_cents INTEGER NOT NULL CHECK (price_cents > 0),
  stock       INTEGER NOT NULL DEFAULT 0 CHECK (stock >= 0),
  category    TEXT    NOT NULL CHECK (category IN ('bois', 'metal', 'textile')),
  active      INTEGER NOT NULL DEFAULT 1 CHECK (active IN (0, 1)),
  created_at  TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

-- Sert le filtre par catégorie de GET /products
CREATE INDEX idx_products_category ON products (category);
-- Sert le tri par prix
CREATE INDEX idx_products_price ON products (price_cents);
-- Sert la recherche par nom (préfixe uniquement ; un LIKE '%terme%' ne peut
-- pas utiliser d'index B-tree — acceptable sur 12 produits, à remplacer par
-- FTS5 au-delà de quelques milliers de lignes)
CREATE INDEX idx_products_name ON products (name COLLATE NOCASE);

-- ---------------------------------------------------------------------------
-- customers
-- ---------------------------------------------------------------------------
CREATE TABLE customers (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  name       TEXT    NOT NULL CHECK (length(trim(name)) > 0),
  email      TEXT    NOT NULL UNIQUE COLLATE NOCASE,
  phone      TEXT,
  city       TEXT,
  created_at TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

-- ---------------------------------------------------------------------------
-- orders
-- Pas de `total_cents` : attribut dérivé, voir la vue `order_totals`.
-- ON DELETE RESTRICT : on ne supprime pas un client qui a un historique.
-- ---------------------------------------------------------------------------
CREATE TABLE orders (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  customer_id INTEGER NOT NULL REFERENCES customers (id) ON DELETE RESTRICT,
  status      TEXT    NOT NULL DEFAULT 'draft'
              CHECK (status IN ('draft', 'confirmed', 'shipped', 'cancelled')),
  created_at  TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
  updated_at  TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE INDEX idx_orders_customer ON orders (customer_id);
-- Sert à la fois le filtre par statut et le tri « plus récentes d'abord »
CREATE INDEX idx_orders_status_created ON orders (status, created_at DESC);

-- ---------------------------------------------------------------------------
-- order_items
-- Clé primaire composite : une commande ne référence un produit qu'une fois
-- (ajouter deux fois le même produit incrémente la quantité côté service).
-- ON DELETE CASCADE sur la commande : les lignes n'existent pas sans elle.
-- ON DELETE RESTRICT sur le produit : cohérent avec le 409 de DELETE /products/:id.
-- ---------------------------------------------------------------------------
CREATE TABLE order_items (
  order_id         INTEGER NOT NULL REFERENCES orders (id)   ON DELETE CASCADE,
  product_id       INTEGER NOT NULL REFERENCES products (id) ON DELETE RESTRICT,
  quantity         INTEGER NOT NULL CHECK (quantity >= 1),
  unit_price_cents INTEGER NOT NULL CHECK (unit_price_cents > 0),
  PRIMARY KEY (order_id, product_id)
);

CREATE INDEX idx_order_items_product ON order_items (product_id);

-- ---------------------------------------------------------------------------
-- Vue : total par commande
-- LEFT JOIN + COALESCE pour qu'une commande sans ligne renvoie 0 et non NULL.
-- ---------------------------------------------------------------------------
CREATE VIEW order_totals AS
SELECT
  o.id                                                   AS order_id,
  COALESCE(SUM(i.quantity * i.unit_price_cents), 0)      AS total_cents,
  COUNT(i.product_id)                                    AS line_count
FROM orders o
LEFT JOIN order_items i ON i.order_id = o.id
GROUP BY o.id;

-- ---------------------------------------------------------------------------
-- Vue : chiffre d'affaires du mois courant (GET /dashboard)
-- Seuls 'confirmed' et 'shipped' comptent ; 'draft' n'est pas vendu,
-- 'cancelled' ne l'est plus.
-- ---------------------------------------------------------------------------
CREATE VIEW revenue_current_month AS
SELECT COALESCE(SUM(t.total_cents), 0) AS revenue_cents
FROM orders o
JOIN order_totals t ON t.order_id = o.id
WHERE o.status IN ('confirmed', 'shipped')
  AND strftime('%Y-%m', o.created_at) = strftime('%Y-%m', 'now');

-- ---------------------------------------------------------------------------
-- Trigger : horodatage de modification
-- Le WHEN empêche la récursion si PRAGMA recursive_triggers est activé.
-- ---------------------------------------------------------------------------
CREATE TRIGGER trg_orders_updated_at
AFTER UPDATE ON orders
FOR EACH ROW
WHEN NEW.updated_at = OLD.updated_at
BEGIN
  UPDATE orders
     SET updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
   WHERE id = NEW.id;
END;

-- ---------------------------------------------------------------------------
-- Trigger : une commande expédiée ou annulée est figée
-- Le contrôle des transitions vit dans le service (il doit renvoyer un 422
-- lisible), mais ce garde-fou empêche une modification directe en base.
-- ---------------------------------------------------------------------------
CREATE TRIGGER trg_order_items_frozen
BEFORE INSERT ON order_items
FOR EACH ROW
WHEN (SELECT status FROM orders WHERE id = NEW.order_id) IN ('shipped', 'cancelled')
BEGIN
  SELECT RAISE(ABORT, 'order_frozen');
END;

-- ============================================================================
-- ALTERNATIVE DÉNORMALISÉE — à n'activer que sur preuve de lenteur
--
-- Si le tableau de bord devient lent (des dizaines de milliers de commandes),
-- remplace la vue par une colonne matérialisée. Le coût : trois triggers à
-- maintenir et un risque de dérive si une écriture passe à côté.
--
--   ALTER TABLE orders ADD COLUMN total_cents INTEGER NOT NULL DEFAULT 0;
--
--   CREATE TRIGGER trg_items_ai AFTER INSERT ON order_items
--   BEGIN
--     UPDATE orders SET total_cents = (SELECT total_cents FROM order_totals
--                                       WHERE order_id = NEW.order_id)
--      WHERE id = NEW.order_id;
--   END;
--   -- idem pour AFTER UPDATE et AFTER DELETE sur order_items
--
-- Mesure d'abord avec EXPLAIN QUERY PLAN. Ne dénormalise jamais par réflexe.
-- ============================================================================
