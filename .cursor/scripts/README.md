# Cursor Agents Management Scripts

Ce répertoire contient des scripts pour gérer les agents Cursor.

## Scripts disponibles

### `manage-agents.js` (Node.js)

Script Node.js pour gérer les agents depuis la ligne de commande.

**Installation:**
```bash
chmod +x .cursor/scripts/manage-agents.js
```

**Utilisation:**
```bash
# Lister tous les agents
node .cursor/scripts/manage-agents.js list

# Afficher les détails d'un agent
node .cursor/scripts/manage-agents.js info cursor-ngo-dedicated-server

# Vérifier la configuration
node .cursor/scripts/manage-agents.js verify

# Créer un nouveau template d'agent
node .cursor/scripts/manage-agents.js create mon-nouvel-agent
```

## Scripts shell

### `list_agents.sh` (Bash)

Script bash pour lister les agents disponibles.

**Utilisation:**
```bash
./list_agents.sh
```

## Intégration Unity

Utilisez le menu Unity pour gérer les agents:

- **Tools → Cursor → Manage Agents** - Ouvrir la fenêtre de gestion
- **Tools → Cursor → List Agents** - Lister les agents dans une boîte de dialogue
- **Tools → Cursor → Open Agents Directory** - Ouvrir le répertoire des agents
- **Tools → Cursor → Verify Agent Configuration** - Vérifier la configuration
- **Tools → Cursor → Create Agent Template** - Créer un nouveau template

## Structure des agents

Les agents doivent être placés dans `.cursor/agents/` avec l'extension `.md`.

Format recommandé:
```markdown
# Agent role
Description de l'agent

# Mandatory context
- Contexte obligatoire

# Target architecture
Architecture cible

# Implementation directives
Directives d'implémentation

# Coding conventions
Conventions de codage
```
