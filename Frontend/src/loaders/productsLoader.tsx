import type { LoaderFunctionArgs } from "react-router";
import { getProducts } from "../API/productsApi";

export function productsLoader({ request }: LoaderFunctionArgs) {
  return { products: getProducts(request.signal) };
}
