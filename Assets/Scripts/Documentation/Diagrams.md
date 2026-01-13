# 📊 System Diagrams

## Architecture Overview

```mermaid
flowchart TB
    subgraph UI["UI Layer"]
        SP[SessionPresenter]
        PUI[PseudUI]
    end

    subgraph Service["Service Layer"]
        ISS[ISessionService]
        SSC[SessionServiceClient]
        SSS[SessionServiceServer]
        SL[ServiceLocator]
    end

    subgraph Games["Game Plugin System"]
        GR[GameRegistry]
        GIM[GameInstanceManager]
        IGD[IGameDefinition]
        SQG[SquareGameDefinition]
        CIG[CircleGameDefinition]
    end

    subgraph Network["Network Layer"]
        SRPC[SessionRpcHub]
        NB[NetworkBootstrap]
        NM[NetworkManager]
    end

    subgraph Data["Data Layer - Server"]
        GSM[GameSessionManager]
        PM[PlayerManager]
    end

    subgraph Pawns["Pawn System"]
        PP[PlayerPawn]
        CP[CirclePawn]
        DP[DefaultPlayer]
    end

    SP --> ISS
    ISS --> SSC
    ISS --> SSS
    SSC --> SRPC
    SSS --> GSM
    SSS --> GIM
    
    SRPC --> GSM
    SRPC --> GIM
    SRPC --> PM
    
    GIM --> GR
    GR --> IGD
    IGD --> SQG
    IGD --> CIG
    
    SQG --> PP
    CIG --> CP
    
    NB --> NM
    NM --> SRPC
    NM --> DP
```

---

## Feature List

| # | Feature | Description |
|---|---------|-------------|
| 1 | **Session Creation** | Create named lobby sessions |
| 2 | **Session Joining** | Join existing sessions |
| 3 | **Session Leaving** | Leave current session |
| 4 | **Ready Toggle** | Mark player as ready/not ready |
| 5 | **Game Type Selection** | Host selects which game to play |
| 6 | **Game Start** | Start game when all players ready |
| 7 | **Pawn Spawning** | Server spawns player pawns |
| 8 | **Player Movement** | Server-authoritative movement |
| 9 | **Multi-Session** | Multiple concurrent sessions |

---

## Feature 1: Session Creation

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager

    UI->>SVC: CreateSession("RoomName")
    SVC->>RPC: CreateSessionServerRpc("RoomName", clientId)
    
    rect rgb(200, 230, 255)
        Note over RPC,GSM: Server Processing
        RPC->>GSM: TryAddSession("RoomName")
        GSM-->>RPC: success/fail
    end
    
    RPC-->>SVC: SyncSessionsClientRpc(sessions[])
    SVC-->>UI: SessionsChanged event
    
    alt Success
        UI->>UI: Navigate to Lobby
    else Session exists
        RPC-->>SVC: SendErrorClientRpc("Session exists")
        SVC-->>UI: ErrorOccurred event
    end
```

---

## Feature 2: Session Joining

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager
    participant PM as PlayerManager

    UI->>SVC: JoinSession("RoomName")
    SVC->>RPC: JoinSessionServerRpc("RoomName", clientId)
    
    rect rgb(200, 230, 255)
        Note over RPC,PM: Server Processing
        RPC->>GSM: TryJoinSession("RoomName", clientId)
        RPC->>PM: GetPlayer(clientId)
        PM-->>RPC: PlayerData{name}
        GSM-->>RPC: success + sessionState
    end
    
    RPC-->>SVC: SyncSessionsClientRpc(sessions[])
    RPC-->>SVC: SendSessionDetailsClientRpc(sessionState)
    SVC-->>UI: CurrentSessionChanged event
    UI->>UI: Show Lobby Panel
```

---

## Feature 3: Session Leaving

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager

    UI->>SVC: LeaveSession()
    SVC->>RPC: LeaveSessionServerRpc(clientId)
    
    rect rgb(200, 230, 255)
        Note over RPC,GSM: Server Processing
        RPC->>GSM: RemovePlayerFromSession(clientId)
        GSM-->>RPC: updated sessions
    end
    
    RPC-->>SVC: SyncSessionsClientRpc(sessions[])
    SVC->>SVC: Clear currentSession
    SVC-->>UI: CurrentSessionChanged(null)
    UI->>UI: Show Session List
```

---

## Feature 4: Ready Toggle

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager

    UI->>SVC: SetReady(true)
    SVC->>RPC: SetReadyServerRpc(true, clientId)
    
    rect rgb(200, 230, 255)
        Note over RPC,GSM: Server Processing
        RPC->>GSM: SetPlayerReady("session", clientId, true)
        GSM-->>RPC: updated state
    end
    
    RPC-->>SVC: SendSessionDetailsClientRpc(sessionState)
    SVC-->>UI: CurrentSessionChanged event
    UI->>UI: Update Ready Status UI
```

