# Architecture du Layer RPC - Diagramme

## Vue d'Ensemble

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT                                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│   SessionLobbyUI  →  SessionRpcHub.CreateSessionServerRpc()     │
│   SessionLobbyUI  →  SessionRpcHub.JoinSessionServerRpc()       │
│   SessionLobbyUI  →  SessionRpcHub.StartGameServerRpc()         │
│   PlayerInput     →  SessionRpcHub.RequestMoveServerRpc()       │
│                                                                   │
└───────────────────────────────┬─────────────────────────────────┘
                                │
                        [NETWORK]
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                        SERVER                                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │           SessionRpcHub (NetworkBehaviour)              │   │
│  │                  (~200 lignes)                          │   │
│  ├─────────────────────────────────────────────────────────┤   │
│  │  [ServerRpc Methods]                                     │   │
│  │  • CreateSessionServerRpc()                             │   │
│  │  • JoinSessionServerRpc()                               │   │
│  │  • StartGameServerRpc()                                 │   │
│  │  • RequestMoveServerRpc()                               │   │
│  │  • ...                                                   │   │
│  ├─────────────────────────────────────────────────────────┤   │
│  │  [Délégation aux Handlers]                              │   │
│  │  lifecycleHandler.HandleCreateSession(...)              │   │
│  │  gameStartHandler.HandleStartGame(...)                  │   │
│  │  movementHandler.HandleRequestMove(...)                 │   │
│  └────────┬───────────┬───────────┬───────────┬────────────┘   │
│           │           │           │           │                 │
│           ▼           ▼           ▼           ▼                 │
│  ┌────────────┐ ┌───────────┐ ┌──────────┐ ┌──────────┐       │
│  │ Lifecycle  │ │GameStart  │ │Movement  │ │SceneLoad │ ...   │
│  │  Handler   │ │ Handler   │ │ Handler  │ │ Handler  │       │
│  └────────────┘ └───────────┘ └──────────┘ └──────────┘       │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │          GameSessionManager                              │   │
│  │  (Gère sessions, players, state)                        │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Hiérarchie des Classes

```
┌──────────────────────────────────────────────────────────────┐
│                     INTERFACES                                │
└──────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ISessionRpc   │    │ISession      │    │ICommand      │
│Handler       │    │Validator     │    │Handler       │
└──────────────┘    └──────────────┘    └──────────────┘
        │                     │                     │
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
        ┌─────────────────────┴─────────────────────┐
        │                                           │
        ▼                                           ▼
┌──────────────────┐                    ┌──────────────────┐
│  BaseRpcHandler  │                    │  BaseValidator   │
│  (abstract)      │                    │  (abstract)      │
├──────────────────┤                    ├──────────────────┤
│• Initialize()    │                    │• ValidateAccess()│
│• Cleanup()       │                    │• ValidateSession │
│• Log()           │                    │  Exists()        │
│• BuildClientRpc  │                    │• ValidateClient  │
│  Params()        │                    │  InSession()     │
└──────────────────┘                    └──────────────────┘
        │                                           │
        │                                           │
        └───────────────┬───────────────────────────┘
                        │
        ┌───────────────┼───────────────┬───────────┐
        │               │               │           │
        ▼               ▼               ▼           ▼
┌────────────┐  ┌────────────┐  ┌──────────┐  ┌──────────┐
│SessionLife │  │GameStart   │  │Movement  │  │SceneLoad │
│cycleHandler│  │Handler     │  │Handler   │  │Handler   │
└────────────┘  └────────────┘  └──────────┘  └──────────┘
```

## Flow de Données

### Exemple: Création de Session

```
1. CLIENT
   ↓
   SessionLobbyUI.OnCreateSession()
   ↓
   SessionRpcHub.CreateSessionServerRpc(sessionName)
   ↓
2. NETWORK (RPC)
   ↓
3. SERVER
   ↓
   SessionRpcHub.CreateSessionServerRpc() [REÇOIT]
   ↓
   [Extraction clientId]
   ulong clientId = rpcParams.Receive.SenderClientId
   ↓
   [Délégation au Handler]
   lifecycleHandler.HandleCreateSession(sessionName, clientId)
   ↓
4. HANDLER (SessionLifecycleHandler)
   ↓
   HandleCreateSession()
   ├─ Validation: sessionName non vide?
   ├─ Validation: GameSessionManager exists?
   └─ Appel: GameSessionManager.TryAddSession()
      ↓
5. MANAGER (GameSessionManager)
   ↓
   TryAddSession(clientId, sessionName)
   ├─ Vérifie: nom unique?
   ├─ Crée: SessionContainer
   └─ Ajoute: à Dictionary<string, SessionState>
      ↓
6. BROADCAST
   ↓
   SessionRpcHub.SyncSessionsClientRpc()
   ↓
7. NETWORK (RPC)
   ↓
8. CLIENT
   ↓
   SessionRpcHub.SyncSessionsClientRpc() [REÇOIT]
   ↓
   SessionsUpdated?.Invoke(sessions)
   ↓
   SessionLobbyUI met à jour la liste
```

