#!/bin/bash
# Script pour fixer le push bloqué par GitHub

set -e

echo "🔧 Fix Push Bloqué par GitHub"
echo "=============================="
echo ""

cd /home/tor/wkspaces/mo2 2>/dev/null || {
    echo "⚠️  Dossier non trouvé, utilisation du dossier actuel"
    cd "$(pwd)"
}

# 1. Retirer KEYS.txt de Git
echo "1. Retrait KEYS.txt de Git..."
git rm --cached .github/KEYS.txt 2>/dev/null || echo "   ⚠️  KEYS.txt pas dans Git (déjà retiré?)"

# 2. S'assurer qu'il est dans .gitignore
echo "2. Vérification .gitignore..."
if ! grep -q "KEYS.txt" .gitignore 2>/dev/null; then
    echo ".github/KEYS.txt" >> .gitignore
    echo "   ✅ Ajouté à .gitignore"
else
    echo "   ✅ Déjà dans .gitignore"
fi

# 3. Commit
echo "3. Commit..."
git add .gitignore 2>/dev/null || true
if ! git diff --cached --quiet 2>/dev/null; then
    git commit -m "🔐 Retrait KEYS.txt de Git (dans .gitignore)"
    echo "   ✅ Commité"
else
    echo "   ⚠️  Rien à commiter"
fi

# 4. Vérifier que KEYS.txt n'est plus dans Git
echo "4. Vérification..."
if git ls-files | grep -q "KEYS.txt"; then
    echo "   ❌ KEYS.txt encore dans Git!"
    echo "   Nettoyage de l'historique nécessaire"
else
    echo "   ✅ KEYS.txt n'est plus dans Git"
fi

# 5. Vérifier que le fichier existe localement
if [ -f ".github/KEYS.txt" ]; then
    echo "   ✅ KEYS.txt existe localement (bon)"
else
    echo "   ⚠️  KEYS.txt manquant localement - Création..."
    mkdir -p .github
    cat > .github/KEYS.txt << 'KEYS_EOF'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
KEYS_EOF
fi

echo ""
echo "=================================="
echo "📊 RÉSUMÉ"
echo "=================================="
echo "KEYS.txt dans Git: $(git ls-files | grep -q 'KEYS.txt' && echo '❌ OUI' || echo '✅ NON')"
echo "KEYS.txt local: $(test -f .github/KEYS.txt && echo '✅ OUI' || echo '❌ NON')"
echo "KEYS.txt dans .gitignore: $(grep -q 'KEYS.txt' .gitignore && echo '✅ OUI' || echo '❌ NON')"
echo ""
echo "📋 Prochaine étape:"
echo "   Si KEYS.txt n'est plus dans Git: git push origin dev"
echo "   Si toujours bloqué: Utiliser le lien GitHub pour autoriser OU nettoyer l'historique"
echo ""
