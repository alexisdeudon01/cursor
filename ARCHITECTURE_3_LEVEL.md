# 3-Level Container Architecture

## Overview

The codebase now uses a **3-level hierarchical container architecture** for proper separation of concerns:

```
Local Container (UI/Client-specific)
  ↓
Session Container (Session logic & player management)
  ↓
Game Container (Game instance with dedicated scene)
```

This architecture enables:
- **Multiple concurrent game sessions** on one server (session isolation)
- **Dedicated Game scene** loaded additively for actual gameplay
- **Clean separation** between UI, session management, and game logic
- **Command Pattern** for player-to-game communication

---

## Architecture Levels

### 1. Local Container (UI Layer)
**Location**: Client.unity scene  
**Responsibility**: Non-networked, client-specific UI and systems

**Components**:
- `SessionLobbyUI` - Main lobby interface with StateMachine
- `PseudoUI` (deprecated, merged into SessionLobbyUI)
- `NetworkBootstrap` - Client connection UI
- Menu navigation and UI transitions

**Characteristics**:
- **Not session-specific** - handles multiple sessions from UI perspective
- No game logic or session state
- UI Toolkit (UXML/USS) for all interfaces
- Centralized in `Assets/UI Toolkit/`

---

### 2. Session Container (Session Layer)
**Location**: `Assets/Scripts/Core/Games/SessionContainer.cs`  
**Responsibility**: Isolated session management and player authorization

**Key Features**:
```csharp
public class SessionContainer : IDisposable
{
    // Identification
    public string SessionName { get; }
    public string SessionId { get; }
    public Vector3 WorldOffset { get; }
    
    // Player management
    private Dictionary<ulong, SessionPlayer> players;
    private HashSet<ulong> authorizedClients;
    
    // Game Container (encapsulates actual game instance)
    private GameContainer gameContainer;
    public GameContainer Game => gameContainer;
    
    // State management
    public enum SessionState { Lobby, Starting, InGame, Ended }
    public SessionState State { get; }
}
```

**Authorization Pattern**:
```csharp
// Always validate access before operations
if (!container.ValidateAccess(clientId, "OperationName"))
    return;
```

**Lifecycle**:
1. **Created**: When host creates session via `CreateSessionServerRpc`
2. **Players Join**: Via `JoinSessionServerRpc` with authorization checks
3. **Game Starts**: Creates `GameContainer` and loads Game scene
4. **Game Ends**: Cleans up `GameContainer` and despawns pawns
5. **Disposed**: When session is destroyed

---

### 3. Game Container (Game Instance Layer)
**Location**: `Assets/Scripts/Core/Games/GameContainer.cs`  
**Responsibility**: Encapsulates single game instance within a session

**Key Features**:
```csharp
public class GameContainer
{
    // Scene reference
    public Scene GameScene { get; private set; }
    
    // Game-specific components
    private Camera gameCamera;
    private Transform mapRoot;
    private Dictionary<ulong, NetworkObject> playerPawns;
    private Dictionary<ulong, PlayerGameState> playerStates;
    
    // Command pattern for player actions
    private CommandInvoker commandInvoker;
    
    // Methods
    public void InitializeGameScene(Scene scene, Camera camera, Transform mapRoot);
    public void RegisterPawn(ulong clientId, NetworkObject pawn);
    public void ExecutePlayerCommand(IPlayerCommand command);
    public void FocusCameraOnPlayer(ulong clientId);
}
```

**Player Game State**:
```csharp
public class PlayerGameState
{
    public ulong ClientId { get; set; }
    public Vector3 Position { get; set; }
    public int Score { get; set; }
    public bool IsAlive { get; set; }
    public float LastUpdateTime { get; set; }
}
```

