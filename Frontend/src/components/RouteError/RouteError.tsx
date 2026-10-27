import { isRouteErrorResponse, useRouteError } from "react-router";
import { Alert } from "../ui/Alert/Alert";
import { Button } from "../ui/Button/Button";
import styles from "./RouteError.module.scss";

export default function RouteError() {
  const error = useRouteError();
  const message = error instanceof Error
    ? error.message
    : "Une erreur est survenue. Veuillez réessayer.";

  return (
    <section className={styles.page} aria-labelledby="route-error-title">
      <h1 id="route-error-title" className="t-h1 mb-20">Chargement impossible</h1>
      <Alert
        title="Impossible d’afficher cette page"
        code={isRouteErrorResponse(error) ? String(error.status) : undefined}
      >
        {message}
      </Alert>
      <Button className="mt-16" onClick={() => window.location.reload()}>
        Réessayer
      </Button>
    </section>
  );
}
