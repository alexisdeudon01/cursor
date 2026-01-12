# Contexte par défaut du projet

## 🎯 Agent principal
- **Agent** : `.cursor/agents/cursor-ngo-dedicated-server.md`
  - Architecture Unity 2D + NGO avec serveur dédié autoritaire
  - Builds client/serveur séparés
  - Pas de services externes

## 📊 État actuel du projet
- **Rapport d'état** : `ARCHITECTURE_STATUS_REPORT.md`
  - Score de conformité : 98%
  - Composants core implémentés et alignés
  - Prêt pour tests réseau

## 📋 Plan d'implémentation
- **Plan** : `IMPLEMENTATION_PLAN_AGENT.md`
  - 6 phases d'implémentation détaillées
  - Tâches spécifiques par phase

## 🔧 Corrections récentes
- **Résumé session** : `SESSION_SUMMARY.md`
  - Assembly Definitions corrigées
  - Dépendance circulaire résolue
  - Menu Unity Editor créé

## 🏗️ Architecture
- **Architecture** : `ARCHITECTURE.md`
- **Diagrammes** : `documentation/diagrams/`

## 📝 Conventions
- Serveur autoritaire : validation serveur-side uniquement
- Client : envoie uniquement des intentions (inputs)
- Séparation : `#if UNITY_SERVER` et Assembly Definitions
- DTOs : messages réseau compacts dans `Core.StateSync`
