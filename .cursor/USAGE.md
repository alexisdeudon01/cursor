# Guide d'utilisation des agents Cursor

## ✅ Configuration vérifiée

Votre agent Cursor est correctement configuré :
- **Agent actif**: `cursor-ngo-dedicated-server.md`
- **Emplacement**: `.cursor/agents/cursor-ngo-dedicated-server.md`
- **Taille**: 4.6 KB (108 lignes)

## 🎯 Comment utiliser les agents dans Cursor

### Méthode 1 : Via le panneau Agents (recommandé)
1. Ouvrez Cursor
2. Cherchez le panneau latéral **"Agents"** ou **"Custom Agents"**
3. Vous verrez votre agent `cursor-ngo-dedicated-server`
4. Cliquez dessus pour l'activer

### Méthode 2 : Via les paramètres
1. `Ctrl+,` (ou `Cmd+,` sur Mac) pour ouvrir les paramètres
2. Recherchez "agent" ou "custom agent"
3. Sélectionnez `cursor-ngo-dedicated-server` comme agent par défaut

### Méthode 3 : Via la palette de commandes
1. `Ctrl+Shift+P` (ou `Cmd+Shift+P` sur Mac)
2. Tapez "agent" ou "select agent"
3. Choisissez votre agent

## 🛠️ Outils Unity disponibles

Dans Unity, allez dans le menu **Tools → Cursor** :

### 1. Manage Agents
Ouvre une fenêtre complète pour :
- ✅ Voir tous les agents disponibles
- ✅ Afficher les détails (description, taille, date de modification)
- ✅ Ouvrir un agent dans l'éditeur par défaut
- ✅ Rechercher parmi les agents
- ✅ Créer de nouveaux agents

### 2. List Agents
Affiche une liste rapide des agents dans une boîte de dialogue

### 3. Open Agents Directory
Ouvre le répertoire `.cursor/agents/` dans votre explorateur de fichiers

### 4. Verify Agent Configuration
Vérifie que :
- Le répertoire des agents existe
- Les fichiers d'agents sont valides
- La configuration est correcte

### 5. Create Agent Template
Crée un nouveau template d'agent que vous pouvez personnaliser

## 📝 Scripts en ligne de commande

### Script Bash
```bash
./list_agents.sh
```

### Script Node.js
```bash
# Lister les agents
node .cursor/scripts/manage-agents.js list

# Voir les détails d'un agent
node .cursor/scripts/manage-agents.js info cursor-ngo-dedicated-server

# Vérifier la configuration
node .cursor/scripts/manage-agents.js verify

# Créer un template
node .cursor/scripts/manage-agents.js create mon-agent
```

## 🎨 Interface Unity

La fenêtre **Manage Agents** dans Unity offre :
- 📋 Liste complète des agents avec recherche
- 📄 Aperçu de la description de chaque agent
- 📊 Informations détaillées (taille, lignes, date de modification)
- 🔍 Recherche en temps réel
- 🚀 Actions rapides (Ouvrir, Révéler dans l'explorateur)
- 🔄 Rafraîchissement automatique

## 💡 Conseils

1. **Utilisez l'agent Unity** : L'interface Unity est la plus complète pour gérer vos agents
2. **Vérifiez régulièrement** : Utilisez "Verify Agent Configuration" pour vous assurer que tout est correct
3. **Créez des templates** : Utilisez "Create Agent Template" pour créer de nouveaux agents rapidement
4. **Sauvegardez vos agents** : Les agents sont dans `.cursor/agents/`, assurez-vous qu'ils sont versionnés dans git

## 🔗 Fichiers créés

- `Assets/Editor/CursorAgentManager.cs` - Gestionnaire Unity pour les agents
- `.cursor/scripts/manage-agents.js` - Script Node.js pour la ligne de commande
- `.cursor/scripts/README.md` - Documentation des scripts
- `.vscode/tasks.json` - Tâches VSCode/Cursor pour les agents

## 📚 Documentation

- Voir `.cursor/README.md` pour la documentation générale des agents
- Voir `.cursor/scripts/README.md` pour la documentation des scripts
