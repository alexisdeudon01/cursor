# Review Playbook v1
**Date**: 2024-12-19  
**Session**: Analyse initiale Unity NGO 2D

---

## Patterns découverts

### 1. Scripts critiques sans assembly spécifique
**Symptôme**: Script dans `Assets/Scripts/Networking/` sans `.asmdef` parent ou sans namespace  
**Impact**: Violation séparation Client/Server  
**Fréquence**: 1 occurrence majeure (SessionRpcHub)  
**Détection**: 
- Chercher fichiers `.cs` dans `Networking/` sans `.asmdef` parent
- Vérifier namespace (absence = alerte)  
**Correction**: Déplacer dans assembly appropriée (Shared/Server/Client/Player)

### 2. Singleton NetworkBehaviour
**Symptôme**: `public static Instance` dans NetworkBehaviour  
**Impact**: Risque conflits si plusieurs instances spawnées  
**Fréquence**: SessionRpcHub utilise Singleton  
**Détection**: Chercher `static.*Instance` dans NetworkBehaviour  
**Vérification**: S'assurer qu'une seule instance est spawnée  
**Évaluation**: Pattern acceptable mais à surveiller

### 3. Dynamic NetworkPrefabs Registration
**Symptôme**: `NetworkPrefabsLists: []` dans NetworkManager mais prefabs fonctionnent  
**Impact**: Configuration moins visible dans l'éditeur  
**Fréquence**: NetworkBootstrap enregistre dynamiquement  
**Détection**: Vérifier `RegisterRequiredNetworkPrefabs()` dans bootstrap  
**Évaluation**: Pattern acceptable si documenté

---

## Anti-patterns identifiés

### ❌ Scripts networking dans Assembly-CSharp
**Pourquoi problématique**: Pas de séparation Client/Server  
**Alternative**: Utiliser assemblies dédiées (Shared/Server/Client/Player)  
**Exemple**: `SessionRpcHub.cs` sans namespace/assembly

### ⚠️ Singleton global dans NetworkBehaviour
**Pourquoi risqué**: Peut causer conflits si plusieurs instances  
**Alternative**: Utiliser NetworkManager.Singleton ou pattern spécifique  
**Exemple**: `SessionRpcHub.Instance`

---

## Checklists mises à jour

### Checklist Architecture
- [x] Vérifier séparation Client/Server dans asmdef
- [x] Vérifier scripts critiques dans assemblies appropriées
- [x] Vérifier absence de références croisées Client ↔ Server
- [NEW] Vérifier scripts dans `Networking/` sans assembly spécifique

### Checklist NGO
- [x] Vérifier NetworkObject ownership
- [x] Vérifier NetworkPrefabs registration
- [x] Vérifier RPC validation
- [NEW] Vérifier Singleton NetworkBehaviour

### Checklist Unity
- [x] Vérifier prefab wiring
- [x] Vérifier scène serveur identifiée
- [NEW] Vérifier configuration NetworkPrefabs (asset vs dynamique)

### Checklist UI
- [x] Vérifier séparation UI ↔ Networking
- [x] Vérifier absence de logique gameplay côté UI

---

## Améliorations de détection

### 1. Détection scripts sans assembly
**Méthode**: 
- Chercher fichiers `.cs` dans `Networking/` sans `.asmdef` parent
- Vérifier namespace (absence = alerte)

**Règle ajoutée**: Scripts dans `Networking/` sans assembly spécifique = alerte

### 2. Détection Singleton NetworkBehaviour
**Méthode**: 
- Chercher `static.*Instance` dans classes héritant de NetworkBehaviour
- Vérifier gestion multi-instance

**Règle ajoutée**: Singleton + NetworkBehaviour = vérifier la gestion multi-instance

### 3. Détection NetworkPrefabs configuration
**Méthode**: 
- Vérifier `NetworkPrefabsLists` dans NetworkManager prefab
- Chercher `RegisterRequiredNetworkPrefabs()` dans bootstrap

**Règle ajoutée**: NetworkPrefabs vides dans NetworkManager = vérifier registration dynamique

---

## Métriques de la session

- **Temps de discovery**: ~15 minutes
- **Fichiers analysés**: ~111 scripts C#, 5 scènes, 7 prefabs, 5 UXML
- **Problèmes détectés**: 5 (2 critiques, 3 mineurs/info)
- **Suggestions générées**: 2
- **Couverture estimée**: ~80% (focus sur networking et core)
- **Précision estimée**: 85% (basée sur analyse statique)

---

## Leçons apprises

1. **Les scripts sans namespace dans Networking/ sont suspects** - Toujours vérifier l'assembly
2. **Les singletons NetworkBehaviour nécessitent une attention particulière** - Vérifier la gestion multi-instance
3. **La registration dynamique de NetworkPrefabs est acceptable** - Mais doit être documentée

---

## Prochaines améliorations prévues

1. Automatiser la détection de scripts sans assembly
2. Créer une checklist spécifique pour les NetworkBehaviour singletons
3. Documenter les patterns de registration dynamique de NetworkPrefabs

---

**Version**: 1.0  
**Dernière mise à jour**: 2024-12-19
