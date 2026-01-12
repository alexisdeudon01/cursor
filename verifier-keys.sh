#!/bin/bash
# Script de vérification KEYS.txt
# À exécuter depuis /home/tor/wkspaces/mo2

cd /home/tor/wkspaces/mo2 || exit 1

echo "📊 Vérification KEYS.txt"
echo "========================="
echo ""

# 1. Vérifier que KEYS.txt n'est plus dans Git
echo "1. KEYS.txt dans Git:"
if git ls-files | grep -q KEYS.txt; then
    echo "   ❌ TROUVÉ (problème!)"
    echo "   → Exécuter: git rm --cached .github/KEYS.txt"
else
    echo "   ✅ NON TROUVÉ (parfait!)"
fi
echo ""

# 2. Vérifier que KEYS.txt existe localement
echo "2. KEYS.txt local:"
if [ -f .github/KEYS.txt ]; then
    echo "   ✅ EXISTE ($(wc -l < .github/KEYS.txt) lignes)"
    echo "   → Chemin: $(realpath .github/KEYS.txt)"
else
    echo "   ❌ MANQUANT"
    echo "   → Créer le fichier si nécessaire"
fi
echo ""

# 3. Vérifier .gitignore
echo "3. .gitignore:"
if grep -q "KEYS.txt" .gitignore; then
    echo "   ✅ DANS .gitignore"
else
    echo "   ❌ PAS dans .gitignore"
    echo "   → Ajouter: .github/KEYS.txt"
fi
echo ""

# Résumé
echo "========================="
echo "✅✅✅ RÉSUMÉ:"
echo "Branche: $(git branch --show-current)"
echo "KEYS.txt dans Git: $(git ls-files | grep -q 'KEYS.txt' && echo '❌ OUI' || echo '✅ NON')"
echo "KEYS.txt local: $(test -f .github/KEYS.txt && echo '✅ OUI' || echo '❌ NON')"
echo ".gitignore: $(grep -q 'KEYS.txt' .gitignore && echo '✅ OUI' || echo '❌ NON')"
echo ""
echo "🎉 Si tout est ✅, KEYS.txt est LOCAL UNIQUEMENT!"
