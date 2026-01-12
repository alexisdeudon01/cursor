# Session System - Diagramme de Composants

Ce diagramme montre l'architecture des composants et leurs interactions.

```mermaid
---
title: Session System - Component Diagram
---
flowchart TB
    subgraph Client["🖥️ CLIENT"]
        direction TB
        UI["SessionLobbyUI<br/>Interface utilisateur"]
        PawnVis["SessionPawnVisibility<br/>Filtrage visuel"]
        LocalPawn["PlayerPawn<br/>Représentation locale"]
        
        UI --> PawnVis
        PawnVis --> LocalPawn
    end
    
    subgraph Network["🌐 NETWORK LAYER"]
        direction TB
        RpcHub["SessionRpcHub<br/>Communication RPC"]
        NetManager["NetworkManager<br/>Netcode for GameObjects"]
        
        RpcHub <--> NetManager
    end
    
    subgraph Server["🖧 DEDICATED SERVER"]
        direction TB
        
        subgraph SessionManagement["Session Management"]
            GSM["GameSessionManager<br/>Gestion des sessions"]
            SCM["SessionContainerManager<br/>Isolation thread-safe"]
        end
        
        subgraph Containers["Isolated Containers"]
            SC1["SessionContainer 1<br/>Session: Partie_A<br/>Offset: X=0"]
            SC2["SessionContainer 2<br/>Session: Partie_B<br/>Offset: X=50"]
            SC3["SessionContainer N<br/>Session: Partie_N<br/>Offset: X=N*50"]
        end
        
        subgraph GameLogic["Game Logic"]
            GIM["GameInstanceManager<br/>Instances de jeu"]
            GR["GameRegistry<br/>Définitions de jeux"]
        end
        
        GSM --> SCM
        SCM --> SC1
        SCM --> SC2
        SCM --> SC3
        GSM --> GIM
        GIM --> GR
    end
    
    Client <-->|"RPCs"| Network
    Network <-->|"ServerRpc/ClientRpc"| Server
    
    style Client fill:#e1f5fe
    style Server fill:#fff3e0
    style Network fill:#f3e5f5
    style Containers fill:#e8f5e9
```

## Architecture des Composants

### 🖥️ Client Side

| Composant | Rôle |
|-----------|------|
| **SessionLobbyUI** | Interface utilisateur pour créer/rejoindre des sessions |
| **SessionPawnVisibility** | Filtre les pawns visibles selon la session locale |
| **PlayerPawn** | Représentation locale du joueur |

### 🌐 Network Layer

| Composant | Rôle |
|-----------|------|
| **SessionRpcHub** | Centralise tous les appels RPC session |
| **NetworkManager** | Gestionnaire Netcode for GameObjects |

### 🖧 Dedicated Server

| Composant | Rôle |
|-----------|------|
| **GameSessionManager** | Orchestration principale des sessions |
| **SessionContainerManager** | Gestion thread-safe des conteneurs |
| **SessionContainer 1..N** | Conteneurs isolés avec offset spatial |
| **GameInstanceManager** | Gestion des instances de jeu |
| **GameRegistry** | Définitions et règles des jeux |

## Isolation des Sessions

Chaque `SessionContainer` est isolé avec :
- **Autorisation** : Seuls les clients autorisés peuvent accéder
- **Offset spatial** : Position X = `N * 50` unités
- **Données séparées** : Players, pawns, état indépendants
- **Thread-safety** : ConcurrentDictionary et locks
