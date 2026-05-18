# desktop-client

**Épice 14** — Client lourd JavaFX offline-first pour les administrateurs.

## Stack envisagée

- **Java 21 LTS** + **JavaFX 21** (UI)
- **Gradle** (build) ou **Maven** — à arbitrer dans un ADR
- **H2** ou **SQLite (Xerial)** pour la base locale embarquée
- **Synchronisation différentielle** avec le backend via REST + jetons
  d'incrément (`since=<timestamp>` / curseurs)

## Périmètre fonctionnel

- Gestion des incidents / alertes signalés depuis le mobile/web
- Statistiques de participation par quartier
- Mode **offline-first** : lecture/écriture locale, synchro à la
  reconnexion, résolution des conflits côté serveur

## Authentification

SSO partagée avec les fronts web : le desktop ouvre un flow OIDC dans le
navigateur système (Authorization Code + PKCE), stocke les tokens dans le
keystore OS.

## Tests

TestFX pour les tests UI, JUnit 5 pour le reste.
