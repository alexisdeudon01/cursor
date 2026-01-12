# Session Isolation - Diagramme de Classes

Ce diagramme montre l'architecture des classes du système d'isolation des sessions.

```mermaid
---
title: Session Isolation - Class Diagram
---
classDiagram
    direction TB
    
    namespace Core {
        class SessionContainer {
            -string sessionId
            -ulong hostClientId
            -SessionState state
            -HashSet~ulong~ authorizedClients
            -Dictionary~ulong, SessionPlayer~ players
            -Dictionary~ulong, NetworkObject~ pawns
            -Vector3 worldOffset
            -object lockObj
            +Initialize()
            +AuthorizeClient(ulong clientId) bool
            +RevokeClient(ulong clientId) bool
            +ValidateAccess(ulong clientId) bool
            +AddPlayer(ulong clientId, string name) bool
            +RemovePlayer(ulong clientId) bool
            +RegisterPawn(ulong clientId, NetworkObject pawn)
            +GetPlayerPawn(ulong clientId) NetworkObject
            +ValidatePositionInBounds(Vector3 pos) bool
            +Dispose()
        }
        
        class SessionContainerManager {
            -ConcurrentDictionary~string, SessionContainer~ containers
            +OnSecurityViolation Action~ulong, string~
            +CreateSession(string sessionId, ulong hostClientId) SessionContainer
            +GetSession(string sessionId) SessionContainer
            +GetAuthorizedSession(string sessionId, ulong clientId) SessionContainer
            +RemoveSession(string sessionId) bool
            +GetAllSessions() IEnumerable~SessionContainer~
        }
        
        class SessionPlayer {
            +ulong ClientId
            +string PlayerName
            +bool IsReady
            +NetworkObject Pawn
        }
        
        class SessionState {
            <<enumeration>>
            Lobby
            Starting
            InGame
            Ended
        }
    }
    
    namespace Networking {
        class GameSessionManager {
            -NetworkManager networkManager
            -SessionContainerManager containerManager
            -GameRegistry gameRegistry
            -Dictionary~string, GameInstance~ activeGames
            +CreateSession(string gameId, string sessionName) string
            +JoinSession(string sessionId, ulong clientId) bool
            +LeaveSession(string sessionId, ulong clientId)
            +StartGame(string sessionId)
            +ValidateClientAccess(string sessionId, ulong clientId) bool
            -OnSecurityViolation(ulong clientId, string sessionId)
        }
        
        class SessionRpcHub {
            +RequestCreateSessionServerRpc(string gameId, string sessionName)
            +RequestJoinSessionServerRpc(string sessionId)
            +RequestLeaveSessionServerRpc(string sessionId)
            +RequestStartGameServerRpc(string sessionId)
            +SessionCreatedClientRpc(string sessionId)
            +JoinSessionResultClientRpc(bool success)
            +PlayerJoinedClientRpc(string playerName)
            +GameStartedClientRpc()
        }
    }
    
    namespace Game {
        class GameInstanceManager {
            -Dictionary~string, GameInstance~ instances
            +CreateInstance(string sessionId, GameDefinition def)
            +StartInstance(string sessionId)
            +StopInstance(string sessionId)
            +SpawnPawnsForSession(SessionContainer container)
        }
        
        class SessionPawnVisibility {
            -string localSessionId
            -Dictionary~string, List~NetworkObject~~ sessionPawns
            +SetLocalSession(string sessionId)
            +RegisterPawn(NetworkObject pawn, string sessionId)
            +UnregisterPawn(NetworkObject pawn)
            +UpdateAllPawnVisibility()
        }
        
        class PlayerPawn {
            -string sessionId
            -ulong ownerClientId
            +SessionId string
            +OwnerClientId ulong
            +Initialize(string sessionId, ulong clientId)
            +OnNetworkSpawn()
            +OnNetworkDespawn()
        }
    }
    
    SessionContainerManager "1" *-- "*" SessionContainer : manages
    SessionContainer "1" *-- "*" SessionPlayer : contains
    SessionContainer --> SessionState : has state
    
    GameSessionManager "1" --> "1" SessionContainerManager : uses
    GameSessionManager "1" --> "1" GameInstanceManager : uses
    GameSessionManager "1" --> "*" SessionRpcHub : receives RPCs
    
    GameInstanceManager --> SessionContainer : spawns pawns for
    SessionPawnVisibility --> PlayerPawn : filters visibility
    PlayerPawn ..> SessionContainer : belongs to
```

## Description des Classes

### Namespace Core (Core.Games)

| Classe | Responsabilité |
|--------|----------------|
| **SessionContainer** | Conteneur isolé par session avec autorisation, gestion des joueurs et des pawns |
| **SessionContainerManager** | Gestionnaire thread-safe de tous les conteneurs de session |
| **SessionPlayer** | Données d'un joueur dans une session |
| **SessionState** | États possibles d'une session |

### Namespace Networking

| Classe | Responsabilité |
|--------|----------------|
| **GameSessionManager** | Orchestration des sessions de jeu côté serveur |
| **SessionRpcHub** | Hub RPC pour la communication client-serveur |

### Namespace Game

| Classe | Responsabilité |
|--------|----------------|
| **GameInstanceManager** | Gestion des instances de jeu |
| **SessionPawnVisibility** | Filtrage visuel des pawns par session (côté client) |
| **PlayerPawn** | Représentation réseau d'un joueur |
