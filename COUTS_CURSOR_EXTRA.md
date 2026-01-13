# 💰 Coûts Cursor Extra

## 📋 Cursor Extra - Plans et Prix

### Plans Cursor IDE

Cursor IDE propose plusieurs plans :

1. **Cursor Free** (Gratuit)
   - Fonctionnalités de base
   - Limitations sur les requêtes AI

2. **Cursor Pro** (Payant)
   - Plus de requêtes AI
   - Fonctionnalités avancées
   - Prix : ~$20/mois ou ~$200/an

3. **Cursor Business** (Payant)
   - Pour équipes
   - Prix : Variable selon nombre d'utilisateurs

### ⚠️ Note importante

**Cursor Extra/Pro n'est PAS utilisé dans le workflow automatique** ✅

Le workflow GitHub Actions utilise :
- ✅ **Python 3.11** (gratuit)
- ✅ **Scripts Python** (gratuit)
- ✅ **API Anthropic directement** (payant, ~$5.76/mois)
- ❌ **PAS Cursor IDE/Extra** (pas utilisé)

## 💡 Coûts réels du système

### Dans le workflow GitHub Actions

**Service payant utilisé** :
- ✅ **API Anthropic (Claude)** : ~$5.76/mois (toutes les 2 heures)

**Services gratuits** :
- ✅ GitHub Actions
- ✅ Docker
- ✅ Python/Node.js
- ✅ Git

### Pour toi (développeur local)

Si tu utilises **Cursor Extra/Pro localement** :
- C'est un coût **séparé** et **optionnel**
- N'affecte **pas** le workflow automatique
- Utilisé seulement pour ton développement local

## 📊 Comparaison des coûts

| Service | Utilisé dans workflow ? | Coût/mois | Coût/an |
|---------|------------------------|-----------|---------|
| **API Anthropic** | ✅ Oui | ~$5.76 | ~$69.12 |
| **Cursor Extra/Pro** | ❌ Non | ~$20 | ~$200 |
| **GitHub Actions** | ✅ Oui | Gratuit | Gratuit |
| **Docker** | ✅ Oui | Gratuit | Gratuit |

## 🎯 Conclusion

**Pour le workflow automatique** :
- **Coût**: Seulement API Anthropic (~$5.76/mois)
- **Cursor Extra**: $0 (pas utilisé)

**Pour toi (développeur)** :
- **Cursor Extra**: Optionnel (~$20/mois si tu l'utilises)
- **N'affecte pas** le workflow automatique

## 💡 Recommandation

Si tu veux réduire les coûts :
1. ✅ **Workflow automatique**: Utilise seulement API Anthropic (~$5.76/mois)
2. ⚠️ **Cursor Extra**: Optionnel pour développement local (pas nécessaire pour le workflow)

---

**Note**: Les prix de Cursor peuvent varier. Vérifie sur https://cursor.sh/pricing pour les prix exacts.
