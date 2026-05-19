# docs/api

Spécification d'API publique du backend Vicinity.

## Fichiers

| Fichier | Rôle |
|---------|------|
| `openapi.yaml` | Source OpenAPI 3.0.3 (routes REST actuelles) |

## Swagger UI (développement)

Avec le backend lancé (`cd backend && npm run dev`) :

- **Interface interactive :** [http://localhost:3000/docs](http://localhost:3000/docs)
- **Spec brute :** [http://localhost:3000/openapi.yaml](http://localhost:3000/openapi.yaml)

### Tester une route protégée

1. `POST /auth/signup` ou `POST /auth/login` dans Swagger.
2. Copier `accessToken` de la réponse.
3. Bouton **Authorize** → `Bearer <accessToken>` (sans le préfixe `Bearer` dans certains clients ; Swagger ajoute le schéma HTTP Bearer automatiquement : coller uniquement le JWT).

## Synchronisation avec le code

La spec est maintenue **manuellement** à côté des routeurs Express
(`backend/src/http/routes/`). Lors de l’ajout d’une route, mettre à jour
`openapi.yaml`.

À envisager plus tard : `zod-to-openapi` pour générer les schémas depuis
les DTO Zod existants.

## Versionnement

Les routes sont aujourd’hui à la racine (`/auth/...`, `/neighbourhoods/...`).
Une future version pourrait être préfixée `/api/v1/...`.
