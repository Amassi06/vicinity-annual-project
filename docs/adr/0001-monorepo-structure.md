# 1. Adoption d'une structure monorepo

- Statut : accepté
- Date : 2026-05-16

## Contexte

Le projet Connected Neighbours impose plusieurs livrables hétérogènes :
backend Node.js, deux frontends React, un client lourd JavaFX, un DSL
lex/yacc, et de l'infrastructure (Docker, CI). Ces composants partagent
des contrats (OpenAPI, schémas d'événements) et doivent évoluer de manière
synchronisée.

## Décision

Tous les sous-projets vivent dans un **monorepo unique** structuré par
domaine technique au premier niveau. La gestion des dépendances reste
**par sous-projet** (pas de Yarn/pnpm workspaces à ce stade) pour garder
l'indépendance de tooling entre Node.js, Java et le DSL.

## Conséquences

- Positives : un seul `git clone`, ADR centralisés, CI mutualisée,
  refactor cross-stack atomique.
- Négatives : pipelines CI plus volumineux (mitigé par jobs conditionnels
  sur paths).

## Alternatives écartées

- **Multi-repo** : surcharge d'orchestration, dérive des versions de
  contrats inter-services, friction d'onboarding.
- **Workspaces npm/pnpm** : prématuré tant que le backend et les fronts
  ne partagent pas encore de package interne.
