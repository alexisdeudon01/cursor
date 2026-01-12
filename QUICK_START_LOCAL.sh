#!/bin/bash
# Script rapide pour setup local - Copier-coller et exécuter

set -e

echo "🚀 Setup rapide local..."

# Aller dans le projet
cd /home/tor/wkspaces/mo2 || { echo "❌ Dossier non trouvé"; exit 1; }

# Récupérer les changements
echo "📥 Récupération des changements..."
git fetch origin
git checkout dev
git pull origin dev || echo "⚠️ Pull échoué, continuons..."

# Créer KEYS.txt
echo "🔑 Création KEYS.txt..."
mkdir -p .github
cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
EOF

# Vérifier .gitignore
if ! grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo ".github/KEYS.txt" >> .gitignore
fi

# Rendre scripts exécutables
chmod +x setup-complete.sh cleanup-branches.sh 2>/dev/null || true

# Vérifications
echo ""
echo "✅ Vérifications:"
echo "  Branche: $(git branch --show-current)"
echo "  KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "  Agent: $(test -f .cursor/agents/thebestclient5.md && echo '✅' || echo '❌')"
echo "  Scripts: $(ls -1 setup-complete.sh cleanup-branches.sh 2>/dev/null | wc -l)"

echo ""
echo "✅ Setup terminé!"
echo ""
echo "📋 Prochaine étape:"
echo "  Ajouter ANTHROPIC_API_KEY dans GitHub Secrets:"
echo "  https://github.com/alexisdeudon01/cursor/settings/secrets/actions"
