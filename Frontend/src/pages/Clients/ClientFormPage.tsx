import { Link } from "react-router";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { Button, TextField } from "../../components/ui";

export default function ClientFormPage() {
  return (
    <section className={styles.page}>
      <PageHeader title="Nouveau client" back={{ to: "/clients", label: "Clients" }} />
      <form className={`${styles.card} ${styles.formCard}`} onSubmit={(event) => event.preventDefault()}>
        <div className={styles.formGrid}>
          <TextField label="Nom" name="name" className={styles.fullWidth} />
          <TextField label="Email" name="email" type="email" />
          <TextField label="Téléphone" name="phone" type="tel" />
          <TextField label="Ville" name="city" className={styles.fullWidth} />
        </div>
        <footer className={styles.formActions}><Link className={styles.secondaryLink} to="/clients">Annuler</Link><Button variant="primary">Enregistrer</Button></footer>
      </form>
    </section>
  );
}

