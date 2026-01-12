# Documents à inclure dans le contexte Cursor

## 📋 Documents essentiels (à toujours inclure)

### 1. Agent principal
**Fichier** : `.cursor/agents/cursor-ngo-dedicated-server.md`  
**Pourquoi** : Définit l'architecture cible, les conventions de codage, et les directives d'implémentation  
**Comment** : `@.cursor/agents/cursor-ngo-dedicated-server.md`

### 2. Plan d'implémentation
**Fichier** : `IMPLEMENTATION_PLAN_AGENT.md`  
**Pourquoi** : Détaille les phases d'implémentation et les tâches à accomplir  
**Comment** : Mentionner dans le prompt ou ouvrir le fichier

### 3. Rapport d'état de l'architecture
**Fichier** : `ARCHITECTURE_STATUS_REPORT.md`  
**Pourquoi** : État actuel du projet, conformité avec l'agent, composants implémentés  
**Comment** : Référencer pour comprendre ce qui existe déjà

## 📚 Documents de référence (selon le besoin)

### Architecture et Design
- `ARCHITECTURE.md` - Vue d'ensemble de l'architecture
- `ARCHITECTURE_STATUS_REPORT.md` - État actuel vs architecture cible
- `documentation/diagrams/` - Diagrammes (Class, Sequence, State Machine, Package)

### Documentation technique
- `SESSION_SUMMARY.md` - Résumé des corrections récentes (Assembly Definitions, etc.)
- `RPC_LAYER_ARCHITECTURE.md` - Architecture de la couche RPC
- `DEPENDENCY_GRAPH.md` - Graphique des dépendances

### Guides d'utilisation
- `.cursor/HOW_TO_USE_AGENT.md` - Comment utiliser l'agent dans Cursor
- `.cursor/QU_EST_CE_QUE_LE_CHAT_CURSOR.md` - Guide du chat Cursor
- `Assets/Editor/README_PROJECT_TOOLS.md` - Documentation des outils Unity Editor

## 🎯 Comment utiliser ces documents dans Cursor

### Méthode 1 : Mention directe dans le prompt
```
@.cursor/agents/cursor-ngo-dedicated-server.md

Référence aussi ARCHITECTURE_STATUS_REPORT.md pour l'état actuel.

[Votre question ici]
```

### Méthode 2 : Ouvrir les fichiers dans Cursor
1. Ouvrez les fichiers importants dans l'éditeur Cursor
2. Cursor les inclura automatiquement dans le contexte

### Méthode 3 : Créer un fichier de contexte
Créez un fichier `.cursor/CONTEXT.md` qui référence les documents clés :

```markdown
# Contexte du projet

Voir :
- .cursor/agents/cursor-ngo-dedicated-server.md (agent principal)
- ARCHITECTURE_STATUS_REPORT.md (état actuel)
- IMPLEMENTATION_PLAN_AGENT.md (plan d'implémentation)
```

## 📝 Ordre de priorité recommandé

### Pour une nouvelle conversation
1. **Agent** : `.cursor/agents/cursor-ngo-dedicated-server.md` (obligatoire)
2. **État actuel** : `ARCHITECTURE_STATUS_REPORT.md` (recommandé)
3. **Plan** : `IMPLEMENTATION_PLAN_AGENT.md` (si vous travaillez sur l'implémentation)

### Pour comprendre l'architecture
1. `ARCHITECTURE.md`
2. `documentation/diagrams/class_diagram.md`
3. `documentation/diagrams/statemachine_diagram.md`

### Pour corriger des erreurs
1. `SESSION_SUMMARY.md` (corrections récentes)
2. `Assets/Editor/README_PROJECT_TOOLS.md` (outils disponibles)

## 🔍 Recherche rapide

Pour trouver rapidement un document :
```bash
# Lister tous les fichiers .md
find . -name "*.md" -type f | grep -v Library | grep -v Temp

# Chercher un terme spécifique
grep -r "SessionContainer" *.md documentation/
```

## 💡 Astuce

Créez un fichier `.cursor/DEFAULT_CONTEXT.md` avec les références essentielles, puis mentionnez-le au début de chaque conversation :

```
@.cursor/DEFAULT_CONTEXT.md

[Votre question ici]
```
