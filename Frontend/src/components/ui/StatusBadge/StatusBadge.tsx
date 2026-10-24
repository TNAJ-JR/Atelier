import { cx } from "../../../utils/cx";
import styles from "./StatusBadge.module.scss";

export type OrderStatus = "draft" | "confirmed" | "shipped" | "cancelled";

const labels: Record<OrderStatus, string> = {
  draft: "Brouillon",
  confirmed: "Confirmée",
  shipped: "Expédiée",
  cancelled: "Annulée",
};

type StatusBadgeProps = {
  status: OrderStatus;
  className?: string;
};

export function StatusBadge({ status, className }: StatusBadgeProps) {
  return <span className={cx(styles.badge, styles[status], className)}>{labels[status]}</span>;
}
