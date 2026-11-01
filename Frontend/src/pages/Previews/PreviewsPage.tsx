import { Link } from "react-router";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import stateStyles from "./VisualStatesPage.module.scss";

const screens = [
  ["Connexion", "/login"],
  ["Tableau de bord", "/dashboard"],
  ["Catalogue", "/maquettes/catalogue"],
  ["Nouveau produit", "/products/new"],
  ["Modification d’un produit", "/products/apercu/edit"],
  ["Clients", "/clients"],
  ["Fiche client", "/clients/apercu"],
  ["Nouveau client", "/clients/new"],
  ["Commandes", "/commandes"],
  ["Nouvelle commande", "/commandes/new"],
  ["Détail et confirmation d’annulation", "/commandes/apercu"],
  ["Chargement, vide et erreurs", "/maquettes/etats"],
  ["Page introuvable", "/maquettes/introuvable"],
];

export default function PreviewsPage() {
  return (
    <section className={styles.page}>
      <PageHeader title="Maquettes" overline="Atelier · Interfaces" />
      <p className="text-secondary mb-24">Aperçu des interfaces sans données de démonstration. Le formulaire Nouveau produit enregistre les données ; les autres formulaires restent des aperçus.</p>
      <div className={stateStyles.previewLinks}>{screens.map(([label, to], index) => <Link key={to} to={to}><span className="t-overline text-tertiary">{String(index + 1).padStart(2, "0")}</span><span className="t-h2">{label}</span><span className="text-secondary">Ouvrir →</span></Link>)}</div>
    </section>
  );
}