**Lifecycle**:
1. **Created**: In `SessionContainer.StartGame()` when game starts
2. **Scene Loaded**: Game.unity loaded additively via `StartGameClientRpc`
3. **Initialized**: `InitializeGameScene()` called with scene components
4. **Pawns Registered**: As players spawn via `RegisterPawn()`
5. **Commands Executed**: Player actions via `ExecutePlayerCommand()`
6. **Cleaned Up**: When session ends or game restarts

---

## Command Pattern

### Purpose
Encapsulate player actions as command objects for:
- **Decoupling**: Separate player intent from game execution
- **Undo/Replay**: Support for action history
- **Network**: Easy serialization of player actions
- **Testing**: Mock commands without full game setup

### Interface
**Location**: `Assets/Scripts/Core/Commands/IPlayerCommand.cs`

```csharp
public interface IPlayerCommand
{
    void Execute();
    void Undo();
    ulong ClientId { get; }
}
```

### Implementations

#### 1. MovePlayerCommand
```csharp
public class MovePlayerCommand : IPlayerCommand
{
    private readonly GameContainer gameContainer;
    private readonly Vector2 direction;
    private readonly float speed;
    
    public void Execute()
    {
        var pawn = gameContainer.GetPlayerPawn(ClientId);
        if (pawn != null)
        {
            Vector3 movement = new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;
            pawn.transform.position += movement;
        }
    }
}
```

#### 2. PlayerActionCommand
```csharp
public class PlayerActionCommand : IPlayerCommand
{
    private readonly string actionType; // "jump", "shoot", "interact"
    
    public void Execute()
    {
        switch (actionType)
        {
            case "jump":
                // Execute jump logic
                break;
            case "shoot":
                // Execute shoot logic
                break;
        }
    }
}
```

### CommandInvoker
Manages command queue and execution:

```csharp
public class CommandInvoker
{
    private Queue<IPlayerCommand> commandQueue = new Queue<IPlayerCommand>();
    private Stack<IPlayerCommand> commandHistory = new Stack<IPlayerCommand>();
    
    public void ExecuteCommand(IPlayerCommand command);
    public void ProcessQueue();
    public void UndoLastCommand();
}
```

### Usage Example
```csharp
// Server receives movement request
[Rpc(SendTo.Server, RequireOwnership = false)]
public void RequestMoveServerRpc(Vector2 direction, RpcParams rpcParams)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    var session = GetSessionForClient(clientId);
    
    // Create command
    var moveCommand = new MovePlayerCommand(session.Game, clientId, direction);
    
    // Execute via GameContainer
    session.Game.ExecutePlayerCommand(moveCommand);
}
```

---

## Scene Architecture

### Client.unity Scene
**Purpose**: Main menu, lobby UI, client bootstrapping

**Contains**:
- `SessionLobbyUI` (with StateMachine)
- `NetworkBootstrap` UI
- `UIManager` singleton
- Local client systems

**Does NOT contain**:
- Game camera (moved to Game.unity)
- Game pawns (spawned in Game.unity)
- Game-specific UI (in Game.unity)

---

### Game.unity Scene
**Purpose**: Actual gameplay, loaded additively when game starts

**Contains**:
- **Main Camera** (Orthographic, 2D)
- **Scene GameObject** with `GameManager`
- **NetworkManagerRoot** prefab (network systems)
- **GameCanvas** with `GameCanvasRoot`
- **GameCanvasManager** prefab

**Loading Flow**:
```csharp
// 1. Server starts game
SessionContainer.StartGame(gameId, hostClientId);
// Creates GameContainer internally

// 2. Server sends StartGameClientRpc to clients
StartGameClientRpc(sessionName, gameId, worldOffset, clientRpcParams);

// 3. Client loads Game scene
SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);

// 4. After load, client initializes systems
GameRegistry.GetGame(gameId).SetupClientVisuals();
PlayerInputHandler.SetSession(sessionName);
GameDebugUI.SetSession(sessionName);
```

**Unloading Flow**:
```csharp
// When game ends or session closes
SceneManager.UnloadSceneAsync("Game");
```

---

## Data Flow Examples

