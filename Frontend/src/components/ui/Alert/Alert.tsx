import type { ReactNode } from "react";
import { cx } from "../../../utils/cx";
import styles from "./Alert.module.scss";

type AlertProps = {
  /** Ce qui s'est passé — ex. « Suppression impossible ». */
  title?: string;
  /** La raison — ex. « Ce produit figure dans une commande non annulée. » */
  children: ReactNode;
  /** Code serveur affiché en mono — ex. « 409 · PRODUCT_IN_USE ». */
  code?: string;
  className?: string;
};

export function Alert({ title, children, code, className }: AlertProps) {
  return (
    <div className={cx(styles.alert, className)} role="alert">
      {title && <p className={styles.title}>{title}</p>}
      <p>{children}</p>
      {code && <p className={styles.code}>{code}</p>}
    </div>
  );
}
