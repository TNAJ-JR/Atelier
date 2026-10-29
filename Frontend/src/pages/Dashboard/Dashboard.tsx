import { Link } from "react-router";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";

function Dashboard() {
  return (
    <section className={styles.page}>
      <PageHeader title="Tableau de bord" overline={new Intl.DateTimeFormat("fr-CA", { month: "long", year: "numeric", timeZone: "America/Toronto" }).format(new Date())}
        action={<Link className={styles.primaryLink} to="/commandes/new">Nouvelle commande</Link>} />
      <div className={styles.dashboardGrid}>
        <div className={styles.stack}>
          <section className={styles.card}>
            <h2 className="t-label text-secondary-strong">Chiffre d’affaires du mois</h2>
            <p className="t-kpi mt-12 mb-20">— $</p>
            <p className={styles.muted}>Commandes confirmées et expédiées</p>
          </section>
          <section className={`${styles.card} ${styles.stockCard}`}>
            <header className={styles.cardHeader}><h2 className="t-label">Stock sous le seuil</h2><span className="t-mono">&lt; 5 unités</span></header>
            <p className={styles.empty}>Aucun produit à afficher.</p>
          </section>
        </div>
        <section className={styles.card}>
          <header className={styles.cardHeader}><h2 className="t-label">5 dernières commandes</h2><Link to="/commandes">Toutes les commandes</Link></header>
          <p className={styles.empty}>Aucune commande à afficher.</p>
        </section>
      </div>
    </section>
  );
}
export default Dashboard;
