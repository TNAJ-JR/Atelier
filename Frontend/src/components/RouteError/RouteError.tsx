import { isRouteErrorResponse, useRouteError } from "react-router";
import { Alert } from "../ui/Alert/Alert";
import { Button } from "../ui/Button/Button";
import styles from "../layout/PageLayout.module.scss";

export default function RouteError() {
  const error = useRouteError();
  const message = error instanceof Error
    ? error.message
    : "Une erreur est survenue. Veuillez réessayer.";

  return <RouteErrorView message={message} code={isRouteErrorResponse(error) ? String(error.status) : undefined} />;
}

export function RouteErrorView({ message = "La navigation reste disponible. Vous pouvez réessayer de charger cet écran.", code }: { message?: string; code?: string }) {
  return (
    <section className={styles.errorPage} aria-labelledby="route-error-title">
      <p className={`t-overline ${styles.danger}`}>Erreur de rendu</p>
      <h1 id="route-error-title" className="t-h1">Cet écran a rencontré un problème.</h1>
      <Alert
        title="Impossible d’afficher cette page"
        code={code}
      >
        {message}
      </Alert>
      <Button className="mt-16" onClick={() => window.location.reload()}>
        Réessayer
      </Button>
    </section>
  );
}
