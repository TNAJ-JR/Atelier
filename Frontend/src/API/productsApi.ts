import * as yup from "yup";
import { apiClient } from "./apiClient";
import type { CreateProductRequest, Product } from "../types/Product";
import { productCategories } from "../utils/productValues";

const ProductResponseSchema = yup.object({
  id: yup.number().integer().positive().required(),
  name: yup.string().required(),
  sku: yup.string().required(),
  priceCents: yup.number().integer().min(0).required(),
  stock: yup.number().integer().min(0).required(),
  category: yup.mixed<string | number>().oneOf([0, 1, 2, "bois", "metal", "textile"]).required(),
  active: yup.boolean().required(),
  createdAt: yup.string().required(),
});

export async function getProducts(signal?: AbortSignal): Promise<Product[]> {
  const response = await apiClient<unknown>("products/all", { signal });
  if (!response || typeof response !== "object" || !("isSuccess" in response) || response.isSuccess !== true || !("data" in response)) {
    throw new Error("Réponse invalide lors du chargement des produits.");
  }
  const products = await yup.array(ProductResponseSchema).required().validate(response.data, { strict: true });
  return products.map((product) => ({
    ...product,
    category: productCategories.find((category) => category.value === product.category || category.key === product.category)!.key,
  }));
}

export function createProduct(request: CreateProductRequest): Promise<Product> {
  return apiClient<Product>("products", { method: "POST", body: JSON.stringify(request) });
}
