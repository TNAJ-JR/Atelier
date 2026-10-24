# Cookies, CSRF et attaques web : explications

## Fonctionnement mis en place
1. Le frontend demande GET /api/auth/csrf avec credentials: include.
2. ASP.NET dépose un cookie antiforgery HttpOnly/Secure et renvoie un jeton de requête dans le JSON.
3. Le frontend garde uniquement ce jeton CSRF en mémoire et l'envoie dans X-CSRF-TOKEN.
4. login, refresh et logout exigent cette paire valide, vérifient Origin lorsqu'il est présent et exigent HTTPS.
5. login/refresh déposent le refresh token dans __Host-atelier-refresh (HttpOnly, Secure, Path=/, sans Domain).
6. Le refresh token n'est jamais sérialisé en JSON. Le JWT reste en mémoire et est envoyé par Authorization.
7. Au rechargement de page, refresh restaure la session avec le cookie ; logout révoque sa famille et efface le cookie.

Le cookie antiforgery et le cookie refresh sont deux cookies distincts. Le jeton CSRF de
requête n'est pas le refresh token et ne permet pas, seul, de se connecter.
Les routes de cycle de vie des cookies n'utilisent pas le bearer JWT pour authentifier la requête :
cela garde une identité anonyme stable pour la paire antiforgery. Leur contrôle repose sur CSRF,
l'origine et, pour refresh/logout, le refresh cookie. /me et les routes métier gardent JWT.
Les routes métier utilisant uniquement Authorization ne sont pas authentifiées implicitement par cookie.

Le frontend envoie credentials: include ; CORS autorise les credentials uniquement pour les
origines explicites. Web Locks sérialise login/refresh/logout entre onglets du même frontend
sur les navigateurs compatibles ; une promesse unique mutualise les refresh dans un onglet.
Ne pas tenter automatiquement une nouvelle rotation après une erreur réseau ambiguë.
Seul un refus csrf_invalid, émis avant l'action, permet une nouvelle tentative CSRF.
La déconnexion est signalée aux autres onglets par BroadcastChannel.

## Configuration et déploiement
Aucun changement de schéma ou de clé de signature n'est requis.
AuthCookies:SameSite vaut None par défaut en Development, car le frontend actuel est
http://localhost:5173 et l'API https://localhost:7262 : leurs schémas différents en font
des sites distincts pour SameSite. Secure reste obligatoire.
En dehors de Development, la valeur par défaut est Strict.
Pour un déploiement réellement cross-site, configurer explicitement None, HTTPS et
Cors:AllowedOrigins. Les restrictions des navigateurs sur les cookies tiers peuvent
alors intervenir ; privilégier frontend et API sur le même site en production.
Avec un proxy qui termine TLS, configurer les en-têtes transférés et les proxys approuvés
dans l'hébergement afin que l'API identifie correctement HTTPS. Ne pas faire confiance
indistinctement à tous les proxys.
Persister les clés ASP.NET Data Protection et les partager entre instances de la même application.

## XSS : du code malveillant exécuté dans le bon site
L'attaquant introduit un contenu qui devient du JavaScript exécuté dans l'origine du site.
- Stockée : le contenu est conservé, par exemple dans un commentaire, puis montré aux visiteurs.
- Réfléchie : une entrée d'URL est renvoyée dans la page sans échappement.
- DOM : le frontend transforme lui-même une entrée non fiable en HTML ou en code.

Exemple : un nom de produit doit rester du texte ; si l'application le traite comme du
HTML exécutable, une personne malveillante peut agir dans la page des autres utilisateurs.

Corrections : laisser React échapper les textes ; éviter innerHTML, eval et
dangerouslySetInnerHTML. Si du HTML riche est nécessaire, utiliser un assainisseur
adapté. Contrôler aussi les URLs non fiables et maintenir les dépendances.
Une CSP adaptée peut apporter une défense supplémentaire ; elle n'a pas été ajoutée ici.
HttpOnly empêche la lecture directe du refresh cookie. Retirer le JWT de localStorage
évite sa persistance. Cela ne neutralise pas une XSS : le script peut encore effectuer
des actions dans la session et lire les données accessibles à la page.

## CSRF : une action demandée par le mauvais site
La victime a déjà un cookie pour l'API. Un site malveillant tente de lui faire envoyer
une requête, par exemple via un formulaire caché. Le navigateur peut joindre les cookies
automatiquement. L'attaquant n'a pas besoin de lire le cookie ni la réponse pour provoquer l'action.
Une variante, login CSRF, connecte la victime au compte de l'attaquant : elle peut ensuite
y saisir des informations que l'attaquant retrouvera.