### Player Movement
```
1. Client Input
   PlayerInputHandler detects WASD input
   ↓
2. RPC Request
   SessionRpcHub.RequestMoveServerRpc(direction)
   ↓
3. Server Authorization
   SessionContainer.ValidateAccess(clientId, "Move")
   ↓
4. Command Creation
   MovePlayerCommand(gameContainer, clientId, direction)
   ↓
5. Command Execution
   gameContainer.ExecutePlayerCommand(command)
   ↓
6. Pawn Update
   pawn.transform.position += movement
   ↓
7. Network Sync
   NetworkObject syncs position to all clients
```

### Game Start Sequence
```
1. Host Requests Start
   SessionLobbyUI.OnStartGame() → StartGameServerRpc()
   ↓
2. Server Validation (8 checks)
   - Session exists
   - Requesting client is host
   - Minimum players (2)
   - All players ready
   - State is Lobby
   - Game ID valid
   - Session state allows start
   - No active game
   ↓
3. Session Container Initialization
   SessionContainer.StartGame(gameId, hostClientId)
   Creates GameContainer
   ↓
4. Scene Load on Clients
   StartGameClientRpc() → LoadGameSceneAndInitialize()
   SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive)
   ↓
5. Client Setup
   - SetupClientVisuals()
   - Initialize PlayerInputHandler
   - Setup SessionPawnVisibility
   - Show GameDebugUI
   ↓
6. Pawn Spawning
   GameInstanceManager.CreateGame()
   → SpawnPlayers()
   → SessionContainer.RegisterPawn()
   → GameContainer.RegisterPawn()
   ↓
7. State Transition
   SessionContainer.SetGameRunning()
   State → InGame
```

---

## Network Patterns

### RPC Naming Convention
```csharp
// Client → Server (no ownership required)
[Rpc(SendTo.Server, RequireOwnership = false)]
public void ActionNameServerRpc(params, RpcParams rpcParams)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    // Always extract clientId from RpcParams, never trust parameters
}

// Server → Specific Clients
[ClientRpc]
private void ActionNameClientRpc(params, ClientRpcParams clientRpcParams)
{
    // Targeted send to specific clients
}
```

### ClientRpcParams Pattern
```csharp
// Build targeted RPC params
private ClientRpcParams BuildClientRpcParams(List<ulong> targetClientIds)
{
    var ids = new ulong[targetClientIds.Count];
    for (int i = 0; i < targetClientIds.Count; i++)
        ids[i] = targetClientIds[i];
        
    return new ClientRpcParams
    {
        Send = new ClientRpcSendParams
        {
            TargetClientIds = ids
        }
    };
}
```

---

## Integration Points

### Adding New Game Types
1. Implement `IGameDefinition` or extend `GameDefinitionAsset`
2. Register in `GameRegistry` (auto-registered for ScriptableObjects)
3. Implement required methods:
   - `SetupGame(Vector3 worldOffset)` - Server-side setup
   - `GetSpawnPosition(int playerIndex, int totalPlayers, Vector3 worldOffset)`
   - `InitializePawn(NetworkObject pawn, string playerName, int playerIndex, Vector3 worldOffset)`
   - `SetupClientVisuals()` - Client camera/UI setup
4. Create pawn prefab with `NetworkObject` component
5. Position all objects relative to `worldOffset`

### Adding Player Features
Use `IPlayerFeature` interface for modular player capabilities:

```csharp
public interface IPlayerFeature
{
    void Initialize(ulong clientId, SessionContainer session);
    void OnGameStart();
    void OnGameEnd();
    void Cleanup();
}
```

---

## Testing

### Session Isolation Tests
**Location**: `Assets/Scripts/Tests/`  
**Run**: `run_tests.bat [quick|normal|stress|custom]`

**Validates**:
- Cross-session player isolation (no unauthorized access)
- WorldOffset spatial separation (player bounds checking)
- Authorization checks (ValidateAccess pattern)
- Concurrent session stability (multiple sessions simultaneously)

