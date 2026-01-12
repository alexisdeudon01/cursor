#!/bin/bash
# Script pour nettoyer l'historique Git et retirer la clé API

set -e

echo "🧹 Nettoyage de l'historique Git..."

# Le commit problématique est 0699ff9
# On va le modifier pour retirer la clé

# Option 1: Rebase interactif (nécessite intervention manuelle)
echo "📝 Pour nettoyer l'historique:"
echo ""
echo "1. Exécuter: git rebase -i 134a886"
echo "2. Dans l'éditeur, changer 'pick' en 'edit' pour le commit 0699ff9"
echo "3. Modifier les fichiers pour retirer la clé"
echo "4. git commit --amend"
echo "5. git rebase --continue"
echo ""
echo "OU utiliser la méthode automatique ci-dessous..."

# Option 2: Créer une nouvelle branche propre
echo ""
echo "🔄 Création d'une branche propre..."

# Sauvegarder l'état actuel
git stash

# Créer une branche depuis avant le commit problématique
git checkout -b dev-clean 134a886

# Appliquer les changements propres (sans la clé)
git checkout dev -- .github/QUICK_SETUP.md .github/SETUP_API_KEY.md .github/scripts/setup-api-key.sh 2>/dev/null || true

# Vérifier qu'il n'y a pas de clé
if grep -r "sk-ant-api03" .github/ 2>/dev/null; then
    echo "❌ Clé encore présente - nettoyage manuel nécessaire"
    exit 1
fi

# Commit propre
git add .github/
git commit -m "🔐 Documentation pour configurer ANTHROPIC_API_KEY (sans clé dans le code)"

# Appliquer les autres commits
git cherry-pick 6fa2a93 449204d 93db58f 2>/dev/null || echo "Certains commits déjà appliqués"

echo "✅ Branche propre créée: dev-clean"
echo ""
echo "📋 Prochaines étapes:"
echo "1. Vérifier: git log --oneline dev-clean"
echo "2. Si OK: git push origin dev-clean:dev --force"
echo "3. Ajouter la clé dans GitHub Secrets"