---

## Feature 5: Game Type Selection

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager
    participant GR as GameRegistry

    UI->>SVC: SetGameType("circle-game")
    SVC->>RPC: SetGameTypeServerRpc("session", "circle-game")
    
    rect rgb(200, 230, 255)
        Note over RPC,GR: Server Validation
        RPC->>GR: GetGame("circle-game")
        GR-->>RPC: IGameDefinition (or null)
        
        alt Valid Game
            RPC->>GSM: SetGameType("session", "circle-game")
            GSM-->>RPC: success
        else Invalid Game
            RPC-->>SVC: SendErrorClientRpc("Unknown game")
        end
    end
    
    RPC-->>SVC: SendSessionDetailsClientRpc(updated)
    SVC-->>UI: CurrentSessionChanged event
```

---

## Feature 6: Game Start

```mermaid
sequenceDiagram
    autonumber
    participant UI as SessionPresenter
    participant SVC as SessionServiceClient
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager
    participant GIM as GameInstanceManager
    participant GR as GameRegistry

    UI->>SVC: StartGame()
    SVC->>RPC: StartGameServerRpc("session")
    
    rect rgb(200, 230, 255)
        Note over RPC,GR: Server: Validate & Setup
        RPC->>GSM: GetSession("session")
        GSM-->>RPC: SessionState{players, gameId}
        RPC->>GSM: AllPlayersReady?
        GSM-->>RPC: true
        RPC->>GR: GetGame(gameId)
        GR-->>RPC: IGameDefinition
    end
    
    rect rgb(200, 255, 200)
        Note over RPC,GIM: Server: Create Game Instance
        RPC->>GIM: CreateGame(sessionId, gameDef, players, offset)
        GIM->>GIM: gameDef.SetupGame(offset)
        GIM->>GIM: Spawn pawns for each player
        GIM-->>RPC: success
    end
    
    RPC-->>SVC: StartGameClientRpc(sessionName, gameId)
    SVC->>SVC: gameDef.SetupClientVisuals()
    SVC-->>UI: GameStarting event
    UI->>UI: Hide Lobby, Show Game
```

---

## Feature 7: Pawn Spawning (Server)

```mermaid
sequenceDiagram
    autonumber
    participant GIM as GameInstanceManager
    participant GD as IGameDefinition
    participant NM as NetworkManager
    participant Pawn as PlayerPawn/CirclePawn

    GIM->>GIM: CreateGame(session, gameDef, players, offset)
    
    loop For each player
        GIM->>GD: GetSpawnPosition(index, total, offset)
        GD-->>GIM: Vector3 position
        
        GIM->>NM: Instantiate(gameDef.PawnPrefab)
        NM-->>GIM: NetworkObject
        
        GIM->>GD: InitializePawn(pawn, playerName, index, offset)
        GD->>Pawn: Initialize(name, colorIndex, offset)
        
        GIM->>NM: pawn.SpawnWithOwnership(clientId)
        Note over NM,Pawn: Pawn replicated to all clients
    end
    
    GIM->>GIM: Store GameInstance{pawns, playerIds}
```

---

## Feature 8: Player Movement

```mermaid
sequenceDiagram
    autonumber
    participant Input as Player Input
    participant RPC as SessionRpcHub
    participant GIM as GameInstanceManager
    participant GD as IGameDefinition
    participant Pawn as Pawn Component

    Input->>RPC: RequestMoveServerRpc(session, direction)
    
    rect rgb(200, 230, 255)
        Note over RPC,Pawn: Server-Authoritative Movement
        RPC->>GIM: HandleMovement(session, clientId, direction)
        GIM->>GIM: Find GameInstance for session
        GIM->>GIM: Find pawn for clientId
        GIM->>GD: HandleMovement(pawn, direction)
        GD->>Pawn: Move(direction)
        Pawn->>Pawn: Update NetworkVariable position
    end
    
    Note over Pawn,Input: Position auto-synced via NetworkVariable
