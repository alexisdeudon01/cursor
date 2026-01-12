#!/bin/bash
# Script pour configurer la clé API Anthropic dans GitHub Secrets
# Nécessite GitHub CLI (gh) installé

set -e

API_KEY="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"
REPO="alexisdeudon01/cursor"
SECRET_NAME="ANTHROPIC_API_KEY"

echo "🔐 Configuration de la clé API Anthropic dans GitHub Secrets..."

# Vérifier si gh est installé
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI (gh) n'est pas installé"
    echo "📝 Installation: https://cli.github.com/"
    echo ""
    echo "💡 Alternative: Utilisez l'interface GitHub web:"
    echo "   1. https://github.com/$REPO/settings/secrets/actions"
    echo "   2. New repository secret"
    echo "   3. Name: $SECRET_NAME"
    echo "   4. Secret: $API_KEY"
    exit 1
fi

# Vérifier si connecté
if ! gh auth status &> /dev/null; then
    echo "❌ Pas connecté à GitHub CLI"
    echo "🔐 Connexion: gh auth login"
    exit 1
fi

# Ajouter le secret
echo "📝 Ajout du secret $SECRET_NAME..."
echo "$API_KEY" | gh secret set "$SECRET_NAME" --repo "$REPO"

echo "✅ Secret $SECRET_NAME ajouté avec succès!"
echo ""
echo "🔍 Vérification:"
gh secret list --repo "$REPO" | grep "$SECRET_NAME" || echo "⚠️ Secret non trouvé (peut prendre quelques secondes)"

echo ""
echo "✨ Prochaine étape:"
echo "   Le workflow GitHub Actions utilisera automatiquement cette clé"
echo "   pour activer l'IA Claude dans les améliorations continues."
