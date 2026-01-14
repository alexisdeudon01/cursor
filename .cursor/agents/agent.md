---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

---
name: agent
model: fast
---

# Agent Auto-Évolutif - TheBestClient

> **Version**: 1.0.0  
> **Auto-Amélioration**: Activée via EvoAgentX  
> **Rollback**: Automatique si régression détectée

---

## 0. Emplacement de référence

- **Cet agent est défini dans** : `.cursor/agents/agent.md`.
- Toute autre documentation doit **référencer ce fichier** comme source d’autorité.

---

## 1. Mission

Développer un jeu 2D client-serveur avec serveur **full authoritative** et architecture **Data‑Oriented Design**.

---

## 2. Principes fondamentaux

| Principe | Règle |
|----------|-------|
| **Serveur Autoritaire** | TOUTE la logique sur le serveur |
| **Client Passif** | Envoie inputs, reçoit état, affiche |
| **DOD** | Structs pour données réseau |
| **Single Executable** | UN seul build, distinction par arguments |
| **NGO Only** | Netcode for GameObjects uniquement |
| **No Third‑Party** | Aucun service tiers (Unity Services, etc.) |
| **No Copilot** | Aucun agent Copilot |

---

## 3. Source de vérité : fichiers `.unity`

### ✅ RÈGLE CRITIQUE
La **structure du projet** (scènes, flux, objets) doit être **déterminée uniquement à partir des fichiers `.unity`**.

### Qu’est‑ce qu’un fichier `.unity` ?
- Un **fichier de scène Unity** (format YAML sérialisé) qui décrit la hiérarchie d’objets, composants, références et paramètres.
- C’est la **seule source fiable** pour comprendre la structure réelle des scènes.

### Interdictions
- ❌ **Ne pas** déduire la structure à partir d’un README, d’un listing de dossier ou d’un diagramme existant.
- ❌ **Ne pas** écrire dans les fichiers `.unity` (lecture seule).

---

## 4. Fichiers Unity et `.meta`

### `.meta`
- Chaque fichier Unity (scène, prefab, script, asset) possède un **fichier `.meta`** associé.
- Le `.meta` contient le **GUID** et des métadonnées critiques utilisées par Unity pour les références.
- **Ne jamais modifier** les `.meta` manuellement, sous peine de casser les liens entre assets.

### Fichiers autorisés à la modification
- ✅ Scripts C#.
- ✅ UXML / USS.
- ✅ Documentation (README, agent, etc.).
- ✅ Manifests Unity (Registry uniquement).
- ❌ `.unity`, `.prefab`, `.meta` (lecture seule).

---

## 5. Règles réseau NGO

- **NGO uniquement** (Unity.Netcode + Unity Transport).
- **StartServer()** et **jamais** StartHost().
- Pas de services cloud tiers.

---

## 6. Auto‑amélioration & UML

### UML (générer à chaque itération)
Produire **tous les diagrammes UML pertinents** :
- **Classes**, **Séquences**, **États**, **Activités**, **Composants**, **Déploiement**.

### Algorithmes d’amélioration via Codex
Le pipeline peut sélectionner un modèle via Codex pour **évaluer** et **améliorer** le code. Propositions :
- **Diff‑based snapshotting** : snapshots et régressions sur base de diffs.
- **Interest Management** : filtrage réseau spatial/zone.
- **Input Prediction + Reconciliation** côté client (autorité serveur maintenue).
- **Delta Compression** et **packing binaire**.
- **State hashing** pour validation serveur.
- **Event batching** pour réduire la charge réseau.

---

## 7. Testing (propositions “smart variables”)

Variables de test recommandées :
- `SIM_SEED` (reproductibilité),
- `NET_TICK_RATE` (tps/s),
- `MAX_PLAYERS`,
- `JITTER_MS`,
- `PACKET_LOSS_PCT`,
- `SNAPSHOT_RATE`,
- `INPUT_BUFFER_MS`,
- `SERVER_AUTHORITY_STRICT`,
- `STATE_HASH_INTERVAL`.

Objectifs de tests :
- **Autorité serveur** stricte.
- **Stabilité** sous perte/jitter.
- **Évolutivité** (n joueurs).

---

## 8. Workflow attendu

- **Exécution horaire** du workflow d’amélioration.
- **Arrêt si coût total > 200€** (guard budgétaire).
- **Push à chaque étape** si le workflow est autorisé.
- **Pas d’approbation** requise.
- **Fonctionnement headless** (machine éteinte). 

---

*Dernière mise à jour: AUTO_DATE*  
*Score actuel: AUTO_SCORE*
