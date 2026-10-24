# Connexion du backend

## Routes
- POST /api/auth/login : JSON { "email": "...", "password": "..." }.
- POST /api/auth/refresh : JSON { "refreshToken": "..." }.
- GET /api/auth/me : Authorization: Bearer <token>.
- POST /api/auth/logout : Authorization: Bearer <token>, réponse 204.

Une connexion ou un renouvellement renvoie directement token, refreshToken, expiresAt
et user (id chaîne, name, email, nom, prenom, roles), sans enveloppe ServiceResult.
Les erreurs métier exposent message et errors avec le statut HTTP approprié.
Les requêtes mal formées retournent les ValidationProblemDetails ASP.NET.

## Sessions et droits
Les mots de passe sont vérifiés par Identity. Les échecs verrouillent le compte après
5 tentatives pour 5 minutes. Les routes login/refresh sont limitées par IP à 10/minute
(configurable via Auth:RequestsPerMinute).
Seuls les hash SHA-256 sont persistés. Chaque connexion possède une famille.
Un renouvellement conserve le hash précédent, renseigne UsedAt et ReplacedByTokenId,
puis crée un successeur avec la même FamilyId et la même expiration absolue.
La session expire après Jwt:RefreshTokenExpirationDays (14 par défaut), sans prolongation.
Les JWT expirent après Jwt:ExpirationMinutes (15 par défaut), au plus tard avec la session.

Le JWT porte l'identifiant de session et la version des identifiants Identity.
Chaque accès vérifie la session, le verrouillage, le security stamp et les rôles actuels.
La déconnexion révoque donc aussi les JWT déjà émis pour cette session.
Une rotation seule laisse les anciens JWT valides jusqu'à expiration ou révocation.
La migration invalide implicitement les anciennes sessions sans security stamp ; il faut se reconnecter.

Les rôles JWT viennent exclusivement de UserManager.GetRolesAsync, donc AspNetUserRoles.
ApplicationUser.Role est conservé pour compatibilité du schéma ; ne pas l'utiliser comme
une seconde source de droits. Les routes métier exigent une authentification par défaut.
Utiliser [Authorize(Roles = "Admin")] pour les opérations réservées à l'administration.

## Configuration
La clé Jwt:Key existante reste en Base64 et n'a pas été modifiée.
Jwt:Issuer et Jwt:Audience sont configurés en développement : Atelier.Api / Atelier.Frontend.
Configurer les valeurs adaptées et les secrets via environnement ou secrets utilisateur.
Cors:AllowedOrigins définit les origines autorisées ; en développement, localhost:5173
est utilisé par défaut. HTTPS est requis pour les vrais échanges de jetons.

Les rôles Admin/Staff/Client sont initialisés au démarrage de façon idempotente.
Aucun compte à mot de passe codé en dur n'est créé.
Pour des comptes locaux de développement uniquement, définir SeedUsers:Enabled=true et,
dans les secrets, SeedUsers:Admin:Email / Password et SeedUsers:Staff:Email / Password.
Les champs optionnels Prenom et Nom sont pris en charge. Un compte existant n'est pas
réinitialisé. L'initialisation des comptes est désactivée hors Development.

## Migration et validation
SecureRefreshTokenSessions ajoute le security stamp, la rowversion et l'index unique du hash.
Elle refuse les valeurs existantes trop longues ou dupliquées plutôt que les supprimer.
La factory EF permet les migrations sans démarrer le serveur ni initialiser les comptes.

La rotation a été vérifiée sur une base SQL temporaire, puis cette base a été supprimée. Aucun script de test n'est conservé dans le projet.

Le frontend existant sait lire la réponse de connexion. Pour exploiter toute la session,
il devra conserver le refreshToken, appeler refresh et remplacer les jetons reçus,
puis appeler logout avant de supprimer les identifiants locaux. Une suppression locale seule
ne révoque pas la session côté serveur.


## Rotation avec historique
TrackRefreshTokenRotation ajoute les familles de sessions, UsedAt et le lien ReplacedByTokenId.
Les jetons existants reçoivent chacun une famille distincte ; leurs hash restent inchangés.
Une transaction SQL et un verrou UPDLOCK/HOLDLOCK sur la famille sérialisent les
renouvellements, les détections de rejeu et la déconnexion, y compris les descendants.
Un hash inconnu renvoie 401 sans révoquer de famille. Un hash déjà consommé révoque
la famille et donc tous ses refresh tokens et JWT, sans affecter les autres connexions.
La révocation commune est stockée dans RefreshTokenFamilies.RevokedAt ; elle ne nécessite
pas de réécrire chaque ligne d'historique. IsActive décrit uniquement l'état local du jeton ;
le service vérifie également la famille. Les anciens JWT restent utilisables après une
rotation normale, jusqu'à leur expiration ou la révocation de la famille.

Le frontend doit mutualiser les appels refresh simultanés et remplacer les jetons avant
les demandes suivantes. Une répétition du même refresh, même à cause d'une réponse perdue,
révoque la connexion et nécessite une nouvelle authentification. L'historique doit être
conservé au moins jusqu'à l'expiration définitive de la famille pour détecter les rejeux.
Le retour arrière de la migration reporte les révocations et consommations dans RevokedAt
avant de retirer les colonnes d'historique. Aucun token brut n'est sauvegardé.

42 vérifications SQL/HTTP ont couvert la migration avec anciennes données, les rotations
en chaîne, la réutilisation d'un ancien token, l'isolation entre connexions d'un même compte,
les renouvellements concurrents, les courses avec déconnexion, le rollback après erreur SQL,
l'expiration et le retour arrière/réapplication de la migration.
Le transport reste JSON ; le cookie HttpOnly reste à traiter séparément.