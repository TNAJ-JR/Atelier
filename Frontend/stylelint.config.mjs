/**
 * Stylelint — garde-fou de la charte graphique Atelier.
 *
 * Règle d'or : aucune couleur, aucun espacement, aucun rayon, aucune taille
 * de police en dur hors de src/styles/tokens. Tout passe par var(--…).
 *
 * Lancer : npm run lint:css  (ou npm run lint:css -- --fix)
 */

const STRICT_PROPERTIES = [
  // Couleurs
  '/color$/',
  'background',
  'fill',
  'stroke',
  'box-shadow',
  // Espacements
  'gap',
  'row-gap',
  'column-gap',
  '/^padding/',
  '/^margin/',
  // Formes et typo
  'border-radius',
  'font-size',
  'font-weight',
  'font-family',
]

/** @type {import('stylelint').Config} */
export default {
  extends: ['stylelint-config-standard-scss'],
  plugins: ['stylelint-declaration-strict-value'],
  ignoreFiles: ['dist/**', 'node_modules/**'],
  rules: {
    // Aucun hex hors des tokens
    'color-no-hex': true,
    'color-named': 'never',

    // Valeurs de la charte obligatoires (var(--…), $variable ou fonction)
    'scale-unlimited/declaration-strict-value': [
      STRICT_PROPERTIES,
      {
        ignoreValues: [
          '0',
          'auto',
          'none',
          'inherit',
          'transparent',
          'currentcolor',
          'unset',
          'initial',
        ],
        disableFix: true,
        message: '« ${property}: ${value} » est hors charte : utiliser un token var(--…) (src/styles/tokens).',
      },
    ],

    // CSS Modules : classes en camelCase (styles.brandPanel) ou kebab-case (globales)
    'selector-class-pattern': [
      '^([a-z][a-zA-Z0-9]*|[a-z][a-z0-9]*(-[a-z0-9]+)*)$',
      { message: 'Classe en camelCase (module) ou kebab-case (globale).' },
    ],
    // Commentaires alignés dans les tokens
    'scss/double-slash-comment-whitespace-inside': null,
    'scss/comment-no-empty': null,
    'scss/dollar-variable-empty-line-before': null,
  },
  overrides: [
    {
      // Seul endroit où les valeurs brutes sont autorisées
      files: ['src/styles/tokens/**/*.scss'],
      rules: {
        'color-no-hex': null,
        'scale-unlimited/declaration-strict-value': null,
      },
    },
  ],
}
