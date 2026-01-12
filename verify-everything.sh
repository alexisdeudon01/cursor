#!/bin/bash
# Script complet de vérification - Vérifie TOUT

set +e  # Ne pas arrêter sur erreur pour continuer les vérifications

echo "🔍 VÉRIFICATION COMPLÈTE DU PROJET"
echo "=================================="
echo ""

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Détecter automatiquement le dossier du projet
if [ -n "$1" ]; then
    PROJECT_DIR="$1"
else
    # Essayer plusieurs chemins possibles
    if [ -d "/home/tor/wkspaces/mo2" ]; then
        PROJECT_DIR="/home/tor/wkspaces/mo2"
    elif [ -d "$(pwd)" ] && [ -d "$(pwd)/.git" ]; then
        PROJECT_DIR="$(pwd)"
    else
        PROJECT_DIR="/home/tor/wkspaces/mo2"
    fi
fi

# Fonction de vérification
check() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ $1${NC}"
        return 0
    else
        echo -e "${RED}❌ $1${NC}"
        return 1
    fi
}

# 1. Vérifier le dossier
echo "📁 1. Vérification du dossier..."
echo "   Chemin testé: $PROJECT_DIR"

if [ -d "$PROJECT_DIR" ]; then
    check "Dossier existe: $PROJECT_DIR"
    cd "$PROJECT_DIR" || {
        echo -e "${RED}❌ Impossible d'accéder au dossier${NC}"
        echo "   Essaie avec le dossier actuel..."
        PROJECT_DIR="$(pwd)"
    }
else
    echo -e "${YELLOW}⚠️  Dossier non trouvé: $PROJECT_DIR${NC}"
    echo "   Utilisation du dossier actuel: $(pwd)"
    PROJECT_DIR="$(pwd)"
    cd "$PROJECT_DIR" || {
        echo -e "${RED}❌ Erreur: Impossible d'accéder au dossier${NC}"
        exit 1
    }
fi

# 2. Vérifier Git
echo ""
echo "📦 2. Vérification Git..."
if [ -d ".git" ]; then
    check "Repository Git initialisé"
    
    # Branche actuelle
    CURRENT_BRANCH=$(git branch --show-current 2>/dev/null || echo "aucune")
    echo "   Branche actuelle: $CURRENT_BRANCH"
    
    # Remote
    if git remote get-url origin >/dev/null 2>&1; then
        REMOTE_URL=$(git remote get-url origin)
        echo -e "${GREEN}   ✅ Remote configuré: $REMOTE_URL${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Remote non configuré${NC}"
        echo "   Configuration du remote..."
        git remote add origin https://github.com/alexisdeudon01/cursor.git 2>/dev/null || true
    fi
    
    # Branches
    echo ""
    echo "   Branches locales:"
    git branch 2>/dev/null | sed 's/^/      /'
    
    echo ""
    echo "   Branches distantes:"
    git branch -r 2>/dev/null | sed 's/^/      /' || echo "      Aucune"
    
else
    echo -e "${RED}❌ Pas de repository Git${NC}"
    echo "Initialisation Git..."
    git init
    git remote add origin https://github.com/alexisdeudon01/cursor.git
    echo -e "${GREEN}✅ Git initialisé${NC}"
fi

# 3. Récupérer les changements
echo ""
echo "📥 3. Récupération des changements..."
git fetch origin --all 2>&1 | head -10 || echo "   ⚠️  Fetch échoué (peut être normal si première fois)"

# 4. Vérifier/Créer branche dev
echo ""
echo "🌿 4. Vérification branche dev..."
if git show-ref --verify --quiet refs/heads/dev; then
    echo -e "${GREEN}   ✅ Branche dev existe localement${NC}"
    git checkout dev 2>/dev/null || echo "   ⚠️  Impossible de checkout dev"
else
    echo "   Création branche dev..."
    if git show-ref --verify --quiet refs/remotes/origin/dev; then
        git checkout -b dev origin/dev 2>/dev/null || git checkout -b dev
    else
        git checkout -b dev
    fi
    check "Branche dev créée"
