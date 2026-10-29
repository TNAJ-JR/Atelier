import { Link } from "react-router";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";

export default function ClientDetailPage() {
  return (
    <section className={styles.page}>
      <PageHeader title="Fiche client" back={{ to: "/clients", label: "Clients" }} />
      <div className={styles.detailsGrid}>
        <section className={styles.card} aria-label="Coordonnées du client">
          <dl className={styles.details}><dt>Email</dt><dd>—</dd><dt>Téléphone</dt><dd>—</dd><dt>Ville</dt><dd>—</dd></dl>
          <div className="mt-20"><p className="t-th text-tertiary">Total commandé</p><p className="t-h2 mt-6">— $</p></div>
        </section>
        <section className={styles.card}>
          <header className={styles.cardHeader}><h2 className="t-label">Historique des commandes</h2><Link to="/commandes/new">Nouvelle commande pour ce client</Link></header>
          <p className={styles.empty}>Aucune commande à afficher.</p>
        </section>
      </div>
    </section>
  );
}

