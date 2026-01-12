#!/bin/bash
# Script complet pour setup la branche dev avec tout le nécessaire

set -e

echo "🚀 Setup complet de la branche dev..."

# 1. Créer le fichier KEYS.txt
echo "📝 Création du fichier KEYS.txt..."
mkdir -p .github

cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
# Ce fichier contient les clés d'accès

# Clé API Anthropic
ANTHROPIC_API_KEY=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

# Clé SSH Privée
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=

# Clé SSH Publique
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com

# Passphrase SSH
SSH_PASSPHRASE=alexis
EOF

echo "✅ KEYS.txt créé"

# 2. Vérifier que .gitignore contient KEYS.txt
if ! grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo ".github/KEYS.txt" >> .gitignore
    echo "✅ .gitignore mis à jour"
fi

# 3. Vérifier que le fichier existe
if [ -f ".github/KEYS.txt" ]; then
    echo "✅ Fichier KEYS.txt vérifié: $(wc -l < .github/KEYS.txt) lignes"
else
    echo "❌ Erreur: KEYS.txt non créé"
    exit 1
fi

# 4. Vérifier la branche
CURRENT_BRANCH=$(git branch --show-current)
echo "📍 Branche actuelle: $CURRENT_BRANCH"

# 5. Afficher les instructions
echo ""
echo "✅ Setup terminé!"
echo ""
echo "📋 Prochaines étapes:"
echo "1. Ajouter la clé API dans GitHub Secrets:"
echo "   https://github.com/alexisdeudon01/cursor/settings/secrets/actions"
echo "2. Name: ANTHROPIC_API_KEY"
echo "3. Secret: (voir .github/KEYS.txt)"
echo ""
echo "Le système fonctionnera automatiquement une fois la clé ajoutée!"
