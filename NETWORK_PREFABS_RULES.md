# Network Prefabs - Règles et Guidelines

## Quand créer un NetworkPrefab? ✅

Un objet doit être un **NetworkPrefab** si:

1. **Spawné dynamiquement par code** pendant le runtime
   - Exemple: Pawns de joueurs (SquarePawn, CirclePawn)
   - Exemple: Projectiles, items ramassables, effets spéciaux

2. **Synchronisé entre serveur et clients** via NetworkObject
   - Objets qui doivent avoir des NetworkVariables
   - Objets qui reçoivent des RPCs

3. **Instantié plusieurs fois** pendant une session
   - Spawns de pawns pour chaque joueur
   - Création dynamique d'objets de jeu

4. **Services réseau partagés** spawnés au démarrage serveur
   - SessionRpcHub (spawné une fois au démarrage)
   - NetworkPawnSpawner (service de spawn)

## Quand NE PAS créer un NetworkPrefab? ❌

1. **Objects pure UI** (Canvas, menus, overlays)
   ```
   ❌ SessionLobbyUI - Local only
   ❌ ToastNotification - Local only
   ❌ ProgressIndicator - Local only
   ❌ GameDebugUI - Local only
   ```
   **Raison**: UI est client-specific, pas besoin de synchronisation réseau

2. **Managers server-only** (singletons de gestion)
   ```
   ❌ GameSessionManager - Server only
   ❌ GameInstanceManager - Server only
   ❌ SessionContainerManager - Server only
   ❌ GameRegistry - Static registry
   ```
   **Raison**: Ces managers n'ont pas besoin de NetworkObject, ils gèrent l'état serveur uniquement

3. **Objets déjà dans une scène** et jamais spawnés par code
   ```
   ❌ NetworkManagerRoot - Scene-placed singleton
   ❌ Camera Main - Déjà dans Game.unity
   ❌ GameCanvas - Déjà dans Game.unity
   ❌ GameManager - Déjà dans Game.unity
   ```
   **Raison**: Risque de doublons si spawné en plus de l'objet de scène

4. **ScriptableObjects** (données de configuration)
   ```
   ❌ GameDefinitionAsset - ScriptableObject
   ❌ CircleGameDefinition - ScriptableObject
   ❌ SquareGameDefinition - ScriptableObject
   ```
   **Raison**: Les ScriptableObjects sont des assets, pas des objets de scène

5. **Pure C# classes** (non-MonoBehaviour)
   ```
   ❌ SessionContainer - Pure C# class
   ❌ GameContainer - Pure C# class
   ❌ PlayerNetworkData - Data structure
   ❌ ClientNetworkData - Data structure
   ```
   **Raison**: Pas de composant Unity, donc pas de NetworkObject possible

## Liste des NetworkPrefabs actuels ✅

### Prefabs réseau essentiels:

1. **DefaultPlayer** (`Assets/Prefabs/Network/DefaultPlayer.prefab`)
   - NetworkObject assigné à NetworkManager pour connexions client
   - Spawné automatiquement pour chaque client connecté
   - Persistant pendant toute la session client

2. **SessionRpcHub** (`Assets/Prefabs/Network/SessionRpcHub.prefab`)
   - Spawné une fois au démarrage serveur (ServerBootstrap)
   - Gère tous les RPCs de session (Create, Join, Leave, etc.)
   - Singleton global pour communication client-serveur

3. **SquarePawn** (`Assets/Prefabs/Pawns/SquarePawn.prefab`)
   - Spawné dynamiquement quand un jeu "square-game" démarre
   - Un pawn par joueur dans la session
   - Déspawné quand le jeu se termine

4. **CirclePawn** (`Assets/Prefabs/Pawns/CirclePawn.prefab`)
   - Spawné dynamiquement quand un jeu "circle-game" démarre
   - Un pawn par joueur dans la session
   - Déspawné quand le jeu se termine

### Enregistrement dans DefaultNetworkPrefabs:

Unity Netcode utilise `DefaultNetworkPrefabs` (Unity 6+) pour gérer automatiquement les prefabs réseau.

**Fichier**: `Assets/DefaultNetworkPrefabs.asset`

**Comment ajouter un prefab**:
1. Créer le prefab avec composant `NetworkObject`
2. Dans Unity Editor: Window → Netcode → Default Network Prefabs
3. Ajouter le prefab à la liste
4. Netcode synchronise automatiquement les NetworkObjectIds

**Avantages vs PrefabReferences (ancien système)**:
- ✅ Système officiel Unity Netcode
- ✅ Pas besoin de ScriptableObject custom
- ✅ Validation automatique des NetworkObjectIds
- ✅ Support des nested prefabs
- ✅ Hot reload dans Editor

## Migration de PrefabReferences → DefaultNetworkPrefabs

### Ancien système (DEPRECATED):
```csharp
[SerializeField] private PrefabReferences prefabReferences;
private NetworkObject SquarePrefab => prefabReferences.SquarePrefab;
```

### Nouveau système (RECOMMANDÉ):
```csharp
// Option 1: Spawn par nom de prefab
NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
    NetworkManager.Singleton.NetworkConfig.Prefabs.GetPrefabByName("SquarePawn"),
    ownerClientId: clientId
);

// Option 2: Référence directe dans Inspector (si nécessaire)
[SerializeField] private NetworkObject squarePawnPrefab;
NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
    squarePawnPrefab,
    ownerClientId: clientId
);

// Option 3: Via service locator
var pawnSpawner = ServiceLocator.Get<IPawnSpawner>();
pawnSpawner.SpawnPawn(clientId, position, pawnType);
```

