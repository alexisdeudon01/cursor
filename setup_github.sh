#!/bin/bash
# Script pour configurer GitHub et pousser le dépôt

echo "=== Configuration GitHub pour le projet mo2 ==="
echo ""

# Vérifier si GitHub CLI est installé
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI n'est pas installé."
    echo ""
    echo "Pour installer GitHub CLI sur Linux, exécutez :"
    echo "  curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg"
    echo "  sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg"
    echo "  echo 'deb [arch=\$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main' | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null"
    echo "  sudo apt update"
    echo "  sudo apt install gh -y"
    echo ""
    echo "Ou visitez : https://github.com/cli/cli/blob/trunk/docs/install_linux.md"
    exit 1
fi

echo "✅ GitHub CLI est installé"
echo ""

# Vérifier si l'utilisateur est connecté
if ! gh auth status &> /dev/null; then
    echo "🔐 Connexion à GitHub requise..."
    echo "Exécutez : gh auth login"
    echo "Suivez les instructions pour vous connecter à votre compte GitHub"
    exit 1
fi

echo "✅ Connecté à GitHub"
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

# Créer le dépôt sur GitHub
echo "📦 Création du dépôt 'cursor' sur GitHub..."
gh repo create cursor --public --source=. --remote=origin --push=false

if [ $? -ne 0 ]; then
    echo "❌ Erreur lors de la création du dépôt"
    echo "Le dépôt 'cursor' existe peut-être déjà sur votre compte GitHub"
    echo "Vous pouvez le créer manuellement sur https://github.com/new"
    exit 1
fi

echo "✅ Dépôt créé avec succès"
echo ""

# Ajouter tous les fichiers
echo "📝 Ajout des fichiers..."
git add .

# Faire un commit initial
echo "💾 Création du commit initial..."
git commit -m "Initial commit: Unity multiplayer game project"

# Pousser vers GitHub
echo "🚀 Push vers GitHub..."
git push -u origin branché-1

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Succès ! Votre dépôt est maintenant sur GitHub"
    echo "🌐 URL du dépôt : $(gh repo view cursor --json url -q .url)"
    echo ""
    echo "📌 Branche actuelle : branché-1"
    echo ""
    echo "💡 GitLens dans VS Code détectera automatiquement ce dépôt Git"
else
    echo "❌ Erreur lors du push"
    exit 1
fi
