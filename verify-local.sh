#!/bin/bash
# Script de vérification 100% local - Pas de téléchargement depuis GitHub

set +e  # Ne pas arrêter sur erreur

echo "🔍 VÉRIFICATION LOCALE COMPLÈTE"
echo "================================"
echo ""

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Détecter le dossier
if [ -d "/home/tor/wkspaces/mo2" ]; then
    cd /home/tor/wkspaces/mo2
elif [ -d "$(pwd)/.git" ]; then
    cd "$(pwd)"
else
    echo -e "${RED}❌ Dossier projet non trouvé${NC}"
    echo "Exécute ce script depuis le dossier du projet"
    exit 1
fi

echo "📁 Dossier: $(pwd)"
echo ""

# 1. Vérifier Git
echo "📦 1. Git..."
if [ -d ".git" ]; then
    echo -e "${GREEN}✅ Repository Git${NC}"
    echo "   Branche: $(git branch --show-current 2>/dev/null || echo 'inconnue')"
    echo "   Remote: $(git remote get-url origin 2>/dev/null | head -c 50 || echo 'non configuré')..."
else
    echo -e "${RED}❌ Pas de Git${NC}"
fi

# 2. Récupérer depuis GitHub
echo ""
echo "📥 2. Récupération depuis GitHub..."
git fetch origin 2>&1 | head -5 || echo "   ⚠️  Fetch échoué"
git checkout dev 2>/dev/null || echo "   ⚠️  Checkout dev échoué"
git pull origin dev 2>&1 | head -5 || echo "   ⚠️  Pull échoué"

# 3. Créer KEYS.txt
echo ""
echo "🔑 3. KEYS.txt..."
mkdir -p .github
if [ -f ".github/KEYS.txt" ]; then
    echo -e "${GREEN}✅ KEYS.txt existe${NC}"
else
    echo "   Création KEYS.txt..."
    cat > .github/KEYS.txt << 'KEYS_EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
KEYS_EOF
    echo -e "${GREEN}✅ KEYS.txt créé${NC}"
fi

# 4. Vérifier .gitignore
echo ""
echo "🚫 4. .gitignore..."
if grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo -e "${GREEN}✅ KEYS.txt dans .gitignore${NC}"
else
    echo ".github/KEYS.txt" >> .gitignore
    echo -e "${GREEN}✅ KEYS.txt ajouté à .gitignore${NC}"
fi

# 5. Vérifier fichiers
echo ""
echo "📄 5. Fichiers..."
FILES_OK=0
[ -f ".cursor/agents/thebestclient5.md" ] && FILES_OK=$((FILES_OK + 1)) && echo -e "${GREEN}✅ Agent Thebestclient5${NC}" || echo -e "${YELLOW}⚠️  Agent manquant${NC}"
[ -f "setup-complete.sh" ] && FILES_OK=$((FILES_OK + 1)) && echo -e "${GREEN}✅ setup-complete.sh${NC}" || echo -e "${YELLOW}⚠️  setup-complete.sh manquant${NC}"
[ -f "cleanup-branches.sh" ] && FILES_OK=$((FILES_OK + 1)) && echo -e "${GREEN}✅ cleanup-branches.sh${NC}" || echo -e "${YELLOW}⚠️  cleanup-branches.sh manquant${NC}"

# 6. Résumé
echo ""
echo "================================"
echo "📊 RÉSUMÉ"
echo "================================"
echo "Branche: $(git branch --show-current 2>/dev/null || echo 'inconnue')"
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "Fichiers: $FILES_OK/3"
echo ""
echo "📋 ACTION REQUISE:"
echo "   Ajouter ANTHROPIC_API_KEY dans GitHub Secrets:"
echo "   https://github.com/alexisdeudon01/cursor/settings/secrets/actions"
echo ""
