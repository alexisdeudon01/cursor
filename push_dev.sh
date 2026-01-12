#!/bin/bash
# Script pour pousser la branche dev vers GitHub

echo "=== Push de la branche dev vers GitHub ==="
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

# Vérifier s'il y a des changements non commités
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "⚠️  Vous avez des changements non commités"
    git status --short
    read -p "Voulez-vous les commiter avant de push ? (o/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Oo]$ ]]; then
        read -p "Message de commit : " COMMIT_MSG
        if [ -z "$COMMIT_MSG" ]; then
            COMMIT_MSG="WIP: Uncommitted changes"
        fi
        git add -A
        if ! git commit -m "$COMMIT_MSG"; then
            echo "❌ Erreur lors du commit"
            exit 1
        fi
    else
        echo "❌ Veuillez commiter ou stash vos changements avant de push"
        exit 1
    fi
fi

# Pousser vers GitHub
echo ""
echo "🚀 Push de dev vers GitHub..."
if git push -u origin dev; then
    echo ""
    echo "✅ Push réussi !"
    echo ""
    echo "📊 Commits poussés :"
    git log origin/dev..HEAD --oneline 2>/dev/null || git log --oneline -3
    echo ""
    echo "🌐 URL de la branche dev :"
    REMOTE_URL=$(git remote get-url origin)
    if [[ "$REMOTE_URL" =~ https://github.com/([^/]+)/([^/]+) ]]; then
        USERNAME="${BASH_REMATCH[1]}"
        REPO="${BASH_REMATCH[2]%.git}"
        echo "   https://github.com/${USERNAME}/${REPO}/tree/dev"
    elif [[ "$REMOTE_URL" =~ git@github.com:([^/]+)/([^/]+) ]]; then
        USERNAME="${BASH_REMATCH[1]}"
        REPO="${BASH_REMATCH[2]%.git}"
        echo "   https://github.com/${USERNAME}/${REPO}/tree/dev"
    fi
else
    echo ""
    echo "❌ Erreur lors du push"
    echo ""
    echo "Vérifiez :"
    echo "  1. Que le dépôt 'cursor' existe sur GitHub"
    echo "  2. Vos identifiants GitHub (token ou SSH)"
    exit 1
fi
