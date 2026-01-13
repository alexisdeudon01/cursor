# 🏗️ Session & Game Architecture

This document describes the session/lobby and pluggable game system.

---

## 📊 Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                              USER FLOW                                    │
│                                                                          │
│   [1. Enter Name] → [2. See Sessions] → [3. Join/Create] → [4. Lobby]   │
│                                         [Select Game Type]      ↓        │
│                                                           [5. Ready Up]  │
│                                                                 ↓        │
│                                                         [6. Start Game]  │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🎮 Game Plugin System

### Adding a New Game

1. **Create Game Definition** (ScriptableObject):
```csharp
[CreateAssetMenu(fileName = "MyGame", menuName = "Games/My Game")]
public class MyGameDefinition : GameDefinitionAsset
{
    public override void SetupGame(Vector3 worldOffset) { }
    public override Vector3 GetSpawnPosition(int index, int total, Vector3 offset) { }
    public override void InitializePawn(NetworkObject pawn, string name, int index, Vector3 offset) { }
    public override void SetupClientVisuals() { }
    public override void HandleMovement(NetworkObject pawn, Vector2 dir) { }
    public override void CleanupGame() { }
}
```

2. **Create Pawn Component** (if needed):
```csharp
public class MyPawn : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName;
    public void Initialize(string name, int color, Vector3 offset) { }
    public void Move(Vector2 direction) { }
}
```

3. **Create ScriptableObject Asset**:
   - Right-click in Project: `Create > Games > My Game`
   - Place in `Resources/Games/` folder
   - Assign pawn prefab

4. **Register prefab with NetworkManager**:
   - Add pawn prefab to NetworkPrefabList

### Available Games

| Game ID | Display Name | Pawn Type | Arena |
|---------|-------------|-----------|-------|
| `square-game` | Square Game | PlayerPawn (square) | Rectangular with obstacles |
| `circle-game` | Circle Game | CirclePawn (circle) | Circular arena with rings |

---

## 👤 Player Feature System

### Adding a New Player Feature

1. **Create Feature Class**:
```csharp
public class HealthFeature : NetworkPlayerFeature
{
    public override string FeatureId => "health";
    
    public NetworkVariable<int> Health = new(...);
    
    public override void OnPlayerSpawn(ulong clientId)
    {
        if (IsServer) Health.Value = 100;
    }
    
    public override void OnPlayerDespawn(ulong clientId) { }
}
```

2. **Add to Player Prefab** OR **Register dynamically**:
```csharp
PlayerFeatureRegistry.RegisterFeatureType<HealthFeature>();
```

---

## 🧱 Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         UI LAYER                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   SessionPresenter                        │  │
│  │  - Binds UI events to service calls                      │  │
│  │  - Transforms service events to UI-friendly data         │  │
│  │  - Manages panel navigation                               │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
├─────────────────────────────────────────────────────────────────┤
│                       SERVICE LAYER                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   ISessionService                         │  │
│  │  Events:                                                  │  │
│  │    - SessionsChanged                                      │  │
│  │    - CurrentSessionChanged                                │  │
│  │    - ErrorOccurred                                        │  │
│  │    - GameStarting                                         │  │
│  │  Commands:                                                │  │
│  │    - CreateSession, JoinSession, LeaveSession            │  │
│  │    - SetReady, StartGame, SetGameType                    │  │
│  │    - RefreshSessions, RefreshCurrentSession              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                │                           │                     │
│       ┌────────┴────────┐         ┌───────┴────────┐           │
│       ▼                 ▼         ▼                ▼           │
│  ┌──────────┐    ┌──────────┐  ┌──────────────┐  ┌─────────┐  │
│  │ Client   │    │ Server   │  │ GameInstance │  │ Game    │  │
│  │ Service  │    │ Service  │  │   Manager    │  │ Registry│  │
│  └──────────┘    └──────────┘  └──────────────┘  └─────────┘  │
│                              │                                   │
│                              ▼                                   │
├─────────────────────────────────────────────────────────────────┤
│                       NETWORK LAYER                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   SessionRpcHub                           │  │
│  │  ServerRPCs:                                              │  │
│  │    - CreateSessionServerRpc                               │  │
│  │    - JoinSessionServerRpc                                 │  │
│  │    - SetReadyServerRpc                                    │  │
│  │    - SetGameTypeServerRpc  ← Game selection              │  │
│  │    - StartGameServerRpc                                   │  │
│  │    - RequestMoveServerRpc                                 │  │
│  │  ClientRPCs:                                              │  │
│  │    - SyncSessionsClientRpc                                │  │
│  │    - StartGameClientRpc (with gameId)                    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
├─────────────────────────────────────────────────────────────────┤
│                       DATA LAYER (Server Only)                   │
│  ┌────────────────────┐    ┌────────────────────┐              │
│  │ GameSessionManager │    │ GameInstanceManager│              │
│  │ - sessions dict    │    │ - activeGames dict │              │
│  │ - selectedGameId   │    │ - per-session state│              │
│  └────────────────────┘    └────────────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 File Structure

