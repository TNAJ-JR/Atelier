import { Link } from "react-router";
import { EmptyTable, PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";

function Clients() {
  return (
    <section className={styles.page}>
      <PageHeader title="Clients" overline="0 client" action={<Link to="/clients/new" className={styles.primaryLink}>Nouveau client</Link>} />
      <EmptyTable label="Liste des clients" columns={["Nom ↑", "Email", "Téléphone", "Ville", "Cmd."]} />
    </section>
  );
}
export default Clients;
