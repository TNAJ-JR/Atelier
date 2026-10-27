import { apiClient } from "../API/apiClient";
import type { Product } from "../types/Product";

export async function productsLoader() {
  let products: Product[];

  try {
    products = await apiClient<Product[]>("products/all");
  } catch (error) {
    if (error instanceof TypeError) {
      throw new Error(
        "Impossible de joindre le serveur. Vérifiez votre connexion ou réessayez plus tard.",
        { cause: error },
      );
    }

    throw error;
  }

  if (!Array.isArray(products)) {
    throw new Error("Réponse invalide lors du chargement des produits.");
  }

  return products;
}
