import type { ButtonHTMLAttributes } from "react";
import { cx } from "../../../utils/cx";
import styles from "./Button.module.scss";

export type ButtonVariant = "primary" | "secondary" | "danger" | "danger-subtle";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant;
  size?: "md" | "sm";
  /** Pleine largeur (42 px de haut, réservé au bouton principal). */
  block?: boolean;
  /** Affiche `loadingLabel` et bloque le bouton pendant une action. */
  loading?: boolean;
  loadingLabel?: string;
};

const variantClass: Record<ButtonVariant, string> = {
  primary: styles.primary,
  secondary: styles.secondary,
  danger: styles.danger,
  "danger-subtle": styles.dangerSubtle,
};

export function Button({
  variant = "secondary",
  size = "md",
  block = false,
  loading = false,
  loadingLabel,
  disabled,
  type = "button",
  className,
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      type={type}
      className={cx(
        styles.button,
        variantClass[variant],
        size === "sm" && styles.sm,
        block && styles.block,
        className,
      )}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading && loadingLabel ? loadingLabel : children}
    </button>
  );
}
