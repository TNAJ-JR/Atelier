import { Alert, Button } from "../../components/ui";
import styles from "./ProductsPage.module.scss";
import { Await, Link, useAsyncError, useLoaderData, useLocation, useRevalidator } from "react-router";
import { Suspense, useState, type ReactNode } from "react";
import pageStyles from "../../components/layout/PageLayout.module.scss";
import type { productsLoader } from "../../loaders/productsLoader";
import type { Product } from "../../types/Product";
import { formatPrice, productCategories } from "../../utils/productValues";

const emptyProducts = Promise.resolve<Product[]>([]);
type Filters = { search: string; category: string; sort: string };

function Products() {
  const data = useLoaderData<typeof productsLoader>();
  const productsPromise = data?.products ?? emptyProducts;
  const { state } = useLocation();
  const revalidator = useRevalidator();
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("all");
  const [sort, setSort] = useState("price-asc");
  const [page, setPage] = useState(1);
  const [attempt, setAttempt] = useState(0);

  async function retry() {
    await revalidator.revalidate();
    setAttempt((value) => value + 1);
  }

  return (
    <section className={styles.page} aria-labelledby="catalogue-title">
      <header className={styles.header}>
        <div>
          <p className="t-overline text-tertiary">
            <Suspense fallback="Chargement des produits…">
              <Await key={attempt} resolve={productsPromise} errorElement={<>Nombre de produits indisponible</>}>
                {(products: Product[]) => <>{products.length} produit{products.length > 1 ? "s" : ""}</>}
              </Await>
            </Suspense>
          </p>
          <h1 id="catalogue-title" className="t-h1">Catalogue</h1>
        </div>
        <Link className={pageStyles.primaryLink} to="/products/new">Nouveau produit</Link>
      </header>
      {state?.productCreated && <p role="status" className="mb-20">Le produit a été créé.</p>}

      <div className={styles.filters}>
        <label className={styles.field}>
          Rechercher par nom
          <input type="search" placeholder="Ex. bol, planche..." value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} />
        </label>
        <div className={styles.field}>
          <label htmlFor="catalogue-category">Catégorie</label>
          <select id="catalogue-category" value={category} onChange={(event) => { setCategory(event.target.value); setPage(1); }}>
            <option value="all">Toutes</option>
            {productCategories.map((item) => <option key={item.key} value={item.key}>{item.label}</option>)}
          </select>
        </div>
        <div className={styles.field}>
          <label htmlFor="catalogue-sort">Trier par</label>
          <select id="catalogue-sort" value={sort} onChange={(event) => { setSort(event.target.value); setPage(1); }}>
            <option value="price-asc">Prix croissant</option>
            <option value="price-desc">Prix décroissant</option>
            <option value="name">Nom</option>
          </select>
        </div>
      </div>

      <Suspense fallback={
        <ProductsTable sort={sort}>
          <tr><td colSpan={7} className={styles.emptyState}><p role="status">Chargement des produits…</p></td></tr>
        </ProductsTable>
      }>
        <Await key={attempt} resolve={productsPromise} errorElement={
          <ProductsLoadError sort={sort} onRetry={retry} retrying={revalidator.state === "loading"} />
        }>
          {(products: Product[]) => <ProductsResults products={products} filters={{ search, category, sort }} page={page} onPageChange={setPage} />}
        </Await>
      </Suspense>
    </section>
  );
}

function ProductsTable({ sort, children }: { sort: string; children?: ReactNode }) {
  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <caption className="visually-hidden">Catalogue des produits</caption>
        <thead>
          <tr>
            <th scope="col">Nom</th>
            <th scope="col">SKU</th>
            <th scope="col">Catégorie</th>
            <th scope="col" className={styles.number} aria-sort={sort === "price-asc" ? "ascending" : sort === "price-desc" ? "descending" : undefined}>Prix {sort === "name" ? "" : sort === "price-asc" ? "↑" : "↓"}</th>
            <th scope="col" className={styles.number}>Stock</th>
            <th scope="col">Statut</th>
            <th scope="col"><span className="visually-hidden">Actions</span></th>
          </tr>
        </thead>
        <tbody>{children}</tbody>
      </table>
    </div>
  );
}

function ProductsLoadError({ sort, onRetry, retrying }: { sort: string; onRetry: () => Promise<void>; retrying: boolean }) {
  const error = useAsyncError();
  return (
    <>
      <ProductsTable sort={sort} />
      <div className={styles.loadError}>
        <Alert title="Impossible de charger les produits">
          {error instanceof Error ? error.message : "Les produits n’ont pas pu être récupérés. Veuillez réessayer."}
        </Alert>
        <Button onClick={() => void onRetry()} loading={retrying} loadingLabel="Chargement…">Réessayer</Button>
      </div>
    </>
  );
}

function ProductsResults({ products, filters, page, onPageChange }: {
  products: Product[]; filters: Filters; page: number; onPageChange: (page: number) => void;
}) {
  const filteredProducts = products.filter((product) =>
    product.name.toLocaleLowerCase("fr-CA").includes(filters.search.trim().toLocaleLowerCase("fr-CA")) &&
    (filters.category === "all" || product.category === filters.category),
  ).sort((first, second) => filters.sort === "name"
    ? first.name.localeCompare(second.name, "fr-CA")
    : (filters.sort === "price-desc" ? second.priceCents - first.priceCents : first.priceCents - second.priceCents) || first.id - second.id);
  const pageCount = Math.max(1, Math.ceil(filteredProducts.length / 10));
  const currentPage = Math.min(page, pageCount);
  const visibleProducts = filteredProducts.slice((currentPage - 1) * 10, currentPage * 10);

  return (
    <>
      <ProductsTable sort={filters.sort}>
        {visibleProducts.map((product) => <tr key={product.id}>
          <td className={styles.productName}>{product.name}</td>
          <td className={styles.sku}>{product.sku}</td>
          <td className={styles.category}>{productCategories.find((item) => item.key === product.category)?.label}</td>
          <td className={styles.number}>{formatPrice.format(product.priceCents / 100)}</td>
          <td className={`${styles.number} ${product.stock < 5 ? styles.lowStock : ""}`}>{product.stock}</td>
          <td className={product.active ? styles.status : styles.inactive}>{product.active ? "Actif" : "Inactif"}</td>
          <td><div className={styles.actions}><Button size="sm" disabled>Modifier</Button><Button size="sm" disabled>Supprimer</Button></div></td>
        </tr>)}
        {!visibleProducts.length && <tr><td colSpan={7} className={styles.emptyState}>
          {products.length ? "Aucun produit ne correspond aux filtres." : "Aucun produit à afficher."}
        </td></tr>}
      </ProductsTable>
      <footer className={styles.footer}>
        <p>{filteredProducts.length ? `${(currentPage - 1) * 10 + 1}–${Math.min(currentPage * 10, filteredProducts.length)}` : "0"} sur {filteredProducts.length}</p>
        <nav className={styles.pagination} aria-label="Pagination du catalogue">
          <Button size="sm" disabled={currentPage === 1} onClick={() => onPageChange(currentPage - 1)}>Précédent</Button>
          <Button size="sm" variant="primary" aria-current="page" disabled={!filteredProducts.length}>{currentPage}</Button>
          <Button size="sm" disabled={currentPage === pageCount} onClick={() => onPageChange(currentPage + 1)}>Suivant</Button>
        </nav>
      </footer>
    </>
  );
}

export default Products;