fi

# 5. Pull les changements
echo ""
echo "⬇️  5. Récupération des fichiers..."
git pull origin dev 2>&1 | head -10 || echo "   ⚠️  Pull échoué (peut être normal)"

# 6. Vérifier les fichiers clés
echo ""
echo "📄 6. Vérification des fichiers..."

# Agent Thebestclient5
if [ -f ".cursor/agents/thebestclient5.md" ]; then
    check "Agent Thebestclient5 existe"
    echo "   Taille: $(wc -l < .cursor/agents/thebestclient5.md) lignes"
else
    echo -e "${YELLOW}   ⚠️  Agent Thebestclient5 manquant${NC}"
    echo "   Création du dossier..."
    mkdir -p .cursor/agents
fi

# KEYS.txt
if [ -f ".github/KEYS.txt" ]; then
    check "KEYS.txt existe"
    echo "   Taille: $(wc -l < .github/KEYS.txt) lignes"
else
    echo -e "${YELLOW}   ⚠️  KEYS.txt manquant - Création...${NC}"
    mkdir -p .github
    cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
EOF
    check "KEYS.txt créé"
fi

# Scripts
SCRIPTS_OK=0
[ -f "setup-complete.sh" ] && SCRIPTS_OK=$((SCRIPTS_OK + 1))
[ -f "cleanup-branches.sh" ] && SCRIPTS_OK=$((SCRIPTS_OK + 1))

if [ $SCRIPTS_OK -eq 2 ]; then
    check "Scripts setup existent"
else
    echo -e "${YELLOW}   ⚠️  Scripts manquants ($SCRIPTS_OK/2)${NC}"
fi

# 7. Vérifier .gitignore
echo ""
echo "🚫 7. Vérification .gitignore..."
if grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    check "KEYS.txt dans .gitignore"
else
    echo "   Ajout de KEYS.txt dans .gitignore..."
    echo ".github/KEYS.txt" >> .gitignore
    check "KEYS.txt ajouté à .gitignore"
fi

# 8. Vérifier les commits
echo ""
echo "📜 8. Vérification des commits..."
COMMIT_COUNT=$(git log --oneline 2>/dev/null | wc -l)
if [ "$COMMIT_COUNT" -gt 0 ]; then
    echo -e "${GREEN}   ✅ $COMMIT_COUNT commit(s) trouvé(s)${NC}"
    echo "   Derniers commits:"
    git log --oneline -5 2>/dev/null | sed 's/^/      /' || echo "      Aucun"
else
    echo -e "${YELLOW}   ⚠️  Aucun commit trouvé${NC}"
fi

