#!/usr/bin/env bash
# Script pour pousser les changements vers la branche dev

set -euo pipefail

BRANCH="dev"
REMOTE="origin"

echo "=========================================="
echo "Push vers dev"
echo "=========================================="
echo ""

# Vérifier qu'on est dans un repo git
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "❌ Erreur: Ce n'est pas un dépôt Git"
    exit 1
fi

# Vérifier l'état actuel
CURRENT_BRANCH=$(git branch --show-current)
echo "📍 Branche actuelle: $CURRENT_BRANCH"

# Vérifier s'il y a des modifications non commitées
if ! git diff-index --quiet HEAD --; then
    echo "⚠️  Attention: Il y a des modifications non commitées"
    echo "   Voulez-vous continuer quand même? (y/N)"
    read -r response
    if [[ ! "$response" =~ ^[Yy]$ ]]; then
        echo "❌ Annulé"
        exit 1
    fi
fi

# Vérifier si la branche dev existe
if ! git show-ref --verify --quiet refs/heads/"$BRANCH"; then
    echo "❌ Erreur: La branche '$BRANCH' n'existe pas localement"
    exit 1
fi

# Vérifier les commits à pousser
COMMITS_AHEAD=$(git rev-list --count "$REMOTE/$BRANCH".."$BRANCH" 2>/dev/null || echo "0")
if [ "$COMMITS_AHEAD" -eq 0 ]; then
    echo "✅ Aucun commit à pousser - la branche est à jour"
    exit 0
fi

echo "📊 Commits à pousser: $COMMITS_AHEAD"
echo ""
echo "📝 Derniers commits à pousser:"
git log "$REMOTE/$BRANCH".."$BRANCH" --oneline -5
echo ""

# Demander confirmation
echo "❓ Voulez-vous pousser vers $REMOTE/$BRANCH? (y/N)"
read -r response
if [[ ! "$response" =~ ^[Yy]$ ]]; then
    echo "❌ Annulé"
    exit 0
fi

# Basculer sur la branche si nécessaire
if [ "$CURRENT_BRANCH" != "$BRANCH" ]; then
    echo "🔄 Basculement vers la branche $BRANCH..."
    git checkout "$BRANCH"
fi

# Faire le push
echo ""
echo "🚀 Poussage vers $REMOTE/$BRANCH..."
if git push "$REMOTE" "$BRANCH"; then
    echo ""
    echo "✅ Push réussi!"
    echo ""
    echo "📊 État après push:"
    git status -sb
else
    echo ""
    echo "❌ Erreur lors du push"
    echo ""
    echo "💡 Solutions possibles:"
    echo "   1. Vérifier votre authentification SSH"
    echo "   2. Vérifier que vous avez les droits d'écriture"
    echo "   3. Faire un pull d'abord si la branche distante a changé"
    exit 1
fi
