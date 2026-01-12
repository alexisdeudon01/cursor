# 🔄 Cycle d'Amélioration Automatique - Toutes les 30 minutes

## 🌿 Branche utilisée

**Branche actuelle:** `dev` ✅

**Vérification:**
- ✅ Workflow GitHub Actions utilise `ref: dev` (ligne 28)
- ✅ Commits automatiques poussés sur `origin/dev`
- ✅ Déclenchement sur push vers `dev`

## ⏰ Déclenchement

Le workflow s'exécute automatiquement :
1. **Toutes les 30 minutes** (cron: `*/30 * * * *`)
2. **Sur push vers `dev`** (si fichiers modifiés dans `.cursor/agents/**`, `Assets/Scripts/**`, etc.)
3. **Manuellement** via GitHub Actions UI (workflow_dispatch)

## 📋 Actions exécutées toutes les 30 minutes

### Phase 1: Préparation (1-2 min)

1. **Checkout du repository**
   ```bash
   git checkout dev
   git pull origin dev
   ```

2. **Setup de l'environnement**
   - Installation Node.js 20
   - Installation Python 3.11
   - Installation dépendances: `requests`, `mermaid-cli`

3. **Vérification des secrets**
   - Vérifie `ANTHROPIC_API_KEY` dans GitHub Secrets
   - Vérifie `GITHUB_TOKEN` (automatique)

### Phase 2: Amélioration avec IA (50% LLM + 50% Code) (10-15 min)

#### 🎮 50% du temps - Entraînement LLM pour jeux 2D

**Script:** `.github/scripts/train-llm-games.py`

**Actions:**
1. **Collecte de données** (2-3 min)
   - Recherche de règles de jeux 2D sur internet
   - Analyse de patterns de mouvement, capture, victoire
   - Documentation de règles communes

2. **Création dataset** (2-3 min)
   - Génération de données d'entraînement
   - Formatage pour l'entraînement LLM
   - Validation des données

3. **Entraînement LLM** (3-5 min)
   - Appel API Claude avec prompts spécialisés
   - Génération de jeux 2D de test
   - Évaluation de la qualité des jeux générés

4. **Test des jeux générés** (2-3 min)
   - Vérification des règles
   - Test de compatibilité avec le framework Unity NGO
   - Documentation des résultats

