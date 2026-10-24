import { useId, type InputHTMLAttributes, type Ref } from "react";
import { cx } from "../../../utils/cx";
import styles from "./TextField.module.scss";

type TextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  /** Message d'erreur : active aria-invalid et le style d'erreur. */
  error?: string;
  /** Texte d'aide affiché sous le champ. */
  hint?: string;
  /** Saisie en JetBrains Mono (SKU, prix, quantités). */
  mono?: boolean;
  /** React 19 : `ref` est une prop normale (compatible avec register() de react-hook-form). */
  ref?: Ref<HTMLInputElement>;
};

export function TextField({
  label,
  error,
  hint,
  mono = false,
  id,
  className,
  ref,
  ...inputProps
}: TextFieldProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  const hintId = hint ? `${inputId}-hint` : undefined;
  const errorId = error ? `${inputId}-error` : undefined;
  const describedBy = [hintId, errorId].filter(Boolean).join(" ") || undefined;

  return (
    <div className={cx(styles.field, className)}>
      <label className={styles.label} htmlFor={inputId}>
        {label}
      </label>
      <input
        ref={ref}
        id={inputId}
        className={cx(styles.input, mono && styles.mono)}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...inputProps}
      />
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className={styles.error} role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
