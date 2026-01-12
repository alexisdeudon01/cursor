#!/usr/bin/env bash
# Script to keep only the Unity NGO Dedicated Server agent in .vscode/agents
set -euo pipefail

REPO_PATH="${1:-.}"
AGENT_FILE="cursor-ngo-dedicated-server.md"
AGENT_DIR="$REPO_PATH/.vscode/agents"

if [ ! -d "$AGENT_DIR" ]; then
  echo "Agent directory not found: $AGENT_DIR"
  exit 1
fi

if [ ! -f "$AGENT_DIR/$AGENT_FILE" ]; then
  echo "Target agent not found: $AGENT_DIR/$AGENT_FILE"
  exit 1
fi

find "$AGENT_DIR" -maxdepth 1 -type f ! -name "$AGENT_FILE" -print -delete
echo "Kept $AGENT_FILE and removed other agent files in $AGENT_DIR"