# 9. Vérifier les fichiers dans le repo
echo ""
echo "📋 9. Liste des fichiers importants..."
echo "   Agents:"
ls -1 .cursor/agents/thebestclient*.md 2>/dev/null | sed 's/^/      /' || echo "      Aucun"
echo "   Scripts:"
ls -1 *.sh 2>/dev/null | sed 's/^/      /' || echo "      Aucun"
echo "   GitHub:"
ls -1 .github/*.sh .github/*.py 2>/dev/null | sed 's/^/      /' || echo "      Aucun"

# 10. Vérifier la connexion GitHub
echo ""
echo "🌐 10. Vérification connexion GitHub..."
if git ls-remote origin >/dev/null 2>&1; then
    check "Connexion GitHub OK"
    echo "   Branches distantes disponibles:"
    git ls-remote --heads origin 2>/dev/null | sed 's/.*refs\/heads\///' | sed 's/^/      /' || echo "      Aucune"
else
    echo -e "${RED}   ❌ Impossible de se connecter à GitHub${NC}"
    echo "   Vérifie ta connexion internet et tes credentials Git"
fi

# 11. Résumé final
echo ""
echo "=================================="
echo "📊 RÉSUMÉ FINAL"
echo "=================================="
echo ""
echo "Branche: $(git branch --show-current 2>/dev/null || echo 'inconnue')"
echo "Remote: $(git remote get-url origin 2>/dev/null || echo 'non configuré')"
echo "Commits: $COMMIT_COUNT"
echo ""
echo "Fichiers:"
echo "  - KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "  - Agent Thebestclient5: $(test -f .cursor/agents/thebestclient5.md && echo '✅' || echo '❌')"
echo "  - setup-complete.sh: $(test -f setup-complete.sh && echo '✅' || echo '❌')"
echo "  - cleanup-branches.sh: $(test -f cleanup-branches.sh && echo '✅' || echo '❌')"
echo ""
echo "=================================="
echo ""

# 12. Actions recommandées
echo "📋 ACTIONS RECOMMANDÉES:"
echo ""
if [ ! -f ".github/KEYS.txt" ]; then
    echo "1. KEYS.txt manquant - Voir section création ci-dessus"
fi

if ! git ls-remote origin >/dev/null 2>&1; then
    echo "2. ⚠️  Problème de connexion GitHub - Vérifie tes credentials"
fi

if [ "$COMMIT_COUNT" -eq 0 ]; then
    echo "3. ⚠️  Aucun commit - Le repo est peut-être vide"
    echo "   Essaie: git pull origin dev --allow-unrelated-histories"
fi

echo ""
echo "4. Ajouter ANTHROPIC_API_KEY dans GitHub Secrets:"
echo "   https://github.com/alexisdeudon01/cursor/settings/secrets/actions"
echo ""

# 13. Créer les fichiers manquants si nécessaire
echo "🔧 Création des fichiers manquants si nécessaire..."

# Créer structure de dossiers
mkdir -p .cursor/agents
mkdir -p .github/scripts
mkdir -p .github/workflows

# Si agent manquant, télécharger depuis GitHub ou créer de base
if [ ! -f ".cursor/agents/thebestclient5.md" ]; then
    echo "   Tentative de récupération thebestclient5.md..."
    # Essayer plusieurs méthodes
    if git show origin/dev:.cursor/agents/thebestclient5.md > .cursor/agents/thebestclient5.md 2>/dev/null; then
        echo -e "${GREEN}   ✅ Récupéré depuis Git${NC}"
    elif curl -s -f https://raw.githubusercontent.com/alexisdeudon01/cursor/dev/.cursor/agents/thebestclient5.md > .cursor/agents/thebestclient5.md 2>/dev/null; then
        echo -e "${GREEN}   ✅ Téléchargé depuis GitHub${NC}"
    else
        echo "   Création thebestclient5.md de base..."
        cat > .cursor/agents/thebestclient5.md << 'AGENT_EOF'
---
name: Thebestclient5
description: Agent AI v5 - 50% LLM jeux 2D + 50% amélioration code
model: default
readonly: false
---

# Agent Thebestclient5

Agent d'amélioration continue avec entraînement LLM pour jeux 2D.
Voir le fichier complet sur GitHub une fois le repo synchronisé.
AGENT_EOF
        echo -e "${YELLOW}   ⚠️  Fichier de base créé (télécharger depuis GitHub plus tard)${NC}"
    fi
fi

# Si scripts manquants, les créer localement
if [ ! -f "setup-complete.sh" ]; then
    echo "   Création setup-complete.sh localement..."
    cat > setup-complete.sh << 'SETUP_EOF'
#!/bin/bash
# Script complet pour setup la branche dev avec tout le nécessaire
set -e
echo "🚀 Setup complet de la branche dev..."
mkdir -p .github
cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
EOF
if ! grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo ".github/KEYS.txt" >> .gitignore
fi
echo "✅ Setup terminé!"
SETUP_EOF
    chmod +x setup-complete.sh
    echo -e "${GREEN}   ✅ setup-complete.sh créé${NC}"
fi

echo ""
echo "✅ Vérification terminée!"
echo ""
echo "📝 Prochaine étape:"
echo "   Vérifie les résultats ci-dessus et corrige les problèmes identifiés"