```

---

## Session State Diagram

```mermaid
stateDiagram-v2
    [*] --> Disconnected
    
    Disconnected --> Connected: Connect to Server
    
    Connected --> InSessionList: Connection Success
    
    state InSessionList {
        [*] --> Browsing
        Browsing --> Creating: Create Session
        Creating --> Browsing: Cancel/Fail
    }
    
    InSessionList --> InLobby: Join/Create Success
    
    state InLobby {
        [*] --> NotReady
        NotReady --> Ready: Set Ready
        Ready --> NotReady: Unset Ready
        
        state HostActions {
            [*] --> SelectingGame
            SelectingGame --> GameSelected: Choose Game Type
        }
    }
    
    InLobby --> InSessionList: Leave Session
    InLobby --> InGame: All Ready + Start
    
    state InGame {
        [*] --> Playing
        Playing --> GameOver: Game Ends
    }
    
    InGame --> InLobby: Return to Lobby
    InGame --> InSessionList: Leave Session
    
    Connected --> Disconnected: Disconnect
    InSessionList --> Disconnected: Disconnect
    InLobby --> Disconnected: Disconnect
    InGame --> Disconnected: Disconnect
```

---

## Game Plugin State Diagram

```mermaid
stateDiagram-v2
    [*] --> Unregistered
    
    Unregistered --> Registered: GameRegistry.Initialize()
    Note right of Registered: Loads from Resources/Games/
    
    Registered --> Selected: Host selects game type
    
    Selected --> Preparing: StartGame called
    
    state Preparing {
        [*] --> SetupGame
        SetupGame --> SpawningPawns: Create arena
        SpawningPawns --> InitializingPawns: Spawn prefabs
        InitializingPawns --> [*]: Set names/colors
    }
    
    Preparing --> Active: All pawns spawned
    
    state Active {
        [*] --> Running
        Running --> Running: Handle movements
        Running --> Paused: Pause
        Paused --> Running: Resume
    }
    
    Active --> Cleanup: Game ends
    
    Cleanup --> Registered: Destroy pawns/arena
```

---

## Network Object Lifecycle

```mermaid
stateDiagram-v2
    direction LR
    
    [*] --> Prefab: In NetworkPrefabList
    
    Prefab --> Instantiated: Server Instantiate()
    
    Instantiated --> Spawned: SpawnWithOwnership(clientId)
    
    state Spawned {
        [*] --> ServerOnly
        ServerOnly --> Replicated: NetworkManager syncs
        Replicated --> Replicated: NetworkVariables sync
    }
    
    Spawned --> Despawned: Despawn()
    
    Despawned --> Destroyed: Destroy()
    
    Destroyed --> [*]
```

---

## File Dependencies

```mermaid
flowchart LR
    subgraph Core["Core/"]
        IGD[IGameDefinition]
        GR[GameRegistry]
        GIM[GameInstanceManager]
        IPF[IPlayerFeature]
    end
    
    subgraph Games["Games/"]
        SQD[SquareGameDefinition]
        CID[CircleGameDefinition]
        CPawn[CirclePawn]
    end
    
    subgraph Networking["Networking/"]
        SRPC[SessionRpcHub]
        GSM[GameSessionManager]
        PM[PlayerManager]
        DP[DefaultPlayer]
        NB[NetworkBootstrap]
    end
    
    subgraph Service["Service/"]
        ISS[ISessionService]
        SSC[SessionServiceClient]
        SSS[SessionServiceServer]
    end
    
    subgraph Game["Game/"]
        PP[PlayerPawn]
        GSS[GameSceneSetup]
    end
    
    %% Dependencies
    GIM --> GR
    GIM --> IGD
    SQD --> IGD
    CID --> IGD
    CID --> CPawn
    SQD --> PP
    
    SRPC --> GSM
    SRPC --> PM
    SRPC --> GIM
    SRPC --> GR
    
    SSC --> SRPC
    SSS --> GSM
    
    DP --> PM
```

---

## Deleted Files (Cleanup)

| File | Reason Deleted |
|------|----------------|
| `Networking/Player/GameManager.cs` | Replaced by GameInstanceManager |
| `Networking/Player/GameRuntime.cs` | Legacy Game/GameStruct/GamePlayerState classes removed |
| `Data/Game/GameRuntimeModel.cs` | Referenced deleted `ISessionRuntimeModel` |
| `Data/Game/GameServerConfig.cs` | Never used |
| `Data/Player/PlayerRuntimeModel.cs` | Wrapper not used |
| `Data/Player/PlayerDataModel.cs` | Adapter not used |
| `Service/GameServer/IGameServer.cs` | Interface never implemented |
| `Service/GameServer/GameServerService.cs` | Never registered |
| `Service/PawnSpawner/IPawnSpawner.cs` | Interface never used |
| `Service/PawnSpawner/NetworkPawnSpawner.cs` | Never instantiated |
| `Service/PlayerRegistry/IPlayerRegistry.cs` | Interface never used |
| `Service/PlayerRegistry/PlayerRegistryService.cs` | Never registered |
| `Service/Core/IGameService.cs` | Interface never implemented |
| `Networking/Sessions/data/` | Empty folder |

---

**Last Updated:** 2026-01-07
