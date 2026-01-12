# AI Coding Agent Instructions

## Project Overview
Unity multiplayer game framework with **3-level container architecture** and **session isolation** - multiple concurrent game sessions on one server, each with isolated player data, world space, and game state.

### Architecture Hierarchy
```
Local Container (UI/Client-specific)
  ↓
Session Container (Session logic & player management)
  ↓
Game Container (Game instance with dedicated Game.unity scene)
```

**See [ARCHITECTURE_3_LEVEL.md](../ARCHITECTURE_3_LEVEL.md) for comprehensive architecture documentation.**

## Architecture

### Session Isolation System
- **SessionContainer** (`Assets/Scripts/Core/Games/SessionContainer.cs`): Session-level isolation boundary
  - Unique `WorldOffset` (Vector3) for spatial separation
  - Isolated player registry with authorization checks via `IsAuthorized(clientId)`
  - Encapsulates **GameContainer** for actual game instance
  - Private game state and pawn management
- **GameContainer** (`Assets/Scripts/Core/Games/GameContainer.cs`): Game instance encapsulation
  - Manages Game.unity scene (loaded additively)
  - Camera, map, player pawns, and game state
  - Command Pattern for player actions via `IPlayerCommand`
- Authorization pattern: Always call `container.ValidateAccess(clientId, operation)` before session operations
- World positioning: All game objects positioned relative to `container.WorldOffset`

### Game Plugin System
- Games implement `IGameDefinition` interface or inherit from `GameDefinitionAsset` ScriptableObject
- Register games: `GameRegistry.Register(gameDefinition)` - auto-initialized on server start
- Key methods to implement:
  - `SetupGame(Vector3 worldOffset)` - server-side game setup
  - `GetSpawnPosition(playerIndex, totalPlayers, worldOffset)` - calculate spawn with offset
  - `InitializePawn(pawn, playerName, playerIndex, worldOffset)` - configure spawned pawn
  - `SetupClientVisuals()` - client-side camera/UI setup
- Example: [CircleGameDefinition.cs](Assets/Scripts/Games/CircleGame/CircleGameDefinition.cs)

### Networking (Unity Netcode for GameObjects)
- **ServerRpc**: Client→Server, use `[Rpc(SendTo.Server, RequireOwnership = false)]` for hub calls
- **ClientRpc**: Server→Client, use `ClientRpcParams` for targeted sends
- **NetworkVariables**: Server authority with read permission Everyone
  ```csharp
  new NetworkVariable<T>(writePerm: NetworkVariableWritePermission.Server)
  ```
- RPC Hub: [SessionRpcHub.cs](Assets/Scripts/Networking/Player/SessionRpcHub.cs) - session management RPCs
- **RPC Pattern**: Extract `clientId` from `rpcParams.Receive.SenderClientId`

