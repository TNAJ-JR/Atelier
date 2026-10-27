import { useRouteError } from "react-router";
import { Alert } from "../../components/ui/Alert/Alert";
import { Button } from "../../components/ui/Button/Button";
import styles from "./ProductsPage.module.scss";

export default function ProductsError() {
  const error = useRouteError();

  return (
    <section className={styles.page} aria-labelledby="products-error-title">
      <header className={styles.header}>
        <h1 id="products-error-title" className="t-h1">Catalogue</h1>
      </header>
      <Alert title="Impossible de charger les produits">
        {error instanceof Error ? error.message : "Une erreur est survenue. Veuillez réessayer."}
      </Alert>
      <Button className="mt-16" onClick={() => window.location.reload()}>
        Réessayer
      </Button>
    </section>
  );
}
