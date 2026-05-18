# lex-yacc

**Épice 12** — DSL maison d'interrogation de MongoDB, généré via `lex` + `yacc`
(ou `flex` + `bison`).

## Objectif

Offrir aux administrateurs (et au back-office) un mini-langage déclaratif
plus lisible que les filtres Mongo bruts, qui se compile en pipelines
d'agrégation MongoDB.

Exemple cible (syntaxe indicative) :

```
FIND messages
WHERE conversation = "c-42"
  AND createdAt > "2026-01-01"
ORDER BY createdAt DESC
LIMIT 50
```

→ compilé en :

```js
db.messages.find(
  { conversationId: "c-42", createdAt: { $gt: ISODate("2026-01-01") } }
).sort({ createdAt: -1 }).limit(50)
```

## Architecture

```
lex-yacc/
├── grammar/
│   ├── lexer.l       # règles lex/flex
│   └── parser.y      # règles yacc/bison
├── src/              # AST + générateur JS (ou WASM exposé au backend)
├── tests/            # corpus d'exemples + expected output
└── Makefile          # cibles build/test
```

## Intégration

Le binaire (ou la lib JS générée) est appelé par le backend Node.js, qui
expose un endpoint `POST /dsl/execute` (protégé MFA, audit RGPD).
