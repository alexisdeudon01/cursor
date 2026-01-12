# Guide: Ajouter un nouveau jeu 2D

Ce guide explique comment ajouter facilement un nouveau jeu 2D au projet.

## Étapes

### 1. Créer le ScriptableObject du jeu

1. Dans Unity Editor: `Assets` → `Create` → `Game Definition` (ou chercher `GameDefinitionAsset`)
2. Créer un nouveau ScriptableObject héritant de `GameDefinitionAsset`

Exemple:
```csharp
using UnityEngine;
using Core.Maps;

[CreateAssetMenu(fileName = "MyNewGame", menuName = "Games/My New Game")]
public class MyNewGameDefinition : GameDefinitionAsset
{
    public override MapConfigData CreateMapConfig(Vector3 worldOffset, int seed)
    {
        // Créer la configuration de la map pour ce jeu
        return new MapConfigData
        {
            // Configuration de la map
        };
    }

    public override Vector3 GetSpawnPosition(int playerIndex, int totalPlayers, MapConfigData config)
    {
        // Calculer la position de spawn pour ce joueur
        return Vector3.zero; // À adapter
    }

    public override void CleanupGame()
    {
        // Nettoyer les ressources spécifiques au jeu
    }
}
```

### 2. Implémenter les méthodes requises

- `CreateMapConfig()`: Créer la configuration de la map
- `GetSpawnPosition()`: Calculer les positions de spawn
- `CleanupGame()`: Nettoyer les ressources

### 3. Créer le prefab de pawn

1. Créer un prefab avec un `NetworkObject` (si nécessaire)
2. Ajouter les composants visuels (SpriteRenderer, etc.)
3. Ajouter le script de pawn associé

### 4. Enregistrer le prefab comme NetworkPrefab

1. Ajouter le prefab à `Assets/DefaultNetworkPrefabs.asset`
2. Ou utiliser `NetworkBootstrap.RegisterRequiredNetworkPrefabs()` pour enregistrement dynamique

### 5. Placer l'asset dans Resources/Games/

1. Créer le dossier `Assets/Resources/Games/` si nécessaire
2. Placer le ScriptableObject du jeu dans ce dossier
3. Le jeu s'enregistrera automatiquement au démarrage via `GameRegistry.Initialize()`

### 6. Configurer les paramètres du jeu

Dans l'inspecteur Unity, configurer:
- `GameId`: Identifiant unique (ex: "my-new-game")
- `DisplayName`: Nom affiché dans l'UI
- `Description`: Description du jeu
- `MinPlayers` / `MaxPlayers`: Limites de joueurs
- `MoveSpeed`: Vitesse de déplacement
- `PawnPrefabType`: Type de prefab de pawn

## Vérification

Le jeu devrait apparaître automatiquement:
- Dans `GameRegistry.AllGames`
- Dans l'UI de sélection de jeu (si implémentée)
- Disponible pour créer des sessions

## Notes

- **Pas besoin de modifier le code core** pour ajouter un jeu
- Le système est **modulaire** et **extensible**
- Les jeux sont **auto-enregistrés** depuis `Resources/Games/`

## Exemples existants

- `SquareGameDefinition`: Jeu simple avec carrés
- `CircleGameDefinition`: Jeu avec cercles

Voir `Assets/Scripts/Games/` pour les implémentations de référence.
