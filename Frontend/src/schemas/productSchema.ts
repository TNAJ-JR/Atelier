import * as yup from "yup";
import { priceToCents } from "../utils/productValues";

export const ProductFormSchema = yup.object({
  name: yup.string().trim().required("Le nom est obligatoire.").max(200, "Maximum 200 caractères."),
  sku: yup.string().trim().required("Le SKU est obligatoire.").max(450, "Maximum 450 caractères."),
  price: yup.string().trim().required("Le prix est obligatoire.")
    .test("price", "Entrez un prix positif ou nul avec au plus deux décimales (maximum 21 474 836,47 $).", (value) => !value || priceToCents(value) !== null),
  stock: yup.number().typeError("Le stock est obligatoire.").integer("Le stock doit être entier.")
    .min(0, "Le stock ne peut pas être négatif.").max(2_147_483_647, "Le stock est trop élevé.").required(),
  category: yup.number().typeError("Choisissez une catégorie.").oneOf([0, 1, 2], "Choisissez une catégorie valide.").required("Choisissez une catégorie."),
  active: yup.boolean().required(),
});

export type ProductFormValues = yup.InferType<typeof ProductFormSchema>;
