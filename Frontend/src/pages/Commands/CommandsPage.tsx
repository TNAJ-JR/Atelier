import { useState } from "react";
import { Link } from "react-router";
import { EmptyTable, PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { StatusBadge } from "../../components/ui";
import type { OrderStatus } from "../../components/ui/StatusBadge/StatusBadge";

function Commands() {
  const [selected, setSelected] = useState<string>("all");
  return (
    <section className={styles.page}>
      <PageHeader title="Commandes" overline="0 commande" action={<Link className={styles.primaryLink} to="/commandes/new">Nouvelle commande</Link>} />
      <div className={styles.tabs} role="group" aria-label="Statut des commandes">
        <button type="button" aria-pressed={selected === "all"} onClick={() => setSelected("all")}>Toutes · 0</button>
        {(["draft", "confirmed", "shipped", "cancelled"] as OrderStatus[]).map((status) => (
          <button key={status} type="button" aria-pressed={selected === status} onClick={() => setSelected(status)}><StatusBadge status={status} /> · 0</button>
        ))}
      </div>
      <EmptyTable label="Liste des commandes" columns={["N°", "Client", "Créée le", "Lignes", "Statut", "Total"]} />
    </section>
  );
}
export default Commands;
