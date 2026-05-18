# web-admin

**Épice 13** — Back-office modérateur/administrateur (React + TypeScript + Vite).

À initialiser le moment venu :

```bash
npm create vite@latest . -- --template react-ts
```

Fonctionnalités à porter : gestion des quartiers (carto PostGIS), modération
des annonces/messages, validation des comptes, supervision des votes,
gestion des consentements RGPD, exports et purges.

Authentification SSO OIDC + MFA obligatoire (Épice 4). UI orientée tableaux
et cartes (Leaflet/MapLibre pour la couche géo).
