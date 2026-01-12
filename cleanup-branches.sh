#!/bin/bash
# Script pour supprimer les branches inutiles

set -e

echo "🧹 Nettoyage des branches..."

# Branches à garder
KEEP_BRANCHES=("main" "dev" "dev-clean-final")

# Branches à supprimer (locales)
DELETE_BRANCHES=("dev-clean")

echo "📋 Branches à garder:"
for branch in "${KEEP_BRANCHES[@]}"; do
    echo "  ✅ $branch"
done

echo ""
echo "🗑️  Branches à supprimer (locales):"
for branch in "${DELETE_BRANCHES[@]}"; do
    if git show-ref --verify --quiet refs/heads/"$branch"; then
        echo "  ❌ $branch"
    else
        echo "  ⚠️  $branch (n'existe pas)"
    fi
done

echo ""
read -p "Supprimer les branches inutiles? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    for branch in "${DELETE_BRANCHES[@]}"; do
        if git show-ref --verify --quiet refs/heads/"$branch"; then
            echo "🗑️  Suppression de $branch..."
            git branch -D "$branch" 2>/dev/null || echo "  ⚠️  Impossible de supprimer $branch"
        fi
    done
    echo "✅ Nettoyage terminé"
else
    echo "❌ Annulé"
fi

echo ""
echo "📊 Branches restantes:"
git branch
