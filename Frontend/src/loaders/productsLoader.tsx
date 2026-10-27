import { apiClient } from "../API/apiClient";
import type { Product } from "../types/Product";

export async function productsLoader() {
  const products = await apiClient<Product[]>("products/all");

  if (!Array.isArray(products)) {
    throw new Error("Réponse invalide lors du chargement des produits.");
  }

  return products;
}
