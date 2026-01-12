# AI Coding Agent Instructions

## Project Overview
Unity multiplayer game framework with **session isolation** - multiple concurrent game sessions on one server, each with isolated player data, world space, and game state.

## Architecture

### Session Isolation System
- **SessionContainer** (`Assets/Scripts/Core/Games/SessionContainer.cs`): Core isolation boundary. Each session has:
  - Unique `WorldOffset` (Vector3) for spatial separation
  - Isolated player registry with authorization checks via `IsAuthorized(clientId)`
  - Private game state and pawn management
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
- **ServerRpc**: Client→Server, use `[ServerRpc(RequireOwnership = false)]` for hub calls
- **ClientRpc**: Server→Client, use `ClientRpcParams` for targeted sends
- **NetworkVariables**: Server authority with read permission Everyone
  ```csharp
  new NetworkVariable<T>(writePerm: NetworkVariableWritePermission.Server)
  ```
- RPC Hub: [SessionRpcHub.cs](Assets/Scripts/Networking/Player/SessionRpcHub.cs) - session management RPCs

### Bootstrap Pattern
- **Server**: [ServerBootstrap.cs](Assets/Scripts/Networking/Server/ServerBootstrap.cs)
  - Parses command-line args: `-port`, `-maxplayers`
  - Auto-starts server, spawns SessionRpcHub, creates GameSessionManager
- **Client**: ClientBootstrap.cs connects via `UnityTransport.SetConnectionData(ip, port)`

### Directory Structure
```
Assets/Scripts/
├── Core/           # Session isolation, game registry, player features
│   ├── Games/      # SessionContainer, GameRegistry, IGameDefinition
│   └── Players/    # IPlayerFeature interface
├── Games/          # Game implementations (CircleGame, SquareGame)
├── Networking/     # SessionRpcHub, bootstraps, player sync
├── UI/             # SessionLobbyUI, menu systems
└── Tests/          # Session isolation tests (see below)
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
- **Game plugin interface**: [IGameDefinition.cs](Assets/Scripts/Core/Games/IGameDefinition.cs)
- **Server startup**: [ServerBootstrap.cs](Assets/Scripts/Networking/Server/ServerBootstrap.cs)
- **Architecture diagram**: [ARCHITECTURE.md](ARCHITECTURE.md) - includes Mermaid class diagram and data flows

## Common Pitfalls
- ❌ Don't forget `worldOffset` when positioning game objects/pawns
- ❌ Don't skip authorization checks in SessionContainer operations
- ❌ Don't use cross-session player lookups - always operate within one SessionContainer
- ❌ Don't create GameObjects in `SetupClientVisuals()` when running as ScriptableObject asset
