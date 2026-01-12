# 📋 Fichiers de l'Agent Thebestclient5 et Configuration

## 🌿 Branche utilisée

**Branche: `dev`**

Le workflow GitHub Actions utilise la branche `dev` pour :
- ✅ Exécution automatique toutes les 30 minutes
- ✅ Commit et push automatiques
- ✅ Déclenchement sur push vers `dev`

## 📁 Fichiers relatifs à l'agent

### Fichiers principaux de l'agent

1. **`.cursor/agents/thebestclient5.md`** ⭐
   - Définition complète de l'agent v5
   - Instructions d'amélioration continue
   - Répartition 50% LLM jeux 2D + 50% amélioration code

2. **`.cursor/agents/thebestclient2.md`** → **`thebestclient5.md`**
   - Historique des versions de l'agent
   - Évolution des instructions

3. **`.cursor/agents/thebestclientX-analysis-report.md`**
   - Rapports d'analyse générés par chaque cycle
   - Analyse de l'architecture, modularité, code

4. **`.cursor/agents/review-playbook-v1.md`**
   - Playbook de revue de code
   - Mis à jour automatiquement

5. **`.cursor/agents/improvement-log.md`**
   - Journal de toutes les améliorations appliquées
   - Historique complet

### Scripts Python

1. **`.github/scripts/auto-improve-ai.py`** ⭐
   - Script principal d'amélioration avec IA
   - Appelle l'API Claude (Anthropic)
   - Gère la répartition 50/50 (LLM + Code)

2. **`.github/scripts/train-llm-games.py`** ⭐
   - Entraînement LLM pour jeux 2D (50% du temps)
   - Collecte de données, création dataset
   - Test de jeux générés

3. **`.github/scripts/check-api-access.py`** ⭐
   - Test de connexion API Anthropic
   - Test Git et GitHub Token
   - Vérification complète

4. **`.github/scripts/generate-uml-diagrams.py`**
   - Génération de diagrammes UML (Mermaid)
   - Architecture, modularité, client/serveur

5. **`.github/scripts/research-2d-games.py`**
   - Recherche de patterns de jeux 2D
   - Analyse de règles communes

6. **`.github/scripts/auto-improve.py`**
   - Script de fallback (sans IA)
   - Vérifications basiques

### Workflow GitHub Actions

1. **`.github/workflows/auto-improve.yml`** ⭐
   - Déclenchement toutes les 30 minutes
   - Utilise la branche `dev`
   - Appelle `auto-improve-ai.py`

### Documentation

1. **`.github/README_AUTO_IMPROVE.md`**
   - Guide de configuration
   - Instructions d'utilisation

2. **`.github/GIT_STRATEGY.md`**
   - Stratégie Git complète
   - Gestion des branches et secrets

## 🐳 Environnement

**Je suis en Docker** ✅
- Hostname: `cursor`
- OS: Linux
- Workspace: `/workspace`

## 🔧 Actions à faire

### ✅ Déjà fait
- [x] Secret `ANTHROPIC_API_KEY` ajouté dans GitHub Secrets
- [x] Fichiers synchronisés sur `origin/dev`
- [x] Workflow configuré

### 🧪 Test de connexion

**Tu peux me laisser faire le test maintenant** ou exécuter manuellement :

```bash
cd /home/tor/wkspaces/mo2
python3 .github/scripts/check-api-access.py
```

## 💰 Évaluation du coût par semaine

### Calcul détaillé

**Configuration:**
- Workflow toutes les **30 minutes**
- **48 exécutions/jour**
- **336 exécutions/semaine**

**Coûts API Anthropic (Claude 3.5 Sonnet):**

1. **Analyse de code (50% du temps)**
   - ~2000 tokens input (codebase)
   - ~1000 tokens output (analyse)
   - Coût: $0.003/1K input, $0.015/1K output
   - Par exécution: ~$0.006
   - 168 exécutions/semaine: **~$1.01**

2. **Entraînement LLM jeux 2D (50% du temps)**
   - ~3000 tokens input (dataset)
   - ~2000 tokens output (jeux générés)
   - Coût: ~$0.009 par exécution
   - 168 exécutions/semaine: **~$1.51**

3. **Génération UML**
   - ~500 tokens input
   - ~500 tokens output
   - Coût: ~$0.001 par exécution
   - 336 exécutions/semaine: **~$0.34**

**Total par semaine: ~$2.86**

### Estimation mensuelle

- **Par semaine:** ~$2.86
- **Par mois (4 semaines):** ~$11.44
- **Par an:** ~$148.72

### Optimisations possibles

1. **Réduire la fréquence** à 1 heure → **~$1.43/semaine**
2. **Désactiver l'entraînement LLM** → **~$1.35/semaine**
3. **Mode basique (sans IA)** → **$0/semaine**

## 🚀 Prochaines étapes

1. **Test de connexion** (je peux le faire maintenant)
2. **Vérifier le workflow** sur GitHub Actions
3. **Surveiller les premiers cycles** (48 premières heures)
4. **Ajuster la fréquence** si nécessaire

## 📊 Monitoring

Pour surveiller les coûts :
1. Aller sur https://console.anthropic.com/
2. Section **Usage & Billing**
3. Vérifier les appels API

Pour surveiller les améliorations :
1. GitHub → **Actions** → `Auto-Improve Project (Thebestclient)`
2. Vérifier les commits sur `dev`
3. Lire `.cursor/agents/improvement-log.md`
