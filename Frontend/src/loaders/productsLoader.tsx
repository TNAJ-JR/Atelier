import { apiClient } from "../API/apiClient";
import type { Product } from "../types/Product";

export async function productsLoader() {
  const result = await apiClient<Promise<Product[]>>("products/all", {
    method: "GET",
  });
  if (!result) {
    throw new Error(
      JSON.stringify({
        message: "Impossible de charger les produits.",
        status: 500,
      }),
    );
  }
  return result;
}
