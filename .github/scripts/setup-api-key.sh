#!/bin/bash
# Script pour configurer la clé API Anthropic dans GitHub Secrets
# Nécessite GitHub CLI (gh) installé

set -e

# ⚠️ REMPLACER PAR TA CLÉ API (ne jamais commiter la clé réelle)
API_KEY="${ANTHROPIC_API_KEY:-[TA_CLÉ_API_ICI]}"
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

# Vérifier que la clé n'est pas un placeholder
if [ "$API_KEY" = "[TA_CLÉ_API_ICI]" ] || [ -z "$API_KEY" ]; then
    echo "❌ Clé API non configurée"
    echo "💡 Utilisez: ANTHROPIC_API_KEY='ta-cle' ./setup-api-key.sh"
    echo "   Ou modifiez le script pour mettre ta clé (localement, ne pas commiter)"
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
