import { Link, useLocation } from "react-router";
import styles from "../components/layout/PageLayout.module.scss";

function NotFoundPage() {
  const { pathname } = useLocation();
  return (
    <section className={styles.errorPage}>
      <p className="t-overline text-tertiary">404</p>
      <h1 className="t-h1">Cette page n’existe pas.</h1>
      <p className="text-secondary">L’adresse <code className="t-mono">{pathname}</code> ne correspond à aucun écran.</p>
      <Link className={styles.primaryLink} to="/">Retour au tableau de bord</Link>
    </section>
  );
}
export default NotFoundPage;
