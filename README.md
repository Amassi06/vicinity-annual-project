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