### Fichiers à supprimer:
- ❌ `Assets/Scripts/Data/PrefabReferences.cs` - Plus nécessaire
- ❌ `Assets/Resources/PrefabReferences.asset` - Remplacé par DefaultNetworkPrefabs.asset
- ❌ Références dans `SessionRpcHub` (ligne 18-20) - Supprimées

### Fichiers à mettre à jour:
- ⚠️ `Assets/Scripts/Editor/UnifiedBuildTool.cs` - Supprimer méthodes `LoadPrefabReferences()` et `SyncNetworkPrefabsWithReferences()`

## Architecture RPC Layer

### Handlers créés (nouveau système modulaire):

```
Assets/Scripts/Networking/RpcHandlers/
├── Interfaces/
│   └── ISessionRpcHandler.cs - Interfaces de base
├── Base/
│   └── BaseRpcHandler.cs - Classes de base avec logging
└── Handlers/
    ├── SessionLifecycleHandler.cs - Create, Join, Leave, SetReady
    ├── GameStartHandler.cs - Validation et démarrage de jeu
    ├── PlayerMovementHandler.cs - Mouvement avec rate limiting
    ├── SceneLoadHandler.cs - Chargement de Game.unity
    └── SessionQueryHandler.cs - RequestSessions, RequestDetails
```

### SessionRpcHub refactorisé:
- **Avant**: 767 lignes monolithiques
- **Après**: ~200 lignes de délégation aux handlers
- Handlers spécialisés avec responsabilité unique
- Logging centralisé avec nom de handler
- Validation via BaseValidator

## Conteneur Player

### PlayerNetworkData (nouveau):

**Fichier**: `Assets/Scripts/Networking/Data/PlayerNetworkData.cs`

```csharp
public class PlayerNetworkData
{
    // IDENTIFICATION
    public ulong ClientId { get; set; }
    public string PlayerName { get; set; }
    public string CurrentSessionId { get; set; }
    
    // NETWORK OBJECTS
    public NetworkObject PlayerObject { get; set; }  // DefaultPlayer
    public NetworkObject CurrentPawn { get; set; }    // SquarePawn/CirclePawn
    
    // SESSION STATE
    public bool IsReady { get; set; }
    public bool IsInGame { get; set; }
    public bool IsSessionHost { get; set; }
    
    // GAME STATE
    public Vector3 Position { get; set; }
    public int Score { get; set; }
    public bool IsAlive { get; set; }
    public int Team { get; set; }
    
    // STATISTICS
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public int MessagesReceived { get; set; }
    public int MessagesSent { get; set; }
    public int GamesPlayed { get; set; }
    public int TotalWins { get; set; }
}
```

**Usage**:
```csharp
// Création au connection
var playerData = new PlayerNetworkData(clientId, playerName);

// Entrée dans session
playerData.CurrentSessionId = sessionName;
playerData.IsSessionHost = (clientId == creatorId);

// Spawn de pawn
playerData.InitializeGameState(pawn, spawnPosition);

// Mise à jour position
playerData.Position = newPosition;
playerData.RecordActivity();

// Fin de jeu
playerData.ResetGameState();
```

## Best Practices

### ✅ DO:
1. Utiliser DefaultNetworkPrefabs pour tous les prefabs réseau
2. Créer des handlers spécialisés pour groupes de RPCs similaires
3. Valider côté serveur avec BaseValidator
4. Logger avec préfixe de handler pour debugging
5. Utiliser PlayerNetworkData pour consolider les données player

### ❌ DON'T:
1. Créer NetworkPrefab pour UI local-only
2. Spawner objets de scène déjà présents
3. Mettre NetworkObject sur managers server-only
4. Utiliser PrefabReferences (système deprecated)
5. Créer handlers monolithiques (respecter SRP)

## Checklist nouvelle feature réseau

Quand vous ajoutez une nouvelle feature réseau:

- [ ] C'est un objet spawné dynamiquement? → NetworkPrefab ✅
- [ ] C'est de l'UI client-only? → Pas de NetworkObject ❌
- [ ] C'est un manager server? → Pas de NetworkPrefab ❌
- [ ] Ajouter prefab à DefaultNetworkPrefabs.asset
- [ ] Créer handler spécialisé dans RpcHandlers/Handlers/
- [ ] Hériter de BaseRpcHandler
- [ ] Implémenter validation avec BaseValidator
- [ ] Logger avec GetHandlerName()
- [ ] Initialiser handler dans SessionRpcHub.InitializeHandlers()
- [ ] Cleanup handler dans SessionRpcHub.OnDestroy()
- [ ] Tester avec multiple sessions concurrentes

## Références

- **DefaultNetworkPrefabs**: Unity Netcode for GameObjects documentation
- **SessionRpcHub**: `Assets/Scripts/Networking/Player/SessionRpcHub.cs`
- **Handlers**: `Assets/Scripts/Networking/RpcHandlers/`
- **PlayerNetworkData**: `Assets/Scripts/Networking/Data/PlayerNetworkData.cs`
- **Architecture 3-Level**: `ARCHITECTURE_3_LEVEL.md`
- **Refactoring Plan**: `REFACTORING_PLAN.md`
