import { cx } from "../../utils/cx";
import styles from "./Logo.module.scss";

type LogoProps = {
  /** Taille de l'icône en pixels (carré). Défaut : 28, aligné sur le wordmark. */
  size?: number;
  /** Affiche le mot « Atelier » à droite de l'icône. */
  withWordmark?: boolean;
  /** "inverse" pour un fond encre (panneau sombre). */
  tone?: "default" | "inverse";
  /** Texte alternatif ; mettre "" si le logo est purement décoratif. */
  title?: string;
  className?: string;
};

/**
 * Logo Atelier — SVG inline, aucune requête réseau.
 * Charte : pas de dégradé, encre + un seul accent terracotta, wordmark en Serif.
 */
export function Logo({
  size = 28,
  withWordmark = false,
  tone = "default",
  title = "Atelier",
  className,
}: LogoProps) {
  // Si le wordmark est affiché, il porte déjà le nom : l'icône devient décorative.
  const decorative = withWordmark || !title;

  return (
    <span className={cx(styles.logo, tone === "inverse" && styles.inverse, className)}>
      <svg
        className={styles.mark}
        viewBox="0 0 512 512"
        width={size}
        height={size}
        role={decorative ? undefined : "img"}
        aria-label={decorative ? undefined : title}
        aria-hidden={decorative || undefined}
        focusable="false"
      >
        <rect className={styles.tile} width="512" height="512" rx="112" />
        <path
          className={styles.letter}
          fillRule="evenodd"
          d="M256 88 L396 424 H338 L308 350 H204 L174 424 H116 Z M256 190 L226 262 H286 Z"
        />
        <rect className={styles.bar} x="190" y="262" width="132" height="42" rx="8" />
        <circle className={styles.hole} cx="212" cy="283" r="7" />
        <circle className={styles.hole} cx="300" cy="283" r="7" />
      </svg>
      {withWordmark && <span className={styles.wordmark}>{title || "Atelier"}</span>}
    </span>
  );
}
