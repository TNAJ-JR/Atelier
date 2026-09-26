type LogoProps = {
  /** Taille de l'icône en pixels (carré). Défaut : 32 */
  size?: number;
  /** Affiche le mot « Atelier » à droite de l'icône */
  withWordmark?: boolean;
  /** Texte alternatif ; mettre "" si le logo est purement décoratif */
  title?: string;
  className?: string;
};

/**
 * Logo Atelier — inline SVG, aucune requête réseau, se colore avec le texte
 * environnant pour le wordmark (currentColor).
 */
export function Logo({
  size = 32,
  withWordmark = false,
  title = 'Atelier',
  className,
}: LogoProps) {
  const mark = (
    <svg
      viewBox="0 0 512 512"
      width={size}
      height={size}
      role={title ? 'img' : 'presentation'}
      aria-label={title || undefined}
      aria-hidden={title ? undefined : true}
      focusable="false"
    >
      <defs>
        <linearGradient id="atelier-bg" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor="#1F2A37" />
          <stop offset="1" stopColor="#111827" />
        </linearGradient>
      </defs>
      <rect width="512" height="512" rx="112" fill="url(#atelier-bg)" />
      <path
        fill="#F9FAFB"
        fillRule="evenodd"
        d="M256 88 L396 424 H338 L308 350 H204 L174 424 H116 Z M256 190 L226 262 H286 Z"
      />
      <rect x="190" y="262" width="132" height="42" rx="8" fill="#F59E0B" />
      <circle cx="212" cy="283" r="7" fill="#111827" />
      <circle cx="300" cy="283" r="7" fill="#111827" />
    </svg>
  );

  if (!withWordmark) return <span className={className}>{mark}</span>;

  return (
    <span
      className={className}
      style={{ display: 'inline-flex', alignItems: 'center', gap: size * 0.3 }}
    >
      {mark}
      <span
        style={{
          fontSize: size * 0.65,
          fontWeight: 700,
          letterSpacing: '-0.02em',
          color: 'currentColor',
        }}
      >
        Atelier
      </span>
    </span>
  );
}
