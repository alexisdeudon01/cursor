# Cursor Agents

Ce répertoire contient les agents Cursor pour ce projet.

## Agent disponible

### `cursor-ngo-dedicated-server.md`

Agent spécialisé pour Unity 2D + Netcode for GameObjects avec architecture client/serveur autoritaire.

**Caractéristiques :**
- Architecture client/serveur avec serveur dédié autoritaire
- Deux builds distincts : client multi-scène et serveur headless
- Utilise uniquement NGO (pas de services externes)
- Aligné avec les diagrammes d'architecture du projet

**Utilisation :**
1. Dans Cursor, allez dans les paramètres
2. Sélectionnez cet agent comme agent par défaut pour ce projet
3. L'agent sera utilisé automatiquement pour toutes les tâches de planification, implémentation et révision

## Structure

```
.cursor/
└── agents/
    └── cursor-ngo-dedicated-server.md
```

## Documentation

Voir le fichier de l'agent pour plus de détails sur :
- L'architecture cible
- Les directives d'implémentation
- Les conventions de codage
- Les instructions de configuration
