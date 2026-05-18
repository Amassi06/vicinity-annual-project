# 3. Prisma comme ORM Postgres et stratégie PostGIS

- Statut : accepté
- Date : 2026-05-16

## Contexte

Le backend Node.js doit accéder à PostgreSQL pour les données relationnelles
(utilisateurs, rôles, sessions, audit RGPD) et aux polygones de quartier
en PostGIS. Il faut un ORM type-safe avec migrations reproductibles.

## Décision

- **Prisma 5** comme ORM + outil de migration. Client typé généré,
  migrations versionnées dans `backend/prisma/migrations/`.
- Les colonnes PostGIS (`geometry`) sont déclarées
  `Unsupported("geometry(Polygon, 4326)")` côté `schema.prisma`. Prisma
  préserve la colonne sans l'exposer en accès typé.
- Les **requêtes spatiales** se font via `prisma.$queryRaw` avec du SQL
  PostGIS brut, encapsulées dans des repositories dédiés.
- Génération des migrations CI-friendly : `prisma migrate diff
  --from-empty --to-schema-datamodel ... --script` puis
  `prisma migrate deploy` (pas de `migrate dev` qui exige un TTY).

## Conséquences

- Positives : client Prisma 100 % typé sur 90 % du domaine, migrations
  reviewables (SQL plain), hot-swap possible vers SQL brut sur hotspots.
- Négatives : les colonnes PostGIS imposent SQL brut → discipline de
  centraliser ces accès dans des repos pour rester testable.

## Alternatives écartées

- **TypeORM** : moins type-safe, migrations moins fiables.
- **Kysely** : excellent mais oblige à coder le schéma à la main ; sera
  utilisé ponctuellement pour les requêtes PostGIS complexes si besoin.
- **Drizzle** : moins mature sur PostgreSQL + extensions.
