import { Link } from "react-router";
import { Alert, Button, StatusBadge } from "../../components/ui";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import stateStyles from "./VisualStatesPage.module.scss";

export default function VisualStatesPage() {
  return (
    <section className={styles.page}>
      <PageHeader title="États visuels" back={{ to: "/maquettes", label: "Maquettes" }} />
      <div className={stateStyles.grid}>
        <section><h2 className="t-overline text-tertiary mb-12">Chargement</h2>
          <div className={styles.card} role="status" aria-label="Chargement des produits">
            {[0, 1, 2, 3, 4].map((row) => <div key={row} className={stateStyles.skeletonRow} aria-hidden="true"><span /><span /><span /></div>)}
          </div>
        </section>
        <section><h2 className="t-overline text-tertiary mb-12">Vide</h2>
          <div className={`${styles.card} ${stateStyles.empty}`}><h3 className="t-h2">Aucun produit</h3><p className="text-secondary">Aucun produit ne correspond aux filtres sélectionnés.</p><Button>Réinitialiser les filtres</Button></div>
        </section>
        <section><h2 className="t-overline text-tertiary mb-12">Erreur serveur</h2>
          <Alert title="Suppression impossible" code="409 · PRODUCT_IN_USE">Ce produit figure dans une commande non annulée.</Alert>
          <div className={`${styles.card} ${stateStyles.retry}`}><p>Le catalogue n’a pas pu être chargé.</p><Button variant="primary" size="sm">Réessayer</Button></div>
        </section>
      </div>
      <section className={`${styles.card} mt-24`}>
        <h2 className="t-label mb-16">Statuts des commandes</h2>
        <div className="d-flex flex-wrap gap-12"><StatusBadge status="draft" /><StatusBadge status="confirmed" /><StatusBadge status="shipped" /><StatusBadge status="cancelled" /></div>
      </section>
      <div className="d-flex flex-wrap gap-12 mt-24"><Link to="/maquettes/produit-invalide">Champ SKU invalide</Link><Link to="/maquettes/commande-invalide">Quantité invalide</Link><Link to="/maquettes/erreur">Erreur de page</Link></div>
    </section>
  );
}