**Test Modes**:
```batch
run_tests.bat quick      # 2 sessions, 2 players
run_tests.bat normal     # 5 sessions, 4 players
run_tests.bat stress     # 50 sessions, 8 players
run_tests.bat custom 10 8  # Custom: 10 sessions, 8 players
```

---

## Migration Guide

### From Single-Scene to Multi-Scene

**Before (Old Architecture)**:
```csharp
// Everything in Client.unity
// Game setup directly in scene
IGameDefinition.SetupClientVisuals() creates GameObjects
Camera in Client.unity
```

**After (New Architecture)**:
```csharp
// Client.unity = Lobby/Menu UI only
// Game.unity = Loaded additively for gameplay
// GameContainer encapsulates game instance
// Camera in Game.unity

// OLD: Direct GameObject creation in SetupClientVisuals()
public void SetupClientVisuals()
{
    var camera = new GameObject("Camera"); // ❌ Don't do this with ScriptableObject
}

// NEW: Reference scene components
public void SetupClientVisuals()
{
    var gameCamera = Camera.main; // ✅ Reference existing camera
}
```

### WorldOffset Positioning
**Always position game objects relative to session's WorldOffset**:

```csharp
// ✅ CORRECT
Vector3 worldPosition = localPosition + container.WorldOffset;
gameObject.transform.position = worldPosition;

// ❌ WRONG
gameObject.transform.position = localPosition; // Ignores isolation
```

---

## Common Patterns

### Session Operation Pattern
```csharp
// 1. Authorization
if (!sessionContainer.ValidateAccess(clientId, "Operation"))
    return;

// 2. State Check
if (sessionContainer.State != SessionState.InGame)
    return;

// 3. Operation
sessionContainer.Game.ExecutePlayerCommand(command);
```

### Error Handling Pattern
```csharp
// Use session events for error propagation
sessionContainer.OnError += (session, error) => {
    Debug.LogError($"[Session:{session.SessionName}] {error}");
    // Send error to client via ClientRpc
};
```

### Logging Pattern
```csharp
// Include session/component identifier
Debug.Log($"[ComponentName:{SessionId}] Message");
Debug.Log($"[GameContainer:{SessionName}] Message");
Debug.Log($"[SessionRpcHub] Message"); // For singleton components
```

---

## File Organization

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Commands/          # Command Pattern (IPlayerCommand, CommandInvoker)
│   │   ├── Games/             # Session & Game containers, GameRegistry
│   │   │   ├── SessionContainer.cs
│   │   │   ├── GameContainer.cs
│   │   │   ├── IGameDefinition.cs
│   │   │   └── GameRegistry.cs
│   │   ├── Players/           # IPlayerFeature interface
│   │   ├── SceneNames.cs      # Scene name constants
│   │   └── StateMachine.cs    # Generic state machine
│   ├── Games/                 # Game implementations (CircleGame, SquareGame)
│   ├── Networking/
│   │   ├── Player/
│   │   │   └── SessionRpcHub.cs  # RPC hub for session operations
│   │   ├── Server/
│   │   │   └── ServerBootstrap.cs
│   │   └── Client/
│   │       └── ClientBootstrap.cs
│   ├── UI/                    # UI controllers
│   │   ├── SessionLobbyUI.cs  # Main lobby with StateMachine
│   │   ├── GameDebugUI.cs     # In-game debug overlay
│   │   ├── Common/            # UIManager, ToastNotification, ProgressIndicator
│   │   └── Presenters/        # MVP pattern presenters
│   └── Tests/                 # Session isolation tests
├── Scenes/
│   ├── Client.unity           # Lobby/Menu scene (always loaded)
│   ├── Game.unity             # Game scene (loaded additively)
│   ├── Menu.unity             # Main menu
│   └── Server.unity           # Dedicated server scene
├── Prefabs/
│   ├── Network/               # SessionRpcHub, NetworkManager, DefaultPlayer
│   └── Pawns/                 # CirclePawn, SquarePawn (game-specific)
└── UI Toolkit/                # UXML/USS files (centralized)
    ├── SessionLobby.uxml/uss
    ├── NetworkBootstrap.uxml
    └── ...