Corrections mises en place : jeton antiforgery ASP.NET et cookie associé, vérification sur
login/refresh/logout, en-tête personnalisé X-CSRF-TOKEN, contrôle de l'origine, SameSite
et HTTPS. Le site adverse ne peut pas lire la réponse CSRF avec la politique CORS actuelle.
Le jeton doit être présent et lié au bon cookie : une simple chaîne arbitraire ne suffit pas.
HttpOnly ne protège pas du CSRF ; il limite la lecture JavaScript. SameSite seul ne suffit pas
dans toutes les architectures. Une XSS peut contourner les défenses CSRF en agissant depuis le site.

## CORS : une règle du navigateur, pas une authentification
CORS n'est pas une attaque. C'est la règle qui dit quelles origines JavaScript peuvent lire
les réponses de l'API. Une mauvaise configuration peut autoriser un site hostile à lire
des réponses avec les cookies de la victime.
Le correctif est une liste d'origines précises avec AllowCredentials, jamais une autorisation
générale. Un client hors navigateur n'est pas bloqué par CORS ; il doit toujours s'authentifier.
Le contrôle Origin serveur complète CORS pour refuser l'exécution des actions cookie depuis un site non autorisé.

## Force brute et credential stuffing
Force brute : essayer beaucoup de mots de passe pour un compte.
Credential stuffing : réutiliser des couples email/mot de passe provenant d'une autre fuite.
Identity verrouille après cinq échecs pour cinq minutes. Le rate limiter limite les appels
par IP, mais plusieurs machines peuvent répartir l'attaque. Des mots de passe uniques,
la MFA et des protections d'infrastructure complètent ces mesures ; la MFA n'est pas implémentée ici.
Derrière un proxy, la bonne configuration de l'IP cliente est nécessaire à une limitation utile.

## Énumération des comptes
Un message « email inconnu » et un autre « mauvais mot de passe » révèlent les adresses inscrites.
Les temps de réponse peuvent également donner un indice.
Le backend garde un statut/message public générique et effectue une vérification de hash
factice pour un compte inconnu. Cela réduit les indices, sans garantir des temps identiques.

## Vol et rejeu de jetons
Vol : récupérer un secret de session. Rejeu : présenter ce secret comme si l'on était son propriétaire.
La signature du JWT empêche d'en modifier librement les rôles, pas de réutiliser un JWT volé intact.
Les refresh tokens sont stockés sous forme de hash en SQL, transportés en cookie HttpOnly/Secure
et consommés une seule fois. Leur historique permet de détecter un ancien jeton présenté à nouveau.
Ce rejeu révoque la famille et ses JWT ; les autres connexions restent indépendantes.
La rotation ne supprime pas toutes les possibilités de vol : HTTPS et la prévention XSS restent nécessaires.

## Injection SQL
L'attaquant essaie de faire interpréter une entrée comme une partie de la commande SQL.
Les requêtes LINQ d'EF et FromSqlInterpolated utilisent des paramètres : une valeur
reste une valeur, pas du code SQL. Ne pas concaténer une entrée utilisateur dans une requête.
Cela ne remplace pas la vérification des droits sur les données demandées.

## Interception réseau et clickjacking
Une interception réseau peut lire ou modifier les échanges si le transport n'est pas protégé.
HTTPS protège le trajet et Secure interdit l'envoi du cookie par HTTP ; HttpOnly n'est pas du chiffrement.
Le clickjacking place une page dans un cadre trompeur pour faire cliquer la victime sur une action.
Il se traite notamment avec CSP frame-ancestors ou X-Frame-Options ; ces en-têtes ne sont pas ajoutés par cette modification.

## Vérifications réalisées
Build backend, build TypeScript/Vite et lint frontend réussis.
32 vérifications HTTPS sur une base SQL temporaire ont couvert les attributs de cookie,
l'absence du refresh token dans le JSON, CSRF absent/faux/d'un autre client, origine hostile,
rotation, rejeu, suppression de cookie, logout sans JWT et CORS avec credentials.
Dans le navigateur : connexion, restauration après rechargement, deuxième onglet
et déconnexion propagée aux deux onglets. Les services et bases de test ont été supprimés.
Aucun script PowerShell de test n'est conservé dans le projet.

Références :
- https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
