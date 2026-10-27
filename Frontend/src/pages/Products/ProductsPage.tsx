import { Button } from "../../components/ui/Button/Button";
import styles from "./ProductsPage.module.scss";

function Products() {
  return (
    <section className={styles.page} aria-labelledby="catalogue-title">
      <header className={styles.header}>
        <div>
          <p className="t-overline text-tertiary">0 produit</p>
          <h1 id="catalogue-title" className="t-h1">Catalogue</h1>
        </div>
        <Button variant="primary">Nouveau produit</Button>
      </header>

      <div className={styles.filters}>
        <label className={styles.field}>
          Rechercher par nom
          <input type="search" placeholder="Ex. bol, planche..." />
        </label>
        <label className={styles.field}>
          Catégorie
          <select defaultValue="all">
            <option value="all">Toutes</option>
            <option>Bois</option>
            <option>Céramique</option>
            <option>Textile</option>
          </select>
        </label>
        <label className={styles.field}>
          Trier par
          <select defaultValue="price-asc">
            <option value="price-asc">Prix croissant</option>
            <option>Prix décroissant</option>
            <option>Nom</option>
          </select>
        </label>
      </div>

      <div className={styles.tableWrapper}>
        <table className={styles.table}>
          <caption className="visually-hidden">Catalogue des produits</caption>
          <thead>
            <tr>
              <th scope="col">Nom</th>
              <th scope="col">SKU</th>
              <th scope="col">Catégorie</th>
              <th scope="col" className={styles.number} aria-sort="ascending">Prix ↑</th>
              <th scope="col" className={styles.number}>Stock</th>
              <th scope="col">Statut</th>
              <th scope="col"><span className="visually-hidden">Actions</span></th>
            </tr>
          </thead>
          <tbody />
        </table>
      </div>

      <footer className={styles.footer}>
        <p>0 sur 0</p>
        <nav className={styles.pagination} aria-label="Pagination du catalogue">
          <Button size="sm" disabled>Précédent</Button>
          <Button size="sm" variant="primary" disabled>1</Button>
          <Button size="sm" disabled>Suivant</Button>
        </nav>
      </footer>
    </section>
  );
}
export default Products;

