#!/bin/bash
# Script d'amélioration continue automatique
# S'exécute toutes les 30 minutes pour améliorer le projet

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Trouver la dernière version de l'agent
LATEST_VERSION=$(ls -1 "$SCRIPT_DIR"/thebestclient*.md 2>/dev/null | grep -oE 'thebestclient[0-9]+' | sed 's/thebestclient//' | sort -n | tail -1)

if [ -z "$LATEST_VERSION" ]; then
    LATEST_VERSION=2
else
    LATEST_VERSION=$((LATEST_VERSION + 1))
fi

echo "[Auto-Improve] Starting cycle for version $LATEST_VERSION"
echo "[Auto-Improve] Previous version: $((LATEST_VERSION - 1))"
echo "[Auto-Improve] Time: $(date)"

# Log de l'exécution
LOG_FILE="$SCRIPT_DIR/improvement-log.md"
echo "## Cycle $(date '+%Y-%m-%d %H:%M:%S') - Version $LATEST_VERSION" >> "$LOG_FILE"
echo "" >> "$LOG_FILE"

# Note: Ce script appelle l'agent AI via Cursor
# L'agent AI doit être activé manuellement ou via API Cursor
# Pour l'instant, on log juste l'exécution

echo "[Auto-Improve] Cycle completed. Next version: thebestclient$LATEST_VERSION"
echo "[Auto-Improve] Log written to: $LOG_FILE"
