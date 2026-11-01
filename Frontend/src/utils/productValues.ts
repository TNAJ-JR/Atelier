export const productCategories = [
  { value: 0, key: "bois", label: "Bois" },
  { value: 1, key: "metal", label: "Métal" },
  { value: 2, key: "textile", label: "Textile" },
] as const;

export function priceToCents(price: string): number | null {
  const normalized = price.trim().replace(",", ".");
  if (!/^\d+(?:\.\d{1,2})?$/.test(normalized)) return null;
  const [whole, fraction = ""] = normalized.split(".");
  const cents = Number(whole) * 100 + Number(fraction.padEnd(2, "0"));
  return Number.isSafeInteger(cents) && cents <= 2_147_483_647 ? cents : null;
}

export const formatPrice = new Intl.NumberFormat("fr-CA", { style: "currency", currency: "CAD" });
