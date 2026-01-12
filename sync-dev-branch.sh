#!/bin/bash
# Script de synchronisation de la branche dev
# Récupère les commits remote et pousse les commits locaux

set -e

cd /workspace || exit 1

echo "🔄 Synchronisation branche dev"
echo "================================"
echo ""

# 1. Vérifier l'état actuel
echo "1. État actuel:"
echo "   Branche: $(git branch --show-current)"
echo "   Commits locaux non poussés: $(git log origin/dev..HEAD --oneline | wc -l)"
echo "   Commits remote non récupérés: $(git log HEAD..origin/dev --oneline | wc -l)"
echo ""

# 2. Récupérer les commits du remote
echo "2. Récupération des commits remote..."
git fetch origin dev

# 3. Rebase pour appliquer nos commits par-dessus les commits remote
echo ""
echo "3. Rebase sur origin/dev..."
git rebase origin/dev

# 4. Pousser les commits
echo ""
echo "4. Push vers origin/dev..."
git push origin dev

echo ""
echo "✅ Synchronisation terminée!"
echo ""
echo "Vérification:"
git log --oneline -5
