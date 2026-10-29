import { Link, useParams } from "react-router";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { Button, TextField } from "../../components/ui";

export default function ProductFormPage({ skuError = false }: { skuError?: boolean }) {
  const { id } = useParams();
  return (
    <section className={styles.page}>
      <PageHeader title={id || skuError ? "Modifier le produit" : "Nouveau produit"} back={{ to: "/products", label: "Catalogue" }} />
      <form className={`${styles.card} ${styles.formCard}`} onSubmit={(event) => event.preventDefault()}>
        <div className={styles.formGrid}>
          <TextField className={styles.fullWidth} label="Nom" name="name" />
          <TextField label="SKU" name="sku" mono error={skuError ? "Ce SKU est déjà utilisé par un autre produit." : undefined} />
          <label className={styles.field}>Catégorie<select name="category" defaultValue=""><option value="" disabled>Choisir une catégorie</option><option>Bois</option><option>Céramique</option><option>Textile</option></select></label>
          <TextField label="Prix (CAD)" name="price" type="number" min="0" step="0.01" mono />
          <TextField label="Stock" name="stock" type="number" min="0" step="1" mono hint="Seuil d’alerte : 5 unités." />
        </div>
        <label className={styles.checkbox}><input type="checkbox" name="active" />Actif — disponible pour les nouvelles commandes</label>
        <footer className={styles.formActions}><Link className={styles.secondaryLink} to="/products">Annuler</Link><Button variant="primary">Enregistrer</Button></footer>
      </form>
    </section>
  );
}

