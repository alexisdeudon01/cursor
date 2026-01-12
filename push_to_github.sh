#!/bin/bash
# Script pour configurer le remote et pousser vers GitHub

echo "=== Configuration et Push vers GitHub ==="
echo ""

# Vérifier si le remote existe
if git remote get-url origin &> /dev/null; then
    CURRENT_URL=$(git remote get-url origin)
    echo "Remote actuel : $CURRENT_URL"
    
    # Vérifier si c'est le placeholder
    if [[ "$CURRENT_URL" == *"VOTRE_USERNAME"* ]]; then
        echo ""
        read -p "Entrez votre nom d'utilisateur GitHub : " GITHUB_USERNAME
        
        if [ -z "$GITHUB_USERNAME" ]; then
            echo "❌ Nom d'utilisateur requis"
            exit 1
        fi
        
        # Mettre à jour le remote
        echo "🔗 Mise à jour du remote..."
        git remote set-url origin "https://github.com/${GITHUB_USERNAME}/cursor.git"
        echo "✅ Remote mis à jour : https://github.com/${GITHUB_USERNAME}/cursor.git"
    else
        GITHUB_USERNAME=$(echo "$CURRENT_URL" | sed -n 's|https://github.com/\([^/]*\)/.*|\1|p')
        echo "✅ Utilisation du remote existant pour : $GITHUB_USERNAME"
    fi
else
    echo "❌ Aucun remote 'origin' configuré"
    read -p "Entrez votre nom d'utilisateur GitHub : " GITHUB_USERNAME
    
    if [ -z "$GITHUB_USERNAME" ]; then
        echo "❌ Nom d'utilisateur requis"
        exit 1
    fi
    
    git remote add origin "https://github.com/${GITHUB_USERNAME}/cursor.git"
    echo "✅ Remote ajouté"
fi

echo ""
echo "🚀 Push vers GitHub..."
echo ""

# Essayer le push
if git push -u origin branché-1 2>&1; then
    echo ""
    echo "✅ Succès ! Votre dépôt est maintenant sur GitHub"
    echo "🌐 URL : https://github.com/${GITHUB_USERNAME}/cursor"
    echo ""
    echo "💡 GitLens dans VS Code détectera automatiquement ce dépôt"
else
    echo ""
    echo "⚠️  Le push a échoué. Raisons possibles :"
    echo ""
    echo "1. Le dépôt 'cursor' n'existe pas encore sur GitHub"
    echo "   → Créez-le sur : https://github.com/new"
    echo "   → Nom : cursor"
    echo "   → NE cochez PAS 'Initialize with README'"
    echo ""
    echo "2. Problème d'authentification"
    echo "   → Utilisez un Personal Access Token au lieu du mot de passe"
    echo "   → Créez un token sur : https://github.com/settings/tokens"
    echo "   → Scopes nécessaires : repo"
    echo "   → Utilisez le token comme mot de passe lors du push"
    echo ""
    echo "3. Alternative : Utiliser SSH"
    echo "   → Configurez une clé SSH : https://docs.github.com/en/authentication/connecting-to-github-with-ssh"
    echo "   → Puis : git remote set-url origin git@github.com:${GITHUB_USERNAME}/cursor.git"
    echo "   → Et relancez : git push -u origin branché-1"
fi
