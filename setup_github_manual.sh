#!/bin/bash
# Script alternatif pour configurer GitHub sans GitHub CLI

echo "=== Configuration GitHub (méthode manuelle) ==="
echo ""

# Vérifier si le remote existe déjà
if git remote get-url origin &> /dev/null; then
    echo "⚠️  Un remote 'origin' existe déjà :"
    git remote get-url origin
    read -p "Voulez-vous le remplacer ? (o/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Oo]$ ]]; then
        git remote remove origin
    else
        echo "Annulé."
        exit 1
    fi
fi

echo "📝 Pour créer le dépôt 'cursor' sur GitHub :"
echo ""
echo "Option 1 - Via l'interface web GitHub :"
echo "  1. Allez sur https://github.com/new"
echo "  2. Nom du dépôt : cursor"
echo "  3. Choisissez Public ou Private"
echo "  4. NE cochez PAS 'Initialize with README'"
echo "  5. Cliquez sur 'Create repository'"
echo ""
echo "Option 2 - Via l'API GitHub (si vous avez un token) :"
echo "  curl -X POST -H 'Authorization: token VOTRE_TOKEN' \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -d '{\"name\":\"cursor\",\"private\":false}' \\"
echo "    https://api.github.com/user/repos"
echo ""

read -p "Avez-vous créé le dépôt 'cursor' sur GitHub ? (o/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Oo]$ ]]; then
    echo "Veuillez créer le dépôt d'abord, puis relancez ce script."
    exit 1
fi

# Demander l'URL du dépôt
echo ""
read -p "Entrez votre nom d'utilisateur GitHub : " GITHUB_USERNAME
echo ""

if [ -z "$GITHUB_USERNAME" ]; then
    echo "❌ Nom d'utilisateur requis"
    exit 1
fi

# Ajouter le remote
echo "🔗 Ajout du remote GitHub..."
git remote add origin "https://github.com/${GITHUB_USERNAME}/cursor.git"

if [ $? -ne 0 ]; then
    echo "❌ Erreur lors de l'ajout du remote"
    exit 1
fi

echo "✅ Remote ajouté"
echo ""

# Pousser vers GitHub
echo "🚀 Push vers GitHub..."
echo "Vous devrez peut-être entrer vos identifiants GitHub"
git push -u origin branché-1

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Succès ! Votre dépôt est maintenant sur GitHub"
    echo "🌐 URL du dépôt : https://github.com/${GITHUB_USERNAME}/cursor"
    echo ""
    echo "📌 Branche actuelle : branché-1"
    echo ""
    echo "💡 GitLens dans VS Code détectera automatiquement ce dépôt Git"
else
    echo ""
    echo "❌ Erreur lors du push"
    echo ""
    echo "Si vous avez des problèmes d'authentification :"
    echo "  1. Utilisez un Personal Access Token au lieu du mot de passe"
    echo "  2. Créez un token sur : https://github.com/settings/tokens"
    echo "  3. Utilisez le token comme mot de passe lors du push"
    echo ""
    echo "Ou configurez SSH :"
    echo "  ssh-keygen -t ed25519 -C 'votre_email@example.com'"
    echo "  cat ~/.ssh/id_ed25519.pub"
    echo "  # Ajoutez la clé sur https://github.com/settings/keys"
    echo "  git remote set-url origin git@github.com:${GITHUB_USERNAME}/cursor.git"
    echo "  git push -u origin branché-1"
fi
