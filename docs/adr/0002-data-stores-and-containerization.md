# 2. Bases de données et stratégie de conteneurisation DEV

- Statut : accepté
- Date : 2026-05-16

## Contexte

Le projet impose trois moteurs de persistance hétérogènes (PostgreSQL,
MongoDB, Neo4j) plus une base locale embarquée pour le client JavaFX.
L'environnement de développement doit être reproductible sans installation
locale de ces moteurs.

## Décision

- **PostgreSQL 16 + PostGIS 3.4** pour les données relationnelles et
  géographiques (polygones de quartier).
- **MongoDB 7.0** pour documents, événements, messages, annonces.
- **Neo4j 5.20 Community** + plugin **APOC** pour le graphe social.
- Conteneurisation dès DEV via `docker-compose.dev.yml`, réseau Docker
  dédié, volumes nommés, healthchecks.
- Variables sensibles via `.env.dev` (non versionné) ; `.env.dev.example`
  documente la structure.

## Conséquences

- Positives : onboarding `make up` en une commande, parité avec la prod.
- Négatives : empreinte mémoire ~ 1 Go ; heap Neo4j limitée à 512 Mo en DEV.

## Alternatives écartées

- Installation locale des moteurs (casse la reproductibilité).
- Postgres `pg_graph` au lieu de Neo4j (Neo4j est imposé par la spec).
