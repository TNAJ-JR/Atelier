export type Product = {
  id: number;
  name: string;
  sku: string;
  stock: number;
  category: string;
  priceCents: number;
  active: boolean;
  createdAt: string;
};

export type CreateProductRequest = {
  sku: string;
  name: string;
  priceCents: number;
  stock: number;
  category: number;
  active: boolean;
};
