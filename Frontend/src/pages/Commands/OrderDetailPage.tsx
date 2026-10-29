import { useRef } from "react";
import { EmptyTable, PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { Button } from "../../components/ui";
import modalStyles from "./OrderDetailPage.module.scss";

export default function OrderDetailPage() {
  const dialog = useRef<HTMLDialogElement>(null);
  return (
    <section className={styles.page}>
      <PageHeader title="Détail de la commande" back={{ to: "/commandes", label: "Commandes" }}
        action={<div className="d-flex flex-wrap gap-10"><Button onClick={() => dialog.current?.showModal()}>Annuler la commande</Button><Button variant="primary" disabled>Marquer expédiée</Button></div>} />
      <p className="text-secondary mb-24">Client — · Date de création — · Statut —</p>
      <div className={modalStyles.orderTable}>
        <EmptyTable label="Produits de la commande" columns={["Produit", "Qté", "Prix unit.", "Sous-total"]} />
        <div className={styles.total}><span>Total</span><span className="t-h2">— $</span></div>
      </div>
      <dialog ref={dialog} className={modalStyles.dialog} aria-labelledby="cancel-title" aria-describedby="cancel-description">
        <h2 id="cancel-title" className="t-h2 mb-16">Annuler la commande ?</h2>
        <p id="cancel-description" className="text-secondary">Le stock sera restauré si la commande est confirmée. Cette action est définitive.</p>
        <div className={modalStyles.actions}><Button autoFocus onClick={() => dialog.current?.close()}>Garder</Button><Button variant="danger" disabled>Annuler la commande</Button></div>
        <p className={`${modalStyles.hint} t-mono`}>Échap pour fermer</p>
      </dialog>
    </section>
  );
}