```

---

## Performance Considerations

### Session Isolation Overhead
- Each `SessionContainer` has isolated dictionaries for players and pawns
- Thread-safe locks (`playerLock`, `pawnLock`) for concurrent access
- **Recommendation**: Limit to 50-100 concurrent sessions per server

### Scene Loading
- `LoadSceneMode.Additive` - keeps Client.unity loaded
- **First load**: ~200-500ms (includes asset loading)
- **Subsequent loads**: ~50-100ms (cached)
- Unload scene when game ends to free memory

### Command Pattern Performance
- `CommandInvoker` uses `Queue<T>` - O(1) enqueue/dequeue
- `Stack<T>` for undo history - O(1) push/pop
- **Recommendation**: Limit history to 100-500 commands

---

## Troubleshooting

### Issue: "Session not found"
**Cause**: Client trying to access session they're not authorized for  
**Fix**: Always call `sessionContainer.ValidateAccess(clientId, operation)`

### Issue: "Pawns not visible"
**Cause**: `SessionPawnVisibility` filtering incorrect session  
**Fix**: Ensure `SessionPawnVisibility.SetLocalSession(sessionName)` called after scene load

### Issue: "Game scene loads but camera is black"
**Cause**: Camera not initialized in GameContainer  
**Fix**: Call `gameContainer.InitializeGameScene(scene, Camera.main, mapRoot)`

### Issue: "Commands not executing"
**Cause**: CommandInvoker queue not being processed  
**Fix**: Call `commandInvoker.ProcessQueue()` in `Update()` or via coroutine

### Issue: "WorldOffset not applied"
**Cause**: Direct positioning without offset  
**Fix**: Always add `container.WorldOffset` to local positions

---

## Future Enhancements

### Planned Features
1. **Scene Pooling**: Pre-load Game scenes for faster startup
2. **Command Replay**: Save command history for replays
3. **Dynamic Scene Selection**: Different scenes for different game types
4. **Advanced Undo/Redo**: Full action history with UI
5. **Session Persistence**: Save/load session state
6. **Spectator Mode**: Observer clients without pawns

### Extension Points
- **IPlayerFeature**: Add modular player capabilities (inventory, chat, etc.)
- **IGameDefinition**: New game types with custom rules
- **IPlayerCommand**: New command types (trade, emote, etc.)
- **SessionContainer Events**: Subscribe to lifecycle events

---

## References

- **Architecture Diagram**: `ARCHITECTURE.md` (includes Mermaid class diagram)
- **Server Commands**: `SERVER_COMMANDS.txt`
- **Test Documentation**: `Assets/Scripts/Tests/README.md`
- **Copilot Instructions**: `.github/copilot-instructions.md`
- **Improvements Log**: `IMPROVEMENTS.md`

---

## Key Takeaways

✅ **3 Levels**: Local (UI) → Session (logic) → Game (instance)  
✅ **Command Pattern**: Encapsulate player actions  
✅ **Multi-Scene**: Client.unity + Game.unity (additive)  
✅ **Session Isolation**: WorldOffset + authorization checks  
✅ **Network Patterns**: [Rpc(SendTo.Server)] + ClientRpcParams  
✅ **Scene Loading**: LoadSceneAsync(LoadSceneMode.Additive)  
✅ **Thread Safety**: Locks on shared session data  

❌ **Don't**: Create GameObjects in ScriptableObject methods  
❌ **Don't**: Forget WorldOffset when positioning  
❌ **Don't**: Skip authorization checks in SessionContainer  
❌ **Don't**: Mix UI and game logic in same scene  
❌ **Don't**: Use cross-session player lookups  