### UI Architecture
- **UI Toolkit**: UXML/USS files centralized in `Assets/UI Toolkit/`
- **State Machine**: Generic `StateMachine<TState>` for UI state management (see SessionLobbyUI)
- **Toast System**: 4 types (Info/Success/Warning/Error) with slide-in animations
- **Progress Indicator**: Fullscreen overlay with 5-phase sequences
- **Validation**: GameStartValidation with 8 typed failure reasons
- **Pattern**: Controller (C#) in `Assets/Scripts/UI/`, View (UXML/USS) in `Assets/UI Toolkit/`

### Bootstrap Pattern
- **Server**: [ServerBootstrap.cs](Assets/Scripts/Networking/Server/ServerBootstrap.cs)
  - Parses command-line args: `-port`, `-maxplayers`
  - Auto-starts server, spawns SessionRpcHub, creates GameSessionManager
- **Client**: ClientBootstrap.cs connects via `UnityTransport.SetConnectionData(ip, port)`

### Command Pattern (Player Actions)
- **IPlayerCommand** (`Assets/Scripts/Core/Commands/IPlayerCommand.cs`): Interface for player actions
  - `Execute()` - Execute command
  - `Undo()` - Undo command (for replay/history)
  - `ClientId` - Player who issued command
- **Implementations**:
  - `MovePlayerCommand` - Player movement with direction and speed
  - `PlayerActionCommand` - Generic actions (jump, shoot, interact)
- **CommandInvoker**: Manages command queue and execution history
- **Usage**: `gameContainer.ExecutePlayerCommand(new MovePlayerCommand(...))`

### Scene Architecture
- **Client.unity**: Lobby/Menu UI, always loaded (Local Container)
- **Game.unity**: Gameplay scene, loaded additively when game starts (Game Container)
  - Contains: Main Camera (orthographic), GameManager, NetworkManagerRoot, GameCanvas, GameDebugUI
  - Loading: `SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive)` in `StartGameClientRpc`
  - Unloading: `SceneManager.UnloadSceneAsync("Game")` when game ends
- **Server.unity**: Dedicated server scene with ServerBootstrap

### Directory Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Session isolation, game registry, player features, commands
│   │   ├── Commands/   # IPlayerCommand, CommandInvoker, MovePlayerCommand
│   │   ├── Games/      # SessionContainer, GameContainer, GameRegistry, IGameDefinition
│   │   └── Players/    # IPlayerFeature interface
│   ├── Games/          # Game implementations (CircleGame, SquareGame)
│   ├── Networking/     # SessionRpcHub, bootstraps, player sync
│   ├── UI/             # UI controllers (SessionLobbyUI, GameDebugUI, etc.)
│   │   ├── Common/     # UIManager, UIColors, ToastNotification, ProgressIndicator
│   │   ├── Presenters/ # MVP pattern presenters
│   └── Tests/          # Session isolation tests
├── Scenes/
│   ├── Client.unity    # Lobby/Menu scene (Local Container - always loaded)
│   ├── Game.unity      # Game scene (Game Container - loaded additively)
│   ├── Menu.unity      # Main menu
│   └── Server.unity    # Dedicated server scene
├── UI Toolkit/         # UXML/USS files (centralized)
│   ├── SessionLobby.uxml/uss
│   └── NetworkBootstrap*.uxml
└── Prefabs/
    ├── Network/        # SessionRpcHub, DefaultPlayer, NetworkManager
    └── Pawns/          # CirclePawn, SquarePawn (game-specific)
```

## Development Workflows

### Building Dedicated Server
```powershell
# Unity Editor: File → Build Profiles → Server → Build
# Output: Build/Server/Server.exe (Windows) or Server.x86_64 (Linux)
```

### Running Dedicated Server
See [SERVER_COMMANDS.txt](SERVER_COMMANDS.txt) for all commands. Common patterns:
```powershell
# Windows
Server.exe -batchmode -nographics -port 7777 -maxplayers 32

# Linux production (systemd service)
/opt/gameserver/Server.x86_64 -batchmode -nographics -port 7777
```

### Testing Session Isolation
Run automated tests via [run_tests.bat](run_tests.bat):
```batch
run_tests.bat quick      # 2 sessions, 2 players
run_tests.bat normal     # 5 sessions, 4 players  
run_tests.bat stress     # 50 sessions, 8 players
run_tests.bat custom 10 8  # Custom: 10 sessions, 8 players
```
Tests validate cross-session isolation, authorization, world bounds. See [Tests/README.md](Assets/Scripts/Tests/README.md).

## Conventions & Patterns

### Logging
- Format: `[ComponentName:SessionId]` or `[ComponentName]` for singletons
- Example: `Debug.Log($"[SessionContainer:{SessionId}] Player joined");`
- Server startup disables stack traces for Log/Warning (see `ServerBootstrap.InitializeLogging()`)

### Session Operations
When adding session functionality:
1. Add method to `SessionContainer` with authorization check
2. Add ServerRpc to `SessionRpcHub` calling the container method
3. Extract `clientId` from `ServerRpcParams.Receive.SenderClientId`
4. Return errors via `SendSessionErrorClientRpc` with targeted ClientRpcParams

### Adding New Games
1. Create ScriptableObject inheriting `GameDefinitionAsset`
2. Implement game-specific logic (setup, spawning, visuals)
3. Create pawn prefab with `NetworkObject` component
4. Asset auto-registers via `GameRegistry.Initialize()` on server start
5. Reference in scene: Add to Resources or serialize in GameRegistry

### Thread Safety
`SessionContainer` uses locks: `playerLock`, `pawnLock` for concurrent access. Follow pattern when adding new shared state.

## Key Files Reference
- **Session isolation logic**: [SessionContainer.cs](Assets/Scripts/Core/Games/SessionContainer.cs)
- **Game instance encapsulation**: [GameContainer.cs](Assets/Scripts/Core/Games/GameContainer.cs)
- **Command Pattern**: [IPlayerCommand.cs](Assets/Scripts/Core/Commands/IPlayerCommand.cs)
- **Game plugin interface**: [IGameDefinition.cs](Assets/Scripts/Core/Games/IGameDefinition.cs)
- **Server startup**: [ServerBootstrap.cs](Assets/Scripts/Networking/Server/ServerBootstrap.cs)
- **RPC hub**: [SessionRpcHub.cs](Assets/Scripts/Networking/Player/SessionRpcHub.cs)
- **Architecture diagram**: [ARCHITECTURE_3_LEVEL.md](ARCHITECTURE_3_LEVEL.md) - comprehensive 3-level container documentation

## Common Pitfalls
- ❌ Don't forget `worldOffset` when positioning game objects/pawns
- ❌ Don't skip authorization checks in SessionContainer operations
- ❌ Don't use cross-session player lookups - always operate within one SessionContainer
- ❌ Don't create GameObjects in `SetupClientVisuals()` when running as ScriptableObject asset
- ❌ Don't use obsolete `[ServerRpc(RequireOwnership = false)]` - use `[Rpc(SendTo.Server, RequireOwnership = false)]`
- ❌ Don't scatter UXML/USS files - keep them in `Assets/UI Toolkit/`
- ❌ Don't use CSS animations in UI Toolkit - use C# coroutines instead
- ❌ Don't load Game.unity in LoadSceneMode.Single - always use LoadSceneMode.Additive
- ❌ Don't forget to initialize GameContainer when SessionContainer.StartGame() is called
- ❌ Don't skip registering pawns with both SessionContainer AND GameContainer
