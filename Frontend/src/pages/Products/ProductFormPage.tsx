import { Link, useNavigate, useParams } from "react-router";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import { PageHeader } from "../../components/layout/PageLayout";
import styles from "../../components/layout/PageLayout.module.scss";
import { Alert, Button, TextField } from "../../components/ui";
import { ApiError } from "../../API/apiClient";
import { createProduct } from "../../API/productsApi";
import { ProductFormSchema, type ProductFormValues } from "../../schemas/productSchema";
import { priceToCents, productCategories } from "../../utils/productValues";

export default function ProductFormPage({ skuError = false }: { skuError?: boolean }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [apiError, setApiError] = useState("");
  const isPreview = Boolean(id) || skuError;
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<ProductFormValues>({
    resolver: yupResolver(ProductFormSchema),
    defaultValues: { name: "", sku: "", price: "", stock: 0, active: true },
  });

  async function onSubmit(values: ProductFormValues) {
    if (isPreview) return;
    setApiError("");
    const priceCents = priceToCents(values.price);
    if (priceCents === null) return;
    try {
      await createProduct({
        name: values.name, sku: values.sku, priceCents,
        stock: values.stock, category: values.category, active: values.active,
      });
      await navigate("/products", { replace: true, state: { productCreated: true } });
    } catch (error) {
      if (error instanceof ApiError) {
        const fieldNames: Record<string, keyof ProductFormValues> = {
          sku: "sku", name: "name", pricecents: "price", stock: "stock", category: "category", active: "active",
        };
        let hasFieldError = false;
        for (const [name, messages] of Object.entries(error.fieldErrors)) {
          const field = fieldNames[name.split(".").at(-1)!.toLowerCase()];
          if (field && messages.length) {
            setError(field, { type: "server", message: messages.join(" ") }, { shouldFocus: !hasFieldError });
            hasFieldError = true;
          }
        }
        if (hasFieldError) return;
        if (error.status === 401) {
          setApiError("Votre session a expiré. Reconnectez-vous avant de réessayer.");
          return;
        }
        if (error.status === 403) {
          setApiError("Vous n’avez pas les droits nécessaires pour créer un produit.");
          return;
        }
      }
      setApiError(error instanceof Error ? error.message : "Impossible de créer le produit.");
    }
  }

  return (
    <section className={styles.page}>
      <PageHeader title={isPreview ? "Modifier le produit" : "Nouveau produit"} back={{ to: "/products", label: "Catalogue" }} />
      <form className={`${styles.card} ${styles.formCard}`} onSubmit={handleSubmit(onSubmit)} noValidate>
        {apiError && <div className="mb-20"><Alert>{apiError}</Alert></div>}
        {id && <p className="text-secondary mb-20">La modification n’est pas encore disponible.</p>}
        <fieldset className={styles.formFields} disabled={isSubmitting}>
          <div className={styles.formGrid}>
            <TextField className={styles.fullWidth} label="Nom" maxLength={200} error={errors.name?.message} {...register("name")} />
            <TextField label="SKU" maxLength={450} mono error={errors.sku?.message ?? (skuError ? "Ce SKU est déjà utilisé par un autre produit." : undefined)} {...register("sku")} />
            <div>
              <div className={styles.field}>
                <label htmlFor="product-category">Catégorie</label>
                <select id="product-category" defaultValue="" aria-invalid={errors.category ? true : undefined} aria-describedby={errors.category ? "category-error" : undefined} {...register("category", { valueAsNumber: true })}>
                  <option value="" disabled>Choisir une catégorie</option>
                  {productCategories.map((category) => <option key={category.value} value={category.value}>{category.label}</option>)}
                </select>
              </div>
              {errors.category && <p id="category-error" role="alert" className={styles.fieldError}>{errors.category.message}</p>}
            </div>
            <TextField label="Prix (CAD)" inputMode="decimal" placeholder="0,00" mono error={errors.price?.message} {...register("price")} />
            <TextField label="Stock" type="number" min="0" step="1" mono hint="Seuil d’alerte : 5 unités." error={errors.stock?.message} {...register("stock", { valueAsNumber: true })} />
          </div>
          <label className={styles.checkbox}><input type="checkbox" {...register("active")} />Actif — disponible pour les nouvelles commandes</label>
        </fieldset>
        <footer className={styles.formActions}>
          {isSubmitting ? <Button disabled>Annuler</Button> : <Link className={styles.secondaryLink} to="/products">Annuler</Link>}
          <Button type="submit" variant="primary" disabled={isPreview} loading={isSubmitting} loadingLabel="Enregistrement…">Enregistrer</Button>
        </footer>
      </form>
    </section>
  );
}