### Exemple: Démarrage de Jeu

```
1. CLIENT
   ↓
   SessionLobbyUI.OnStartGame()
   ↓
   SessionRpcHub.StartGameServerRpc(sessionName)
   ↓
2. NETWORK (RPC)
   ↓
3. SERVER
   ↓
   SessionRpcHub.StartGameServerRpc() [REÇOIT]
   ↓
   [Délégation au Handler]
   gameStartHandler.HandleStartGame(sessionName, clientId)
   ↓
4. HANDLER (GameStartHandler)
   ↓
   HandleStartGame()
   ├─ [VALIDATION PHASE]
   │  ├─ GameStartValidator.ValidateGameStart()
   │  ├─ Check: client in session?
   │  ├─ Check: client is host?
   │  ├─ Check: enough players?
   │  ├─ Check: all ready?
   │  └─ Check: valid game type?
   │
   ├─ [SI VALIDATION ÉCHOUE]
   │  └─ SendGameStartFailedClientRpc()
   │
   └─ [SI VALIDATION RÉUSSIT]
      └─ StartGameForPlayers()
         ├─ Initialise GameRegistry
         ├─ Crée GameInstanceManager
         ├─ Résout player names (ResolvePlayerName)
         ├─ Détermine worldOffset
         ├─ GameInstanceManager.CreateGame()
         └─ SessionRpcHub.StartGameClientRpc() [BROADCAST]
            ↓
5. NETWORK (Targeted ClientRpc)
   ↓
6. CLIENTS (session players only)
   ↓
   SessionRpcHub.StartGameClientRpc() [REÇOIT]
   ↓
   [Délégation au SceneLoadHandler]
   sceneLoadHandler.LoadGameSceneAndInitialize()
   ├─ SceneManager.LoadSceneAsync("Game", Additive)
   ├─ Initialize GameRegistry
   ├─ GameDefinition.SetupClientVisuals()
   ├─ PlayerInputHandler.SetSession()
   ├─ SessionPawnVisibility.SetLocalSession()
   └─ GameDebugUI.Show()
      ↓
7. GAME RUNNING
```

### Exemple: Mouvement Joueur

```
1. CLIENT
   ↓
   PlayerInputHandler (Update loop)
   ↓
   Détecte: WASD input
   ↓
   SessionRpcHub.RequestMoveServerRpc(sessionName, direction)
   ↓
2. NETWORK (RPC)
   ↓
3. SERVER
   ↓
   SessionRpcHub.RequestMoveServerRpc() [REÇOIT]
   ↓
   [Délégation au Handler]
   movementHandler.HandleRequestMove(sessionName, direction, clientId)
   ↓
4. HANDLER (PlayerMovementHandler)
   ↓
   HandleRequestMove()
   ├─ [SECURITY CHECKS]
   │  ├─ Authoritative session resolution
   │  ├─ Rate limiting (20 req/s max)
   │  └─ Input magnitude clamping
   │
   ├─ [VALIDATION]
   │  └─ GameSessionManager.ValidateClientAccess()
   │
   └─ [EXECUTION]
      ├─ Get SessionContainer
      ├─ Create MovePlayerCommand
      └─ GameContainer.ExecutePlayerCommand()
         ↓
5. COMMAND PATTERN
   ↓
   MovePlayerCommand.Execute()
   ├─ Get pawn NetworkObject
   ├─ Calculate movement vector
   └─ Update pawn.transform.position
      ↓
6. NETWORK TRANSFORM (automatic sync)
   ↓
7. ALL CLIENTS
   ↓
   NetworkTransform syncs position
   ↓
   Pawn moves on all clients
```

## Structure des Fichiers

