#!/bin/bash
# Script pour lister tous les agents Cursor disponibles

echo "=== AGENTS CURSOR DISPONIBLES ==="
echo ""

AGENTS_DIR=".cursor/agents"

if [ ! -d "$AGENTS_DIR" ]; then
    echo "❌ Répertoire $AGENTS_DIR n'existe pas"
    exit 1
fi

AGENT_COUNT=$(find "$AGENTS_DIR" -name "*.md" -type f | wc -l)

if [ "$AGENT_COUNT" -eq 0 ]; then
    echo "⚠️  Aucun agent trouvé dans $AGENTS_DIR"
    exit 0
fi

echo "📁 Emplacement: $AGENTS_DIR"
echo "📊 Nombre d'agents: $AGENT_COUNT"
echo ""
echo "📋 Liste des agents:"
echo ""

# Lister les agents avec leurs métadonnées
for agent_file in "$AGENTS_DIR"/*.md; do
    if [ -f "$agent_file" ]; then
        AGENT_NAME=$(basename "$agent_file" .md)
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "📄 $AGENT_NAME"
        echo "   Fichier: $agent_file"
        
        # Extraire le nom et la description depuis le frontmatter YAML
        if grep -q "^---" "$agent_file"; then
            NAME=$(grep -A 1 "^name:" "$agent_file" | tail -1 | sed 's/^[[:space:]]*//' | sed 's/"//g')
            DESC=$(grep -A 1 "^description:" "$agent_file" | tail -1 | sed 's/^[[:space:]]*//' | sed 's/"//g')
            
            if [ -n "$NAME" ]; then
                echo "   Nom: $NAME"
            fi
            if [ -n "$DESC" ]; then
                echo "   Description: $DESC"
            fi
        fi
        
        # Afficher la taille du fichier
        SIZE=$(du -h "$agent_file" | cut -f1)
        LINES=$(wc -l < "$agent_file")
        echo "   Taille: $SIZE ($LINES lignes)"
        echo ""
    fi
done

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "💡 Pour voir le contenu complet d'un agent:"
echo "   cat $AGENTS_DIR/<nom-agent>.md"
echo ""
echo "💡 Pour ouvrir dans Cursor:"
echo "   Ouvrez le fichier dans Cursor et allez dans les paramètres pour le sélectionner"
echo ""
echo "💡 Pour utiliser le gestionnaire Unity:"
echo "   Tools → Cursor → Manage Agents"
echo ""
echo "💡 Commandes utiles:"
echo "   - Ouvrir le répertoire: Tools → Cursor → Open Agents Directory"
echo "   - Vérifier la config: Tools → Cursor → Verify Agent Configuration"