**Fichiers générés:**
- `.cursor/agents/llm-training-data/` (datasets)
- `.cursor/agents/generated-games/` (jeux générés)
- `.cursor/agents/llm-training-log.md` (journal d'entraînement)

#### 💻 50% du temps - Amélioration du code

**Script:** `.github/scripts/auto-improve-ai.py`

**Actions:**
1. **Détection de la version de l'agent** (30 sec)
   - Scan de `.cursor/agents/thebestclient*.md`
   - Trouve la dernière version (actuellement v5)
   - Lit les instructions de l'agent

2. **Analyse du codebase avec IA** (3-5 min)
   - Appel API Claude avec prompt d'analyse
   - Scan des fichiers importants:
     - `Assets/Scripts/**/*.cs`
     - `Assets/**/*.asmdef`
     - `Assets/**/*.unity`
     - `Assets/**/*.prefab`
   - Analyse de:
     - Architecture (séparation Client/Serveur)
     - Modularité (jeux, sessions, maps)
     - Configuration réseau
     - Problèmes de code
     - Optimisations possibles

3. **Génération du rapport d'analyse** (1-2 min)
   - Création de `thebestclient6-analysis-report.md`
   - Documentation des problèmes trouvés
   - Suggestions d'améliorations

4. **Application des améliorations critiques** (2-4 min)
   - Améliorations automatiques (sécurité, bugs critiques)
   - Création de patches pour améliorations importantes
   - Mise à jour de la documentation

5. **Création de la nouvelle version de l'agent** (1-2 min)
   - Génération de `thebestclient6.md` (ou version suivante)
   - Incorporation des apprentissages
   - Mise à jour des instructions

6. **Tests de connexion réseau** (1 min)
   - Vérification `UseEncryption = false`
   - Vérification configuration `UnityTransport`
   - Vérification `NetworkPrefabs` enregistrés

### Phase 3: Génération UML (2-3 min)

**Script:** `.github/scripts/generate-uml-diagrams.py`

**Actions:**
1. **Génération diagrammes Mermaid**
   - Architecture globale
   - Modularité (jeux, sessions, maps)
   - Client/Serveur séparation

2. **Conversion en PNG**
   - Export des diagrammes
   - Stockage dans `.cursor/agents/diagrams/`

**Fichiers générés:**
- `.cursor/agents/diagrams/architecture-v6.mmd`
- `.cursor/agents/diagrams/architecture-v6.png`
- `.cursor/agents/diagrams/modularity-v6.mmd`
- `.cursor/agents/diagrams/client-server-v6.mmd`

### Phase 4: Commit et Push (1 min)

**Actions:**
1. **Configuration Git**
   ```bash
   git config user.email "action@github.com"
   git config user.name "GitHub Action"
   ```

2. **Ajout des changements**
   ```bash
   git add -A
   ```

3. **Commit automatique**
   ```bash
   git commit -m "🤖 Auto-improve: Cycle 20250112-143000 [skip ci]"
   ```
   (Date/heure dynamique)

4. **Push vers `origin/dev`**
   ```bash
   git push origin dev
   ```

## 📊 Fichiers modifiés/créés par cycle

### Fichiers toujours créés
- `.cursor/agents/thebestclientX-analysis-report.md` (nouveau rapport)
- `.cursor/agents/thebestclientX.md` (nouvelle version agent)
- `.cursor/agents/improvement-log.md` (mis à jour)
- `.cursor/agents/diagrams/*-vX.mmd` et `.png` (nouveaux diagrammes)

### Fichiers créés si améliorations appliquées
- Modifications dans `Assets/Scripts/**/*.cs`
- Modifications dans `.github/scripts/*.py`
- Mise à jour documentation (`*.md`)

### Fichiers créés pour LLM (50% du temps)
- `.cursor/agents/llm-training-data/*.json`
- `.cursor/agents/generated-games/*.cs`
- `.cursor/agents/llm-training-log.md`

## ⏱️ Temps total par cycle

| Phase | Durée | Description |
|-------|-------|-------------|
| Préparation | 1-2 min | Setup environnement |
| Entraînement LLM (50%) | 10-15 min | Jeux 2D |
| Amélioration code (50%) | 10-15 min | Analyse + améliorations |
| Génération UML | 2-3 min | Diagrammes |
| Commit/Push | 1 min | Git |
| **TOTAL** | **24-36 min** | Par cycle |

**Note:** Si le cycle dépasse 30 minutes, le prochain cycle attendra la fin du précédent.

## 💰 Coût par cycle

- **Entraînement LLM:** ~$0.009
- **Analyse code:** ~$0.006
- **Génération UML:** ~$0.001
- **TOTAL:** ~$0.016 par cycle

**Par semaine (336 cycles):** ~$2.86

## 🔍 Monitoring

### Vérifier l'exécution
1. GitHub → **Actions** → `Auto-Improve Project (Thebestclient)`
2. Voir les logs en temps réel
3. Vérifier les commits sur `dev`

### Vérifier les résultats
1. Lire `.cursor/agents/improvement-log.md`
2. Lire `thebestclientX-analysis-report.md`
3. Voir les diagrammes dans `.cursor/agents/diagrams/`

### Vérifier les coûts
1. https://console.anthropic.com/
2. Section **Usage & Billing**

## 🎯 Objectif final

L'agent continue d'améliorer le projet jusqu'à atteindre un **projet parfait** :
- ✅ Architecture optimale
- ✅ Modularité maximale
- ✅ Code propre et maintenable
- ✅ Framework adapté à tous les jeux 2D
- ✅ LLM spécialisé pour génération de jeux 2D

## 🚨 En cas de problème

Si un cycle échoue :
- Le workflow GitHub Actions affichera l'erreur
- Le prochain cycle (30 min après) réessayera
- Les erreurs sont loggées dans GitHub Actions

---

**✅ Le système fonctionne automatiquement toutes les 30 minutes sur la branche `dev` !**
