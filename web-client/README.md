# web-client

**Épice 13** — Application web habitant (React + TypeScript + Vite).

À initialiser le moment venu :

```bash
npm create vite@latest . -- --template react-ts
```

Fonctionnalités à porter : feed quartier, annonces/services, événements
(swipe), messagerie multimédia, signatures de documents, votes, profil.

Authentification via SSO OIDC (Épice 4), API consommée depuis le backend
Node.js. Routing avec React Router. State global à arbitrer (Zustand /
Redux Toolkit) au moment du scaffolding.
