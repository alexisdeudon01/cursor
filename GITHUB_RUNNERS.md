# 🏃 Runners GitHub Actions

## 📊 Configuration actuelle

### Runner utilisé

**Runner**: `ubuntu-latest` (runner par défaut GitHub)

**Fichier**: `.github/workflows/auto-improve.yml`

```yaml
jobs:
  improve:
    runs-on: ubuntu-latest  # Runner par défaut GitHub
```

## 🔍 Détails du runner

### ubuntu-latest

**Type**: Runner hébergé par GitHub (gratuit)

**Spécifications** :
- **OS**: Ubuntu Linux (dernière version LTS)
- **CPU**: 2 cores
- **RAM**: 7 GB
- **Disque**: 14 GB SSD
- **Coût**: Gratuit (jusqu'à 2000 minutes/mois)

**Limites** :
- ✅ 2000 minutes/mois gratuites
- ✅ Illimité pour repos publics
- ⚠️ Limite pour repos privés (selon plan GitHub)

## 🎯 Runners disponibles

### Runners GitHub (hébergés)

1. **ubuntu-latest** ✅ (utilisé actuellement)
   - Ubuntu Linux
   - Gratuit (2000 min/mois)
   - Recommandé pour la plupart des cas

2. **windows-latest**
   - Windows Server
   - Gratuit (2000 min/mois)
   - Pour builds Windows

3. **macos-latest**
   - macOS
   - Gratuit (2000 min/mois)
   - Pour builds macOS/iOS

### Runners self-hosted (personnalisés)

**Non configuré actuellement** ❌

Si tu veux utiliser des runners self-hosted :
- Tu dois les configurer toi-même
- Nécessite une machine avec Docker
- Avantages : Plus de contrôle, pas de limite de temps
- Inconvénients : Maintenance, coûts serveur

## 📊 Utilisation actuelle

### Workflow auto-improve.yml

**Runner**: `ubuntu-latest`

**Utilisé pour** :
- ✅ Exécution Python scripts
- ✅ Appels API Anthropic
- ✅ Génération diagrammes UML
- ✅ Build Docker Unity (si disponible)
- ✅ Git operations (commit, push)

**Temps d'exécution estimé** :
- Par cycle : ~15-30 minutes
- Par jour (12 cycles) : ~3-6 heures
- Par mois : ~90-180 heures
- **Dans la limite gratuite** ✅ (2000 min/mois = ~33 heures)

## 💡 Optimisations possibles

### Option 1: Garder ubuntu-latest (recommandé)

**Avantages** :
- ✅ Gratuit
- ✅ Pas de maintenance
- ✅ Mise à jour automatique
- ✅ Suffisant pour nos besoins

### Option 2: Self-hosted runner

**Si tu veux plus de contrôle** :
- Configurer un runner sur ta machine/serveur
- Plus de temps disponible
- Mais nécessite maintenance

**Configuration** :
```yaml
runs-on: self-hosted
```

## ✅ Vérification

Pour vérifier les runners disponibles :

1. GitHub → **Settings** → **Actions** → **Runners**
2. Voir les runners configurés
3. Vérifier l'utilisation (minutes utilisées/mois)

## 📋 Résumé

**Runner actuel** :
- ✅ `ubuntu-latest` (runner GitHub par défaut)
- ✅ Gratuit (2000 min/mois)
- ✅ Suffisant pour le workflow
- ✅ Pas de configuration supplémentaire nécessaire

**Runners personnalisés** :
- ❌ Non configuré
- ⚠️ Pas nécessaire actuellement
- 💡 Peut être ajouté si besoin

---

**Configuration vérifiée le**: 2026-01-13