```
Assets/Scripts/
├── Core/
│   ├── Games/
│   │   ├── IGameDefinition.cs      ← Game plugin interface
│   │   ├── GameRegistry.cs         ← Game type registry
│   │   └── GameInstanceManager.cs  ← Active game instances
│   ├── Players/
│   │   ├── IPlayerFeature.cs       ← Player feature interface
│   │   └── PlayerFeatureRegistry.cs← Feature registry
│   └── SceneNames.cs               ← Scene constants
│
├── Games/
│   ├── SquareGame/
│   │   └── SquareGameDefinition.cs ← Square game implementation
│   └── CircleGame/
│       ├── CircleGameDefinition.cs ← Circle game implementation
│       └── CirclePawn.cs           ← Circle pawn component
│
├── Game/
│   ├── PlayerPawn.cs               ← Square pawn component
│   ├── PlayerInputHandler.cs       ← Client input handler
│   ├── CameraFollowPawn.cs         ← Client camera controller
│   └── GameSceneSetup.cs           ← Fallback scene setup
│
├── Networking/
│   ├── Connections/
│   │   ├── NetworkBootstrap.cs     ← Network initialization
│   │   ├── AppNetworkConfig.cs     ← Network settings
│   │   ├── NetworkConfigProvider.cs← Config loader
│   │   └── PrefabIdentity.cs       ← Prefab duplicate detection
│   ├── Player/
│   │   ├── DefaultPlayer.cs        ← Player network object
│   │   ├── PlayerManager.cs        ← Server player registry
│   │   └── SessionRpcHub.cs        ← RPC router
│   ├── Sessions/
│   │   ├── GameSessionManager.cs   ← Session storage
│   │   ├── SessionState.cs         ← Session data
│   │   └── GameSession.cs          ← Session DTOs
│   └── NetworkPrefabListManager.cs ← Runtime prefab management
│
├── Service/
│   ├── Core/
│   │   ├── ISessionService.cs      ← Session interface
│   │   ├── SessionServiceClient.cs ← Client implementation
│   │   ├── SessionServiceServer.cs ← Server implementation
│   │   └── ServiceLocator.cs       ← DI container
│   └── SceneService/
│       ├── ISceneServiceSync.cs    ← Scene service interface
│       ├── Client/                 ← Client scene loading
│       └── Server/                 ← Server scene loading
│
├── Data/
│   └── PrefabReferences.cs         ← Prefab ScriptableObject
│
└── Documentation/
    ├── Architecture.md             ← This file
    ├── Diagrams.md                 ← Mermaid diagrams
    └── BugTracker.md               ← Known issues
```

---

## 🔌 Dependency Injection

Services are registered via `ServiceLocator`:

```csharp
// Registration (at bootstrap)
ServiceLocator.Register<ISessionService>(new SessionServiceClient());

// Resolution (anywhere)
var service = ServiceLocator.Get<ISessionService>();
service.CreateSession("My Room");
```

---

## 🎯 Session-Game Decoupling

Sessions are **not** tied to a specific game type. The flow is:

1. **Create Session** - Session starts with no game selected
2. **Select Game** - Host calls `SetGameType("square-game")` or `SetGameType("circle-game")`
3. **Start Game** - Server looks up game from `GameRegistry`, spawns pawns via `GameInstanceManager`

```csharp
// In lobby, host selects game type
sessionService.SetGameType(sessionId, "circle-game");

// When starting, server uses the selected game
var gameDef = GameRegistry.GetGame(session.SelectedGameId);
GameInstanceManager.CreateGame(sessionId, gameDef, players, worldOffset);
```

---

## 🌍 Multi-Session Support

Multiple sessions can run simultaneously with world isolation:

- Each session gets a world offset: `sessionIndex * 50` on X axis
- `CameraFollowPawn` handles local camera for each client
- `GameInstanceManager` tracks which pawns belong to which session

---

## 📝 Setup Checklist

To use the game plugin system:

1. ☐ Create `Resources/Games/` folder in Unity
2. ☐ Create SquareGame.asset: `Assets > Create > Games > Square Game`
3. ☐ Create CircleGame.asset: `Assets > Create > Games > Circle Game`
4. ☐ Create Circle prefab with NetworkObject + CirclePawn components
5. ☐ Add Circle prefab to NetworkPrefabList
6. ☐ Ensure PlayerPawn prefab is also in NetworkPrefabList

---

**Last Updated:** 2026-01-08
