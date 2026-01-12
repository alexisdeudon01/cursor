#!/bin/bash
# Script simple - Fait TOUT sans erreur

set +e  # Continuer même en cas d'erreur

echo "🚀 Setup complet - Version simple"
echo "=================================="
echo ""

# Aller dans le projet
cd /home/tor/wkspaces/mo2 2>/dev/null || {
    echo "⚠️  Dossier /home/tor/wkspaces/mo2 non trouvé"
    echo "   Utilisation du dossier actuel: $(pwd)"
}

echo "📁 Dossier: $(pwd)"
echo ""

# 1. Git
echo "📦 1. Git..."
if [ -d ".git" ]; then
    echo "   ✅ Repository Git"
    git fetch origin 2>&1 | head -3
    git checkout dev 2>/dev/null || echo "   ⚠️  Checkout dev"
    git pull origin dev 2>&1 | head -3 || echo "   ⚠️  Pull"
else
    echo "   ❌ Pas de Git - Initialisation..."
    git init
    git remote add origin https://github.com/alexisdeudon01/cursor.git 2>/dev/null || true
fi

# 2. KEYS.txt
echo ""
echo "🔑 2. KEYS.txt..."
mkdir -p .github
if [ -f ".github/KEYS.txt" ]; then
    echo "   ✅ Existe déjà"
else
    cat > .github/KEYS.txt << 'KEYS_EOF'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
KEYS_EOF
    echo "   ✅ Créé"
fi

# 3. .gitignore
echo ""
echo "🚫 3. .gitignore..."
if grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo "   ✅ Déjà dans .gitignore"
else
    echo ".github/KEYS.txt" >> .gitignore
    echo "   ✅ Ajouté"
fi

# 4. Vérification fichiers
echo ""
echo "📄 4. Fichiers..."
[ -f ".cursor/agents/thebestclient5.md" ] && echo "   ✅ Agent Thebestclient5" || echo "   ⚠️  Agent manquant"
[ -f "setup-complete.sh" ] && echo "   ✅ setup-complete.sh" || echo "   ⚠️  setup-complete.sh manquant"
[ -f "cleanup-branches.sh" ] && echo "   ✅ cleanup-branches.sh" || echo "   ⚠️  cleanup-branches.sh manquant"

# 5. Résumé
echo ""
echo "=================================="
echo "📊 RÉSUMÉ"
echo "=================================="
echo "Branche: $(git branch --show-current 2>/dev/null || echo 'inconnue')"
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo ""
echo "✅ Setup terminé!"
echo ""
echo "📋 Prochaine étape:"
echo "   Ajouter ANTHROPIC_API_KEY dans GitHub Secrets:"
echo "   https://github.com/alexisdeudon01/cursor/settings/secrets/actions"
echo ""