```
Assets/Scripts/Networking/
│
├── Player/
│   └── SessionRpcHub.cs                    (200 lignes, délégation)
│
├── RpcHandlers/
│   ├── Interfaces/
│   │   └── ISessionRpcHandler.cs           (108 lignes)
│   │       ├── ISessionRpcHandler
│   │       ├── ISessionValidator
│   │       ├── ICommandHandler
│   │       ├── ValidationResult
│   │       └── ValidationErrorCode
│   │
│   ├── Base/
│   │   └── BaseRpcHandler.cs               (201 lignes)
│   │       ├── BaseRpcHandler (abstract)
│   │       │   ├── Initialize()
│   │       │   ├── Cleanup()
│   │       │   ├── Log() / LogWarning() / LogError()
│   │       │   └── BuildClientRpcParams()
│   │       │
│   │       └── BaseValidator (abstract)
│   │           ├── ValidateAccess()
│   │           ├── ValidateSessionExists()
│   │           ├── ValidateClientInSession()
│   │           └── ValidateIsHost()
│   │
│   └── Handlers/
│       ├── SessionLifecycleHandler.cs      (107 lignes)
│       │   ├── HandleCreateSession()
│       │   ├── HandleJoinSession()
│       │   ├── HandleLeaveSession()
│       │   └── HandleSetReady()
│       │
│       ├── GameStartHandler.cs             (291 lignes)
│       │   ├── HandleStartGame()
│       │   ├── HandleSetGameType()
│       │   ├── GameStartValidator
│       │   └── StartGameForPlayers()
│       │
│       ├── PlayerMovementHandler.cs        (141 lignes)
│       │   ├── HandleRequestMove()
│       │   ├── ExecuteCommand()
│       │   └── CanHandleCommand()
│       │
│       ├── SceneLoadHandler.cs             (146 lignes)
│       │   ├── LoadGameSceneAndInitialize()
│       │   ├── HandleRegisterPawnSession()
│       │   └── HandleLateJoiner()
│       │
│       └── SessionQueryHandler.cs          (54 lignes)
│           ├── HandleRequestSessions()
│           └── HandleRequestSessionDetails()
│
├── Data/
│   ├── PlayerNetworkData.cs                (177 lignes)
│   └── ClientNetworkData.cs                (65 lignes)
│
└── Sessions/
    └── GameSessionManager.cs               (527 lignes)
```

## Responsabilités par Handler

### SessionLifecycleHandler
```
┌─────────────────────────────────────┐
│  SessionLifecycleHandler            │
├─────────────────────────────────────┤
│ • CreateSession                     │
│ • JoinSession                       │
│ • LeaveSession                      │
│ • SetReady                          │
├─────────────────────────────────────┤
│ Interactions:                       │
│ → GameSessionManager.TryAddSession()│
│ → GameSessionManager.TryJoinSession│
│ → GameSessionManager.LeaveSession() │
│ → GameSessionManager.SetReady()    │
└─────────────────────────────────────┘
```

### GameStartHandler
```
┌─────────────────────────────────────┐
│  GameStartHandler                   │
├─────────────────────────────────────┤
│ • StartGame (avec validation 8 pts) │
│ • SetGameType                       │
│ • GameStartValidator                │
├─────────────────────────────────────┤
│ Validations:                        │
│ ✓ Client in session?                │
│ ✓ Client is host?                   │
│ ✓ Enough players?                   │
│ ✓ All ready?                        │
│ ✓ Valid game type?                  │
│ ✓ Minimum players met?              │
│ ✓ Session state allows start?      │
│ ✓ No active game?                   │
├─────────────────────────────────────┤
│ Interactions:                       │
│ → GameRegistry.GetGame()            │
│ → GameInstanceManager.CreateGame()  │
│ → StartGameClientRpc()              │
└─────────────────────────────────────┘
```

### PlayerMovementHandler
```
┌─────────────────────────────────────┐
│  PlayerMovementHandler              │
├─────────────────────────────────────┤
│ • HandleRequestMove                 │
│ • Rate limiting (20 req/s)          │
│ • Command Pattern support           │
├─────────────────────────────────────┤
│ Security:                           │
│ ⚠ Authoritative session resolution │
│ ⚠ Rate limiting per client         │
│ ⚠ Input magnitude clamping         │
├─────────────────────────────────────┤
│ Interactions:                       │
│ → SessionContainer.Game             │
│ → GameContainer.ExecuteCommand()    │
│ → MovePlayerCommand                 │
│ → GameInstanceManager (fallback)   │
└─────────────────────────────────────┘
```

### SceneLoadHandler
```
┌─────────────────────────────────────┐
│  SceneLoadHandler                   │
├─────────────────────────────────────┤
│ • LoadGameSceneAndInitialize        │
│ • RegisterPawnSession               │
│ • LateJoiner setup                  │
├─────────────────────────────────────┤
│ Scene Management:                   │
│ → SceneManager.LoadSceneAsync()     │
│ → GameRegistry.Initialize()         │
│ → GameDefinition.SetupClientVisuals│
├─────────────────────────────────────┤
│ Client Systems:                     │
│ → PlayerInputHandler.SetSession()   │
│ → SessionPawnVisibility.SetLocal()  │
│ → GameDebugUI.Show()                │
└─────────────────────────────────────┘
```

