#!/bin/bash
# Script pour merger la branche dev vers branché-1

echo "=== Merge dev → branché-1 ==="
echo ""

# Vérifier que nous sommes sur la branche dev
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" != "dev" ]; then
    echo "⚠️  Vous n'êtes pas sur la branche 'dev' (branche actuelle: $CURRENT_BRANCH)"
    read -p "Voulez-vous basculer vers 'dev' ? (o/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Oo]$ ]]; then
        git checkout dev
    else
        echo "Annulé."
        exit 1
    fi
fi

# Vérifier que la branche dev est à jour
echo "📥 Mise à jour de la branche dev..."
git fetch origin dev 2>/dev/null || echo "Pas de remote dev, continuation..."

# Vérifier s'il y a des changements non commités
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "⚠️  Vous avez des changements non commités"
    git status --short
    read -p "Voulez-vous les commiter avant de merger ? (o/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Oo]$ ]]; then
        read -p "Message de commit : " COMMIT_MSG
        if [ -z "$COMMIT_MSG" ]; then
            COMMIT_MSG="WIP: Uncommitted changes"
        fi
        git add -A
        git commit -m "$COMMIT_MSG"
    else
        echo "❌ Veuillez commiter ou stash vos changements avant de merger"
        exit 1
    fi
fi

# Basculer vers branché-1
echo ""
echo "🔄 Basculement vers branché-1..."
if ! git checkout branché-1; then
    echo "❌ Erreur lors du basculement vers branché-1"
    exit 1
fi

# Mettre à jour branché-1
echo "📥 Mise à jour de branché-1..."
git fetch origin branché-1 2>/dev/null || echo "Pas de remote branché-1, continuation..."

# Merger dev dans branché-1
echo ""
echo "🔀 Merge de dev dans branché-1..."
if git merge dev --no-ff -m "Merge dev into branché-1"; then
    echo ""
    echo "✅ Merge réussi !"
    echo ""
    echo "📊 Statut actuel :"
    git log --oneline --graph -5
    echo ""
    read -p "Voulez-vous pousser les changements vers GitHub ? (o/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Oo]$ ]]; then
        echo "🚀 Push vers GitHub..."
        if git push origin branché-1; then
            echo "✅ Push réussi !"
        else
            echo "❌ Erreur lors du push"
            exit 1
        fi
    fi
else
    echo ""
    echo "❌ Conflits de merge détectés !"
    echo ""
    echo "Résolvez les conflits puis :"
    echo "  1. git add <fichiers-résolus>"
    echo "  2. git commit"
    echo "  3. git push origin branché-1"
    exit 1
fi
