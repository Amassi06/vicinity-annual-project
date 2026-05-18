# Vicinity

> **"Voisins, services et bonne humeur."**
> Plateforme collaborative sécurisée et extensible de quartiers : services entre
> voisins, documents signés, événements, messagerie multimédia, votes.

Ce dépôt est un **monorepo** regroupant le backend, deux fronts React (utilisateur
et back-office), un client lourd JavaFX offline-first, un DSL maison
d'interrogation MongoDB et toute l'infrastructure (Docker, CI).

---

## Table des matières

1. [Vision produit](#1-vision-produit)
2. [Architecture cible](#2-architecture-cible)
3. [Avancement actuel](#3-avancement-actuel)
4. [Structure du dépôt](#4-structure-du-dépôt)
5. [Mise en route (DEV)](#5-mise-en-route-dev)
6. [Étape 1 — Conteneurisation des bases de données](#étape-1--conteneurisation-des-bases-de-données)
7. [Étape 2 — Squelette du backend Node.js + Express](#étape-2--squelette-du-backend-nodejs--express)
8. [Étape 3 — Modèles de données et persistance](#étape-3--modèles-de-données-et-persistance)
9. [Conventions](#9-conventions)
10. [Vérifications et qualité](#10-vérifications-et-qualité)
11. [Prochaines étapes](#11-prochaines-étapes)

---

## 1. Vision produit

Vicinity est une plateforme orientée **quartiers**. Un administrateur
définit géographiquement un quartier, et les habitants peuvent ensuite :

- **Échanger des services** (gratuits ou payants via un système de points), avec
  contrat automatique pour tout service payant.
- **Signer numériquement des documents PDF** (zones de signature/initiales,
  archivage, vérification d'intégrité).
- **Organiser et participer à des événements** communautaires, avec interface
  de "swipe" pour exprimer l'intérêt.
- **Discuter en messagerie multimédia** (texte, photos, vocaux, présence
  temps réel ; appels vidéo en bonus).
- **Voter** sur des sujets du quartier via un système paramétrable.

Un **client Java Desktop (JavaFX)** complète l'application web, dédié aux
**administrateurs** : gestion des incidents/alertes signalés, statistiques de
participation, **fonctionnement offline-first** avec base locale embarquée
(SQLite/H2) et synchronisation différentielle.

### Contraintes techniques imposées

| Aspect              | Choix                                                                 |
|---------------------|-----------------------------------------------------------------------|
| Backend             | **Node.js + Express**                                                 |
| Frontends web       | **React** (client + back-office)                                      |
| Client desktop      | **Java + JavaFX**, base locale **H2/SQLite**                          |
| Stockage relationnel & géo | **PostgreSQL + PostGIS**                                       |
| Documents & messages | **MongoDB**                                                          |
| Graphe social & reco | **Neo4j**                                                            |
| Sécurité            | **SSO web ↔ desktop**, **MFA obligatoire** sur actions sensibles      |
| Conformité          | **RGPD** (droit d'accès, rectification, suppression, export)          |
| DSL                 | Langage maison d'interrogation MongoDB via **lex/yacc**               |
| Livraison           | Tout **conteneurisé**, environnements séparés                         |

---

## 2. Architecture cible

```
┌──────────────┐   ┌──────────────┐    ┌────────────────┐
│  Web Client  │   │  Web Admin   │    │ Desktop JavaFX │
│   (React)    │   │   (React)    │    │ offline-first  │
└──────┬───────┘   └──────┬───────┘    └────────┬───────┘
       │                  │                     │
       └──────── HTTPS / WSS / OIDC SSO ────────┘
                          │
                  ┌───────▼────────┐
                  │  Backend API   │   Express + TypeScript
                  │  Node.js 20    │   (hexagonal, factory createApp)
                  └───────┬────────┘
       ┌──────────────────┼───────────────────────┐
       │                  │                       │
  ┌────▼─────┐       ┌────▼─────┐            ┌────▼────┐
  │ Postgres │       │ MongoDB  │            │  Neo4j  │
  │ +PostGIS │       │ docs/msg │            │ social  │
  └──────────┘       └──────────┘            └─────────┘
   relationnel        documents,              graphe
   + spatial          messages,               social et
   (Prisma)           annonces                recommandations
                      (Mongoose)              (driver Bolt)
```

L'architecture cible prévoit également : un objet store (PDF) type MinIO/S3,
un reverse-proxy (Traefik), un clustering Socket.IO avec broker (Redis pour
le scaling), et un provider OIDC pour SSO + MFA. Ces briques seront montées/en
durcies en Épice 15. **Socket.IO** et un DSL `/dsl/compile` sont déjà embarqués
pour les flux DEV des épiques 10 et 12.

---

## 3. Avancement actuel

Le projet est piloté en **Épices** (lots fonctionnels) déclinées en **tâches
atomiques** (un commit Conventional par tâche).

| Épice                                              | Statut |
|----------------------------------------------------|:------:|
| **0** — Gouvernance du dépôt (ADR, templates GH)                    | ✅ |
| **1** — Conteneurisation DEV (Postgres+PostGIS, Mongo, Neo4j)       | ✅ |
| **2** — Squelette backend Node.js + Express + TypeScript            | ✅ |
| **3** — Modèles de données et persistance (3 bases)                 | ✅ |
| 4 — Authentification, SSO & MFA                                     | ✅ |
| 5 — Conformité RGPD                                                 | ⏳ |
| 6 — Modélisation géographique du quartier                           | ✅ |
| 7 — Annonces, services et système de points                         | ✅ |
| 8 — Documents PDF & signatures                                      | ✅ |
| 9 — Événements + recommandations Neo4j                              | ✅ |
| 10 — Messagerie multimédia + présence temps réel                    | ✅ |
| 11 — Votes + système de plugins                                     | ✅ |
| 12 — DSL maison MongoDB (lex/yacc)                                  | ✅ |
| 13 — Frontends React (client + admin)                               | ✅ |
| 14 — Client Desktop JavaFX                                          | ✅ |
| 15 — Observabilité, sécurité avancée, livraison                     | ⏳ |

### Historique git (extrait récent sur `main`)

```
9cab932 feat(desktop): javafx vicinity admin skeleton on gradle kotlin
4e775ac feat(web): vite react scaffolds for vicinity client and admin
a71fc47 feat(dsl): mongo FIND dialect compile endpoint plus lex yacc reference
4740100 feat(backend): neighbourhood polls votes and builtin plugin bootstrap
84cce68 feat(backend): add messaging REST and socket.io presence hints
45792b9 feat(backend): add events API and neo4j-backed recommendations
171bb9c feat(documents): add pdf upload signature zones and mfa-protected signing
23bd2b9 feat(listings): expose crud and accept flow with contract creation
1b7c5bd feat(wallet): add points wallet with transactional transfers
2fd0785 feat(points): add PointTransaction model and PointTxReason enum for transaction tracking
e65f9ec feat(neighbourhood): add point lookup and overlap detection
9c90421 feat(neighbourhood): add crud with postgis polygon validation
```

---

## 4. Structure du dépôt

```
.
├── backend/                  # API Node.js + Express + TypeScript (Épices 2-11)
│   ├── prisma/               # Schéma + migrations Postgres
│   ├── src/
│   │   ├── config/           # env.ts — variables typées via Zod
│   │   ├── logger/           # pino + redaction des champs sensibles
│   │   ├── db/               # prisma + mongo (mongoose) + neo4j (driver Bolt)
│   │   ├── http/             # app.ts (factory Express) + routes/
│   │   └── server.ts         # entrypoint + lifecycle signaux
│   ├── tests/                # Jest + Supertest
│   └── (tsconfig, eslint, jest, package.json)
│
├── web-client/               # React utilisateur (Épice 13)
├── web-admin/                # React back-office (Épice 13)
├── desktop-client/           # JavaFX offline-first (Épice 14)
├── lex-yacc/                 # DSL Mongo via lex/yacc (Épice 12)
│
├── infra/
│   ├── docker/
│   │   ├── docker-compose.dev.yml
│   │   ├── .env.dev.example
│   │   └── postgres/init/01_extensions.sql
│   └── ci/                   # Workflows CI (à venir)
│
├── docs/
│   ├── adr/                  # Architecture Decision Records (MADR)
│   ├── api/                  # OpenAPI / Swagger (à venir)
│   └── architecture/         # Diagrammes C4, séquence, vue déploiement
│
├── scripts/                  # Scripts dev (seed, dump, smoke...)
├── .github/                  # CODEOWNERS + templates issues/PR
├── Makefile                  # wrappers Docker Compose
└── README.md                 # ce fichier
```

> Les dossiers **`web-client/`, `web-admin/`, `lex-yacc/`, `desktop-client/`**
> contiennent des squelettes documentés avec leur README local (`npm`, Gradle…).

---

## 5. Mise en route (DEV)

### Prérequis

- **Docker Engine ≥ 24** + **Docker Compose v2**
- **Node.js ≥ 20.11** + **npm ≥ 10** (pour le backend)
- **GNU Make**
- Ports libres sur l'hôte : `55432` (Postgres), `57017` (Mongo), `57474`/`57687` (Neo4j)

> Les ports sont décalés (`5XXXX`) pour ne pas entrer en conflit avec des
> instances locales existantes.

### Bootstrap complet

```bash
# 1) Cloner
git clone <repo-url> vicinity
cd vicinity

# 2) Démarrer les bases de données
make up                           # crée .env.dev + docker compose up -d
make ps                           # 3 services doivent être "(healthy)"

# 3) Préparer le backend
cd backend
npm install
cp .env.example .env
npm run db:migrate                # applique les migrations Prisma
npm run db:generate               # génère @prisma/client typé

# 4) Lancer les vérifications
npm run lint                      # ESLint type-aware strict
npm test                          # Jest (--runInBand ; intégration si Docker UP)
npm run build                     # compile vers dist/

# 5) Backend en boucle DEV
npm run dev                       # http://localhost:3000
```

### Faces web Vicinity (Vite + React)

Deux terminaux depuis la **racine** du repo :

```bash
cd web-client && npm install && npm run dev   # proxifie /api → :3000
```

```bash
cd web-admin && npm install && npm run dev    # même proxy, port 5174 affiché par Vite
```

### Commandes Make

| Commande     | Effet                                                    |
|--------------|----------------------------------------------------------|
| `make help`  | Liste les cibles disponibles                             |
| `make env`   | Crée `infra/docker/.env.dev` depuis l'exemple si absent  |
| `make up`    | Démarre la stack DEV (détaché)                           |
| `make ps`    | Liste les services et leur état                          |
| `make logs`  | Suit les logs (Ctrl+C pour sortir)                       |
| `make down`  | Arrête la stack (volumes conservés)                      |

> Pour des accès interactifs aux bases :
> `docker compose --env-file infra/docker/.env.dev -f infra/docker/docker-compose.dev.yml exec postgres psql -U cn_app -d connected_neighbours`
> (idem avec `mongo mongosh ...` ou `neo4j cypher-shell ...`).

---

## Étape 1 — Conteneurisation des bases de données

### Objectif

Mettre à disposition un environnement reproductible avec les trois moteurs de
persistance imposés par la spec, **sans rien installer en local** sur le poste
développeur.

### Ce qui a été livré

- **`infra/docker/docker-compose.dev.yml`** : 3 services sur un réseau Docker
  dédié `cn_backend`, volumes nommés persistants, healthchecks Docker.
- **`infra/docker/.env.dev.example`** : variables d'environnement (mots de
  passe DEV, ports décalés). Le fichier `.env.dev` réel n'est pas versionné.
- **`infra/docker/postgres/init/01_extensions.sql`** : extensions Postgres
  activées au tout premier démarrage (`postgis`, `uuid-ossp`, `pgcrypto`,
  `citext`).
- **`Makefile`** racine : enveloppe minimaliste de `docker compose --env-file ... -f ...`
  (`up`, `down`, `ps`, `logs`) pour ne pas avoir à mémoriser les chemins.

### Détail des services

| Service     | Image                       | Port hôte | Particularités                                |
|-------------|-----------------------------|-----------|-----------------------------------------------|
| `cn-postgres` | `postgis/postgis:16-3.4`  | `55432`   | PostGIS activé via SQL d'init                 |
| `cn-mongo`    | `mongo:7.0`               | `57017`   | Auth root activée                             |
| `cn-neo4j`    | `neo4j:5.20-community`    | `57474` (HTTP), `57687` (Bolt) | Plugin **APOC** chargé      |

### Vérifications

```bash
make up && sleep 30 && make ps
# Doit afficher 3 lignes avec "Up X seconds (healthy)"

# Smoke tests rapides :
docker compose --env-file infra/docker/.env.dev \
  -f infra/docker/docker-compose.dev.yml \
  exec postgres psql -U cn_app -d connected_neighbours -c "SELECT PostGIS_Version();"
# → 3.4 USE_GEOS=1 USE_PROJ=1 USE_STATS=1

docker compose --env-file infra/docker/.env.dev \
  -f infra/docker/docker-compose.dev.yml \
  exec neo4j cypher-shell -u neo4j -p cn_dev_password \
  "RETURN apoc.version() AS v;"
# → "5.20.0"
```

### Décisions tracées

Voir [`docs/adr/0002-data-stores-and-containerization.md`](docs/adr/0002-data-stores-and-containerization.md) :
choix des versions, justification de PostGIS dès le DEV, limitation du heap
Neo4j à 512 Mo pour rester raisonnable en empreinte mémoire.

### Sécurité (DEV uniquement)

Les mots de passe `cn_dev_password` et `cn_root` sont **explicitement** des
secrets de développement. Ils ne doivent **jamais** être réutilisés en
staging/prod. La rotation passera par un secret manager (à définir Épice 15).

---

## Étape 2 — Squelette du backend Node.js + Express

### Objectif

Poser une base backend **TypeScript strict**, testable, observable, prête à
accueillir le métier sans avoir à recâbler l'outillage à chaque ajout de
feature.

### Choix techniques

| Choix                  | Pourquoi                                                                                  |
|------------------------|-------------------------------------------------------------------------------------------|
| **TypeScript strict**  | `strict + noUncheckedIndexedAccess + exactOptionalPropertyTypes` ; bugs attrapés au build |
| **ESLint flat config + type-checked** | Lint sémantique (`no-unsafe-call`, etc.) sur tout le code TS                |
| **Express 4**          | Imposé par la spec, large écosystème                                                      |
| **pino**               | Logger structuré JSON, ultra-rapide, redaction native des champs sensibles                |
| **zod**                | Validation typée des variables d'environnement (échec fast au démarrage)                  |
| **Jest + ts-jest + Supertest** | Tests unitaires/intégration ; Supertest attaque l'`Express` sans bind réseau      |
| **Factory `createApp()`** | Sépare l'initialisation Express du `listen()` → tests E2E sans port             |
| **`tsx watch`**        | Boucle de dev rapide sans build intermédiaire                                             |
| **NodeNext + ESM imports en `.js`** | Aligné sur le format moderne ; `moduleNameMapper` côté Jest pour la résolution |

### Structure du backend

```
backend/src/
├── config/env.ts         # parse process.env via Zod → objet typé immuable
├── logger/index.ts       # pino + transport pretty en DEV
├── db/                   # voir Étape 3
├── http/
│   ├── app.ts            # createApp(): assemble middlewares + routeurs
│   └── routes/
│       └── health.ts     # GET /healthz et GET /readyz
└── server.ts             # bootstrap : connexions DB, listen, shutdown gracieux
```

### Endpoints livrés

| Méthode | Chemin     | Description                                                |
|---------|------------|------------------------------------------------------------|
| `GET`   | `/healthz` | **Liveness** — 200 immédiat, aucune dépendance externe     |
| `GET`   | `/readyz`  | **Readiness** — 200 si Postgres + Mongo + Neo4j sont OK, sinon 503 |

### Cycle de vie du serveur

`server.ts` :
1. `prisma.$connect()` (Postgres)
2. `connectMongo()` (Mongo)
3. `getNeo4jDriver()` + `applyNeo4jConstraints()` (Neo4j)
4. `createApp()` puis `app.listen(PORT)`
5. Handlers `SIGINT` / `SIGTERM` → `server.close()` → ferme proprement toutes
   les connexions DB → `process.exit(0)`. Watchdog 10 s pour forcer la sortie.

### Scripts npm

```jsonc
{
  "dev":           "tsx watch src/server.ts",     // boucle de dev
  "build":         "tsc -p tsconfig.build.json",  // → dist/
  "start":         "node dist/server.js",         // exécution prod
  "test":          "jest --runInBand",            // suites séries (DB intégration)
  "lint":          "eslint \"src/**/*.ts\" \"tests/**/*.ts\"",
  "format":        "prettier --write ...",
  "db:migrate":    "prisma migrate deploy",       // applique migrations
  "db:generate":   "prisma generate",             // régénère @prisma/client
  "db:studio":     "prisma studio"                // GUI Prisma
}
```

### Vérifications

```bash
cd backend
npm run lint       # 0 erreur
npm test           # 7 tests verts (4 suites)
npm run build      # dist/ généré
PORT=3000 npm start &
sleep 3
curl http://localhost:3000/healthz   # → {"status":"ok","uptime":...}
curl http://localhost:3000/readyz    # → {"status":"ready","checks":{...:true,...}}
```

---

## Étape 3 — Modèles de données et persistance

L'épice est découpée en quatre tâches atomiques (un commit par tâche) :

| # | Domaine                | Commit                                                                                |
|---|------------------------|---------------------------------------------------------------------------------------|
| 3.1 | Postgres via Prisma  | `feat(backend): integrate prisma with initial schema and postgres connection`         |
| 3.2 | MongoDB via Mongoose | `feat(backend): integrate mongoose models for documents events messages and listings` |
| 3.3 | Neo4j via driver Bolt | `feat(backend): integrate neo4j driver with social graph constraints`                 |
| 3.4 | `/readyz` agrégé      | `feat(backend): expose readyz aggregating database health checks`                     |

### 3.1 Postgres + Prisma + PostGIS

**Schéma** (`backend/prisma/schema.prisma`) :

| Modèle          | Rôle                                                                       |
|-----------------|----------------------------------------------------------------------------|
| `User`          | Comptes habitants/modérateurs/admins (rôle, statut, MFA, points)           |
| `Session`       | Sessions actives (hash du refresh token, IP, user-agent, expiration)       |
| `Neighbourhood` | Quartiers — colonne `boundary geometry(Polygon, 4326)` PostGIS             |
| `AuditLog`      | Journal RGPD (login, export, suppression, signature, consentement, ...)    |

Enums : `UserRole` (`HABITANT`/`MODERATOR`/`ADMIN`), `UserStatus`
(`ACTIVE`/`PENDING`/`SUSPENDED`/`DELETED`), `AuditAction`.

**Conventions** :
- PK en `UUID` générés par `gen_random_uuid()` (extension `pgcrypto`).
- Timestamps en `TIMESTAMPTZ` (UTC).
- E-mails en `CITEXT` pour comparaison case-insensitive.
- Soft delete via `deleted_at`.

**PostGIS** : Prisma ne supporte pas nativement le type `geometry`. La
colonne `boundary` est déclarée `Unsupported("geometry(Polygon, 4326)")`,
ce qui préserve la colonne sans l'exposer en accès typé. Les requêtes
spatiales utiliseront `prisma.$queryRaw` avec du SQL PostGIS dans les
repositories dédiés (Épice 6). Décision tracée dans
[`docs/adr/0003-prisma-and-postgis-strategy.md`](docs/adr/0003-prisma-and-postgis-strategy.md).

**Migration générée** (`prisma/migrations/20260516000000_init/migration.sql`,
111 lignes) : extensions, enums, 4 tables, foreign keys, index, contraintes
d'unicité.

**Pourquoi `prisma migrate diff --script` plutôt que `migrate dev`** : la commande
`migrate dev` exige un TTY interactif, incompatible avec les pipelines CI et
les sessions agent. On génère le SQL avec `migrate diff` puis on l'applique
avec `migrate deploy` qui, lui, est non-interactif et idempotent.

### 3.2 MongoDB + Mongoose

**Connexion** (`src/db/mongo/connection.ts`) : promise singleton, écoute des
événements `connected`/`error`/`disconnected`, pool de 20 connexions, timeout
de sélection de serveur à 5 s pour fail-fast.

**Modèles** (`src/db/mongo/models/`) :

| Modèle          | Fichier              | Champs clés                                                                    |
|-----------------|----------------------|--------------------------------------------------------------------------------|
| `DocumentModel` | `document.model.ts`  | `ownerId`, `storageKey` (unique), `sha256`, `zones[]` (signatures), `status`   |
| `EventModel`    | `event.model.ts`     | `organizerId`, `neighbourhoodId`, `startsAt/endsAt`, `location` (2dsphere)     |
| `MessageModel`  | `message.model.ts`   | `conversationId`, `senderId`, `body`, `attachments[]`, `readBy`, `deliveredTo` |
| `ListingModel`  | `listing.model.ts`   | `authorId`, `kind` (offer/request), `pricePoints`, `isFree`, `status`          |

**Index notables** : `2dsphere` sur `events.location` (recherche géo),
`{conversationId: 1, createdAt: -1}` sur `messages` (timeline conversation),
`{neighbourhoodId, status, createdAt}` sur `listings` (feed quartier).

### 3.3 Neo4j + driver officiel

**Driver** (`src/db/neo4j/driver.ts`) : singleton, pool 50 connexions, timeout
d'acquisition 10 s.

**Modèle de graphe** (documenté dans `src/db/neo4j/constraints.ts`) :

```
Nodes
  (:User       {id, neighbourhoodId})
  (:Event      {id, neighbourhoodId, startsAt})
  (:Listing    {id, neighbourhoodId})
  (:Neighbourhood {id})

Relations (orientation = sens de l'action)
  (:User)-[:HELPED         {at}]->(:User)
  (:User)-[:PARTICIPATED   {at}]->(:Event)
  (:User)-[:INTERESTED_IN  ]->(:Event)
  (:User)-[:TRUSTS         {score}]->(:User)
  (:User)-[:LIVES_IN       ]->(:Neighbourhood)
```

Ce graphe alimentera le moteur de recommandations Cypher en Épice 9
(voisins fiables, événements pertinents, services compatibles).

**Contraintes appliquées au boot** (4 unicités + 3 index) :

```cypher
CREATE CONSTRAINT user_id_unique           IF NOT EXISTS FOR (u:User)          REQUIRE u.id IS UNIQUE;
CREATE CONSTRAINT event_id_unique          IF NOT EXISTS FOR (e:Event)         REQUIRE e.id IS UNIQUE;
CREATE CONSTRAINT listing_id_unique        IF NOT EXISTS FOR (l:Listing)       REQUIRE l.id IS UNIQUE;
CREATE CONSTRAINT neighbourhood_id_unique  IF NOT EXISTS FOR (n:Neighbourhood) REQUIRE n.id IS UNIQUE;
CREATE INDEX user_neighbourhood_idx        IF NOT EXISTS FOR (u:User)  ON (u.neighbourhoodId);
CREATE INDEX event_neighbourhood_idx       IF NOT EXISTS FOR (e:Event) ON (e.neighbourhoodId);
CREATE INDEX event_starts_at_idx           IF NOT EXISTS FOR (e:Event) ON (e.startsAt);
```

Toutes les instructions utilisent `IF NOT EXISTS` → **idempotentes**,
ré-appliquées à chaque démarrage du backend sans effet de bord.

### 3.4 Healthcheck applicatif `/readyz`

Endpoint qui agrège les trois sources de persistance :

- `checkPostgresHealth()` → `prisma.$queryRaw\`SELECT 1\``
- `checkMongoHealth()` → `mongoose.connection.db.admin().ping()`
- `checkNeo4jHealth()` → `driver.verifyConnectivity()`

```http
GET /readyz
  → 200 OK  {"status":"ready",    "checks":{"postgres":true, "mongo":true, "neo4j":true}}
  → 503     {"status":"degraded", "checks":{"postgres":true, "mongo":false,"neo4j":true}}
```

Ce contrat respecte les conventions Kubernetes (`/healthz` = liveness,
`/readyz` = readiness) et permet à un orchestrateur de retirer l'instance du
load-balancer si **une seule** des trois DB est indisponible.

### Tests d'intégration

| Suite                                  | Tests | Stack requise   |
|----------------------------------------|:-----:|-----------------|
| `tests/health.test.ts`                 | 1     | Aucune          |
| `tests/mongo.integration.test.ts`      | 2     | `make up`       |
| `tests/neo4j.integration.test.ts`      | 3     | `make up`       |
| `tests/readyz.integration.test.ts`     | 1     | `make up`       |
| **Total**                              | **7** | —               |

Lancement : `npm test` (depuis `backend/`). Toutes les suites passent en série
(`--runInBand`) pour éviter les contentions sur la même DB de test.

---

## 9. Conventions

### Commits — Conventional Commits

Format recommandé : `<type>(<scope>): <sujet>`.

Types : `feat`, `fix`, `chore`, `docs`, `refactor`, `test`, `build`, `ci`,
`perf`, `revert`, `style`. **Scope** en kebab-case (ex. `backend`, `infra`,
`repo`, `auth`).

Exemples :
- `feat(backend): add mfa verification middleware`
- `fix(infra): pin neo4j heap to 512m for dev parity`
- `docs(adr): record decision to use prisma over kysely`

### Branches

- `main` (protégée — toutes les modifs passent par PR)
- `feat/<scope>-<description>`, `fix/<scope>-<description>`, `chore/<scope>-<description>`

### ADR (Architecture Decision Records)

Toute décision structurante (techno, pattern, contrat d'API public) est tracée
dans `docs/adr/` au format MADR. Numérotation séquentielle **immuable** : une
décision révisée fait l'objet d'un nouvel ADR qui supersede l'ancien.

À ce jour : ADR 0001 (monorepo), ADR 0002 (data stores), ADR 0003 (Prisma+PostGIS).

---

## 10. Vérifications et qualité

État actuel **vérifié sur la machine de développement** :

```text
✅ docker compose ps    → cn-postgres, cn-mongo, cn-neo4j tous "(healthy)"
✅ npm run lint         → 0 erreur, 0 warning
✅ npm test             → 7 tests verts en 4 suites (1.6 s)
✅ npm run build        → tsc OK, dist/ généré
✅ curl /healthz        → 200 {"status":"ok","uptime":4.007}
✅ curl /readyz         → 200 {"status":"ready","checks":{"postgres":true,"mongo":true,"neo4j":true}}
✅ psql \dt             → users, sessions, neighbourhoods, audit_logs, spatial_ref_sys, _prisma_migrations
✅ SHOW CONSTRAINTS     → 4 contraintes Neo4j enregistrées
```

### Stratégie de tests

- **Tests unitaires** : modules sans dépendance externe (config, logger,
  pures fonctions). À enrichir au fil des épices fonctionnelles.
- **Tests d'intégration** : utilisent la stack docker DEV réelle (pas de
  testcontainers à ce stade, gardé volontairement simple). Chaque suite
  ouvre/ferme ses propres connexions et nettoie ses données de test
  (préfixe `__test__`).
- **Tests E2E** : `Supertest` attaque l'application via la factory
  `createApp()` sans bind réseau. Idéal pour les routes HTTP.
- **À venir** : tests de contrats OpenAPI (Épice 4+), tests de mutation
  PostGIS (Épice 6), tests de cohérence Neo4j (Épice 9), tests Playwright
  sur les fronts (Épice 13), tests TestFX sur JavaFX (Épice 14).

---

## 11. Prochaines étapes

### Épice 4 — Authentification, SSO & MFA (priorité 1)

Deux décisions à arbitrer avant démarrage :

1. **Provider OIDC** :
   - **Keycloak conteneurisé** — standard industriel, MFA + SSO out-of-the-box,
     1 conteneur en plus, configuration via Realm export.
   - **Implémentation custom** dans le backend Node — plus léger, plus de code à
     écrire et à sécuriser, contrôle total.
2. **MFA** :
   - **TOTP seul** (RFC 6238, compatible Google Authenticator/Authy).
   - **TOTP + WebAuthn** (clés FIDO2 type YubiKey, UX meilleure, plus complexe).

### Roadmap après l'Épice 4

5. **Conformité RGPD** : endpoints d'accès/rectification/suppression/export,
   journal d'audit immuable (déjà préparé en table `audit_logs`), gestion
   des consentements.
6. **Quartiers géographiques** : API CRUD avec validation GeoJSON, détection
   de chevauchements, "à quel quartier appartient ce point" via PostGIS.
7. **Annonces, services et points** : marketplace de services, wallet de
   points, contrats auto-générés pour les services payants.
8. **Documents PDF & signatures** : upload via MinIO, zones de signature,
   signature numérique horodatée, archivage WORM, MFA obligatoire avant signature.
9. **Événements + recommandations Neo4j** : CRUD événements, swipe, projection
   Neo4j des interactions, Cypher de recommandations.
10. **Messagerie + présence** : Socket.IO, photos/vocaux, accusés de
    réception, présence temps réel.

---

## Licence

[MIT](./LICENSE).

## Contribuer

Voir [`CONTRIBUTING.md`](./CONTRIBUTING.md).
