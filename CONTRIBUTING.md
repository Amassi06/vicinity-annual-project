# Contribuer à Vicinity

## Workflow

1. Branche depuis `main` : `feat/<scope>-<description>` (ou `fix/`, `chore/`...).
2. Code + tests pour la couche concernée.
3. Commits au format **Conventional Commits** :
   `feat(scope): ...`, `fix(scope): ...`, `chore(scope): ...`, `docs(scope): ...`,
   `refactor(scope): ...`, `test(scope): ...`, `build(scope): ...`, `ci(scope): ...`.
4. Ouvre une Pull Request vers `main`.

## Décisions d’architecture

Toute décision structurante est tracée dans `docs/adr/` (format MADR,
numérotation séquentielle immuable).

## Règles de base

- Pas de secret commité — utiliser `.env.example`.
- `.editorconfig` respecté par l'éditeur.
