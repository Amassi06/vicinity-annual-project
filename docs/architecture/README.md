# docs/architecture

Documentation d'architecture **hors ADR**.

Les **décisions** (choix de techno, contrats stables) vont dans `docs/adr/`.
Ici on garde :

- **Diagrammes C4** (contexte / conteneurs / composants) — au format
  Mermaid ou Structurizr DSL pour rester versionnables.
- **Schémas de séquence** (auth OIDC, signature PDF, sync desktop, etc.).
- **Modèle de données consolidé** (mapping entre Postgres, Mongo, Neo4j).
- **Vue déploiement** (réseaux Docker, reverse proxy, brokers).

Convention de nommage : `NN-titre-court.md` (numérotation indicative,
réordonnable).
