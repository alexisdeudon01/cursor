# Architecture du Projet

## Diagramme de Classes

```mermaid
classDiagram
    direction TB

    namespace Bootstrap {
        class ServerBootstrap {
            +Instance$ ServerBootstrap
            -ushort serverPort
            -int maxPlayers
            +Awake()
            +Start()
            -ParseCommandLine()
            -InitializeLogging()
            -StartServerDelayed()
            -SpawnSessionRpcHub()
            +Log(string)
            +LogGame(string, string, int)$
        }

        class ClientBootstrap {
            +Instance$ ClientBootstrap
            -string serverIP
            -ushort serverPort
            +Connect(string, ushort)
            +Disconnect()
        }

        class NetworkBootstrap {
            -NetworkManager networkManager
            -bool networkStarted
            +Initialize()
            +StartServer()
            +StartClient()
            -AutoStartBySceneName()
            -ApplyTransportConfig()
        }
    }

    namespace Session {
        class GameSessionManager {
            +Instance$ GameSessionManager
            -Dictionary sessions
            +TryAddSession(ulong, string) bool
            +TryJoinSession(ulong, string) bool
            +SetReady(ulong, string, bool)
            +GetPlayers(string) List
            +GetSessionsSnapshot() GameSession[]
        }

        class SessionRpcHub {
            +Instance$ SessionRpcHub
            +SessionsUpdated$ Action
            +SessionDetailsUpdated$ Action
            +CreateSessionServerRpc(string)
            +JoinSessionServerRpc(string)
            +SetReadyServerRpc(string, bool)
            +StartGameServerRpc(string)
            +RequestSessionsServerRpc()
        }

        class GameSession {
            <<struct>>
            +FixedString64Bytes name
            +ulong creator
            +int playerCount
            +int readyCount
        }

        class SessionDetails {
            <<struct>>
            +GameSession session
            +List players
        }
    }

    namespace UI {
        class MenuButtons {
            -UIDocument uiDocument
            -TextField ipField
            -TextField portField
            +OnConnect()
            +OnDisconnect()
        }

        class SessionLobbyUI {
            -VisualElement sessionsList
            -VisualElement playersList
            +OnCreateSession()
            +OnJoinSession(string)
            +OnReadyChanged(bool)
            +OnStartGame()
            +OnLeaveSession()
            -UpdateSessionsList(GameSession[])
        }
    }

    namespace Games {
        class GameRegistry {
            <<static>>
            -Dictionary games$
            +Register(IGameDefinition)$ bool
            +GetGame(string)$ IGameDefinition
            +AllGames$ IReadOnlyCollection
        }

        class IGameDefinition {
            <<interface>>
            +GameId string
            +DisplayName string
            +SetupClientVisuals()
            +CreateServerInstance()
        }

        class GameInstanceManager {
            +Instance$ GameInstanceManager
            -Dictionary activeGames
            +CreateGame(string, string, List) bool
            +HasActiveGame(string) bool
            +HandleMovement(string, ulong, Vector2)
        }
    }

    namespace Network {
        class NetworkManager {
            <<Unity.Netcode>>
            +Singleton$ NetworkManager
            +IsServer bool
            +IsClient bool
            +StartServer() bool
            +StartClient() bool
            +Shutdown()
        }

        class UnityTransport {
            <<Unity.Netcode>>
            +ConnectionData
            +SetConnectionData(string, ushort)
        }
    }

    %% Relationships
    ServerBootstrap --> NetworkManager : uses
    ServerBootstrap --> GameSessionManager : creates
    ServerBootstrap --> SessionRpcHub : spawns

    ClientBootstrap --> NetworkManager : uses
    ClientBootstrap --> UnityTransport : configures

    NetworkBootstrap --> NetworkManager : initializes
    NetworkBootstrap --> UnityTransport : configures

    MenuButtons --> ClientBootstrap : calls Connect
    SessionLobbyUI --> SessionRpcHub : calls RPCs
    SessionLobbyUI ..> GameSession : displays

    GameSessionManager --> SessionRpcHub : broadcasts via
    SessionRpcHub --> GameSessionManager : queries

    GameInstanceManager --> GameRegistry : gets games from
    GameRegistry o-- IGameDefinition : contains

    SessionRpcHub ..> GameSession : serializes
    SessionRpcHub ..> SessionDetails : serializes
```

## Flux de Données

### Démarrage Serveur
```
ServerBootstrap.Awake()
    └── ParseCommandLine() → serverPort, maxPlayers
    └── InitializeLogging()

ServerBootstrap.Start()
    └── StartServerDelayed()
        └── NetworkManager.StartServer()
        └── SpawnSessionRpcHub()
        └── EnsureGameSessionManager()
```

### Connexion Client
```
MenuButtons.OnConnect()
    └── ClientBootstrap.Connect(ip, port)
        └── UnityTransport.SetConnectionData()
        └── NetworkManager.StartClient()

→ Scene change: Menu → Client

SessionLobbyUI
    └── SessionRpcHub.RequestSessionsServerRpc()
    └── OnSessionsUpdated(sessions)
```

### Création de Session
```
SessionLobbyUI.OnCreateSession()
    └── SessionRpcHub.CreateSessionServerRpc(name)
        └── [Server] GameSessionManager.TryAddSession()
        └── [Server] SessionRpcHub.SyncSessionsClientRpc()
    └── [Client] OnSessionsUpdated()
```

### Démarrage de Partie
```
SessionLobbyUI.OnStartGame()
    └── SessionRpcHub.StartGameServerRpc(sessionName)
        └── [Server] GameSessionManager.GetPlayers()
        └── [Server] GameRegistry.GetGame(gameId)
        └── [Server] GameInstanceManager.CreateGame()
        └── [Server] SessionRpcHub.StartGameClientRpc()
    └── [Client] IGameDefinition.SetupClientVisuals()
```

## Scènes

| Scène | Bootstrap | UI | Description |
|-------|-----------|-----|-------------|
| Server | ServerBootstrap | - | Serveur dédié headless |
| Menu | ClientBootstrap | MenuButtons | Écran de connexion |
| Client | - | SessionLobbyUI | Lobby des sessions |
| Game | - | GameUI | Jeu en cours |
