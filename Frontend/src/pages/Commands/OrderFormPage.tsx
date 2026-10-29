import { useRef, useState } from "react";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { Button } from "../../components/ui";

export default function OrderFormPage({ quantityError = false }: { quantityError?: boolean }) {
  const nextLine = useRef(2);
  const [lines, setLines] = useState([1]);
  return (
    <section className={styles.page}>
      <PageHeader title="Nouvelle commande" back={{ to: "/commandes", label: "Commandes" }} />
      <div className={styles.orderGrid}>
        <div>
          <label className={`${styles.field} ${styles.orderClient}`}>Client<select defaultValue=""><option value="" disabled>Choisir un client</option></select></label>
          <div className={styles.tableWrapper}>
            <table className={styles.table}>
              <caption className="visually-hidden">Lignes de la commande</caption>
              <thead><tr>{["Produit", "Quantité", "Prix unit.", "Sous-total", ""].map((label, index) => <th key={index} scope="col">{label || <span className="visually-hidden">Actions</span>}</th>)}</tr></thead>
              <tbody>{lines.map((line, index) => (
                <tr key={line}>
                  <td><select aria-label={`Produit de la ligne ${index + 1}`} defaultValue=""><option value="" disabled>Choisir un produit</option></select></td>
                  <td><input aria-label={`Quantité de la ligne ${index + 1}`} type="number" min="1" defaultValue={quantityError && index === 0 ? 0 : 1} aria-invalid={quantityError && index === 0 ? true : undefined} aria-describedby={quantityError && index === 0 ? "quantity-error" : undefined} />{quantityError && index === 0 && <p id="quantity-error" className={styles.fieldError}>Minimum 1</p>}</td>
                  <td className="t-mono">— $</td><td className="t-mono">— $</td>
                  <td><Button size="sm" aria-label={`Retirer la ligne ${index + 1}`} onClick={() => setLines(lines.filter((id) => id !== line))}>×</Button></td>
                </tr>
              ))}</tbody>
            </table>
            <div className={styles.addLine}><Button size="sm" onClick={() => setLines([...lines, nextLine.current++])}>+ Ajouter une ligne</Button></div>
          </div>
        </div>
        <aside className={styles.summary} aria-label="Récapitulatif de la commande">
          <h2 className="t-label mb-12">Total</h2>
          <p className="t-total">— $</p>
          <p className={styles.muted}>Prix figés à la création. La commande est créée en brouillon ; le stock est décrémenté à la confirmation.</p>
          <Button variant="primary" block disabled>Créer la commande</Button>
          {quantityError && <p className={`${styles.muted} mt-16`}>Corrigez la quantité pour continuer.</p>}
        </aside>
      </div>
    </section>
  );
}

