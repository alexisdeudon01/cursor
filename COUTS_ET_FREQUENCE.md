# 💰 Coûts et Fréquence - Système d'Amélioration Continue

## 💳 Ce qui est payant

### API Anthropic (Claude)

**C'est le seul service payant utilisé** :
- **Service**: API Anthropic (Claude 3.5 Sonnet)
- **Utilisation**: Analyse de code + Entraînement LLM jeux 2D
- **Coût**: Payant par token utilisé

## 📊 Calcul des coûts

### Tarifs API Anthropic (Claude 3.5 Sonnet)

- **Input**: $0.003 par 1K tokens
- **Output**: $0.015 par 1K tokens

### Par cycle (toutes les 2 heures)

**Analyse de code (50% du temps)** :
- ~2000 tokens input (codebase)
- ~1000 tokens output (analyse)
- Coût: ~$0.006 par cycle

**Entraînement LLM jeux 2D (50% du temps)** :
- ~3000 tokens input (dataset)
- ~2000 tokens output (jeux générés)
- Coût: ~$0.009 par cycle

**Génération UML** :
- ~500 tokens input
- ~500 tokens output
- Coût: ~$0.001 par cycle

**Total par cycle**: ~$0.016

### Par jour (toutes les 2 heures)

- **12 cycles/jour** (24h / 2h)
- **Coût/jour**: 12 × $0.016 = **~$0.19/jour**

### Par semaine

- **84 cycles/semaine** (12 cycles/jour × 7 jours)
- **Coût/semaine**: 84 × $0.016 = **~$1.34/semaine**

### Par mois

- **~360 cycles/mois** (12 cycles/jour × 30 jours)
- **Coût/mois**: 360 × $0.016 = **~$5.76/mois**

### Par an

- **Coût/an**: ~$69.12/an

## ⏰ Fréquence modifiée

### Avant
- **Fréquence**: Toutes les 30 minutes
- **Cycles/jour**: 48
- **Coût/jour**: ~$0.77
- **Coût/mois**: ~$23.04

### Après (modifié)
- **Fréquence**: Toutes les 2 heures ✅
- **Cycles/jour**: 12
- **Coût/jour**: ~$0.19
- **Coût/mois**: ~$5.76

### Économie
- **Réduction**: 75% des coûts
- **Économie/mois**: ~$17.28

## 🔧 Modification appliquée

**Fichier**: `.github/workflows/auto-improve.yml`

**Avant**:
```yaml
schedule:
  - cron: '*/30 * * * *'  # Toutes les 30 minutes
```

**Après**:
```yaml
schedule:
  - cron: '0 */2 * * *'  # Toutes les 2 heures
```

## 📋 Autres services (gratuits)

- ✅ **GitHub Actions**: Gratuit (jusqu'à 2000 minutes/mois)
- ✅ **GitHub**: Gratuit (repo public/privé)
- ✅ **Docker Hub**: Gratuit (images publiques)
- ✅ **Git**: Gratuit

## 💡 Optimisations possibles

### Option 1: Réduire encore la fréquence
- **Toutes les 4 heures**: ~$2.88/mois
- **Toutes les 6 heures**: ~$1.92/mois
- **Une fois par jour**: ~$0.48/mois

### Option 2: Désactiver l'entraînement LLM
- **Coût réduit**: ~50% (seulement analyse code)
- **Coût/mois**: ~$2.88

### Option 3: Mode basique (sans IA)
- **Coût**: $0/mois
- **Fonctionnalités**: Vérifications basiques uniquement

## 🎯 Recommandation

**Fréquence actuelle (2 heures)** :
- ✅ Bon équilibre coût/qualité
- ✅ 12 cycles/jour suffisants pour amélioration continue
- ✅ Coût raisonnable (~$5.76/mois)

## 📊 Monitoring des coûts

Pour surveiller les coûts :
1. Aller sur https://console.anthropic.com/
2. Section **Usage & Billing**
3. Vérifier les appels API et coûts

---

**Fréquence modifiée le**: 2026-01-13  
**Nouvelle fréquence**: Toutes les 2 heures  
**Économie**: 75% des coûts
