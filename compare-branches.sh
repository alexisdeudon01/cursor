#!/bin/bash
#===============================================================================
#  SCRIPT DE COMPARAISON MAIN vs DEV
#  Génère un rapport complet des différences entre les deux branches
#===============================================================================

REPO_PATH="/home/tor/wkspaces/mo2"
OUTPUT_FILE="${REPO_PATH}/diff-report-$(date +%Y%m%d-%H%M%S).md"

cd "$REPO_PATH" || exit 1

echo "🔍 Analyse des différences main ↔ dev..."
echo ""

# Créer le rapport
cat > "$OUTPUT_FILE" << 'HEADER'
# Rapport de comparaison Main ↔ Dev

**Date**: REPORT_DATE
**Repository**: alexisdeudon01/cursor

---

HEADER

# Remplacer la date
sed -i "s/REPORT_DATE/$(date '+%Y-%m-%d %H:%M:%S')/" "$OUTPUT_FILE"

# Section 1: Résumé des fichiers modifiés
cat >> "$OUTPUT_FILE" << 'EOF'
## 1. Résumé des fichiers modifiés

EOF

echo '```' >> "$OUTPUT_FILE"
git diff --stat main dev >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 2: Liste des fichiers
cat >> "$OUTPUT_FILE" << 'EOF'
## 2. Liste des fichiers différents

EOF

echo '```' >> "$OUTPUT_FILE"
git diff --name-status main dev >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 3: Différences .github/workflows
cat >> "$OUTPUT_FILE" << 'EOF'
## 3. Différences Workflows (.github/workflows/)

EOF

echo '```diff' >> "$OUTPUT_FILE"
git diff main dev -- .github/workflows/ >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 4: Différences .github/scripts
cat >> "$OUTPUT_FILE" << 'EOF'
## 4. Différences Scripts (.github/scripts/)

EOF

echo '```diff' >> "$OUTPUT_FILE"
git diff main dev -- .github/scripts/ >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 5: Différences Assets
cat >> "$OUTPUT_FILE" << 'EOF'
## 5. Différences Assets (Assets/)

EOF

echo '```' >> "$OUTPUT_FILE"
git diff --name-only main dev -- Assets/ >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 6: Différences .cursor/agents
cat >> "$OUTPUT_FILE" << 'EOF'
## 6. Différences Agents (.cursor/agents/)

EOF

echo '```' >> "$OUTPUT_FILE"
git diff --name-only main dev -- .cursor/agents/ >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 7: Commits sur dev pas sur main
cat >> "$OUTPUT_FILE" << 'EOF'
## 7. Commits sur dev non présents sur main

EOF

echo '```' >> "$OUTPUT_FILE"
git log main..dev --oneline --date=short --format="%h %ad %s" >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 8: Commits sur main pas sur dev
cat >> "$OUTPUT_FILE" << 'EOF'
## 8. Commits sur main non présents sur dev

EOF

echo '```' >> "$OUTPUT_FILE"
git log dev..main --oneline --date=short --format="%h %ad %s" >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Section 9: État des branches
cat >> "$OUTPUT_FILE" << 'EOF'
## 9. État des branches

EOF

echo '```' >> "$OUTPUT_FILE"
echo "=== Branche actuelle ===" >> "$OUTPUT_FILE"
git branch --show-current >> "$OUTPUT_FILE" 2>&1
echo "" >> "$OUTPUT_FILE"
echo "=== Derniers commits main ===" >> "$OUTPUT_FILE"
git log main --oneline -5 >> "$OUTPUT_FILE" 2>&1
echo "" >> "$OUTPUT_FILE"
echo "=== Derniers commits dev ===" >> "$OUTPUT_FILE"
git log dev --oneline -5 >> "$OUTPUT_FILE" 2>&1
echo '```' >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"

# Footer
cat >> "$OUTPUT_FILE" << 'EOF'
---

## Légende

| Symbole | Signification |
|---------|---------------|
| M | Modifié |
| A | Ajouté |
| D | Supprimé |
| R | Renommé |

---
*Rapport généré automatiquement*
EOF

echo "============================================================"
echo "✅ Rapport généré: $OUTPUT_FILE"
echo "============================================================"
echo ""
echo "📄 Aperçu du rapport:"
echo ""
head -50 "$OUTPUT_FILE"
echo ""
echo "..."
echo ""
echo "📁 Fichier complet: $OUTPUT_FILE"
