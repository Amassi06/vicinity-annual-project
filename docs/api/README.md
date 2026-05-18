# docs/api

Spécification d'API publique du backend.

## Format

OpenAPI 3.1, fichier source unique `openapi.yaml` (ou éclaté par domaine
puis assemblé via `$ref`).

## Génération

- À envisager : génération depuis le code (`zod-to-openapi`, `express-zod-api`)
  pour garder la spec en phase avec les schémas Zod déjà utilisés côté
  backend (`backend/src/config/env.ts` et futurs DTO).
- Rendu HTML statique avec Redoc ou Swagger UI (servi sous `/docs` ou via
  GitHub Pages).

## Versionnement

Convention sémantique sur l'URL : `/api/v1/...`. Breaking changes → bump
majeur + maintien temporaire de l'ancienne version.