### SessionQueryHandler
```
┌─────────────────────────────────────┐
│  SessionQueryHandler                │
├─────────────────────────────────────┤
│ • RequestSessions (broadcast)       │
│ • RequestSessionDetails (targeted)  │
├─────────────────────────────────────┤
│ Interactions:                       │
│ → GameSessionManager.GetSnapshot()  │
│ → GameSessionManager.BuildDetails() │
│ → SyncSessionsClientRpc()           │
│ → SendSessionDetailsClientRpc()     │
└─────────────────────────────────────┘
```

## Diagramme de Séquence: Create + Join + Start

```
Client1 (Host)      SessionRpcHub      LifecycleHandler    GameStartHandler    GameSessionManager
    │                     │                    │                   │                    │
    │──CreateSession────>│                    │                   │                    │
    │                     │──HandleCreate────>│                   │                    │
    │                     │                    │──TryAddSession──────────────────────>│
    │                     │                    │                   │                    │
    │                     │                    │<─────Success──────────────────────────│
    │<────Success─────────│                    │                   │                    │
    │                     │                    │                   │                    │
    │                     │                    │                   │                    │
Client2 (Player)          │                    │                   │                    │
    │                     │                    │                   │                    │
    │──JoinSession──────>│                    │                   │                    │
    │                     │──HandleJoin──────>│                   │                    │
    │                     │                    │──TryJoinSession─────────────────────>│
    │                     │                    │<─────Success──────────────────────────│
    │<────Success─────────│                    │                   │                    │
    │                     │                    │                   │                    │
    │──SetReady(true)───>│                    │                   │                    │
    │                     │──HandleSetReady──>│                   │                    │
    │                     │                    │──SetReady────────────────────────────>│
    │                     │                    │                   │                    │
    │                     │                    │                   │                    │
Client1 (Host)            │                    │                   │                    │
    │──SetReady(true)───>│                    │                   │                    │
    │                     │──HandleSetReady──>│                   │                    │
    │                     │                    │──SetReady────────────────────────────>│
    │                     │                    │                   │                    │
    │──StartGame────────>│                    │                   │                    │
    │                     │──────────────────────HandleStartGame─>│                    │
    │                     │                    │                   │──Validate────────>│
    │                     │                    │                   │<─Valid─────────────│
    │                     │                    │                   │──CreateGame──────>│
    │                     │<────────────────StartGameClientRpc─────│                    │
    │<────StartGame───────│                    │                   │                    │
Client2                   │                    │                   │                    │
    │<────StartGame───────│                    │                   │                    │
    │                     │                    │                   │                    │
```

## Patterns de Communication

### 1. Client → Server (ServerRpc)
```
SessionLobbyUI → SessionRpcHub.[Action]ServerRpc()
                      ↓
              [Extraction clientId]
                      ↓
              handler.Handle[Action]()
                      ↓
              GameSessionManager.[Action]()
```

### 2. Server → Client (ClientRpc)
```
GameSessionManager → SessionRpcHub.[Result]ClientRpc()
                      ↓
                 [Targeted Send]
                      ↓
              Client receives RPC
                      ↓
              UI updates
```

### 3. Server → All Clients (Broadcast ClientRpc)
```
GameSessionManager → SessionRpcHub.SyncSessionsClientRpc()
                      ↓
                 [Broadcast to all]
                      ↓
              All clients receive
                      ↓
              SessionsUpdated event
                      ↓
              UI updates on all clients
```

## Avantages de l'Architecture

### ✅ Séparation des Responsabilités (SRP)
- Chaque handler gère un domaine spécifique
- SessionRpcHub = orchestration uniquement
- Validation isolée dans BaseValidator

### ✅ Testabilité
- Handlers testables indépendamment
- Mocking facile (ISessionRpcHandler)
- Validation unitaire

### ✅ Extensibilité
- Nouveau handler = nouvelle feature
- Pas de modification de SessionRpcHub
- Interfaces claires

### ✅ Maintenabilité
- Code court (200 lignes au lieu de 767)
- Responsabilités claires
- Logging structuré

### ✅ Performance
- Aucun overhead supplémentaire
- Rate limiting maintenu
- Validation optimisée

---

**Architecture Layer RPC - Documentation Complète** 🏗️
