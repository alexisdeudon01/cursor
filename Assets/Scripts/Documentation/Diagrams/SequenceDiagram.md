# Session Join - Diagramme de Séquence

Ce diagramme détaille les interactions lors de la création et jonction d'une session.

```mermaid
---
title: Session Join - Sequence Diagram
---
sequenceDiagram
    autonumber
    
    participant C as 🖥️ Client
    participant UI as SessionLobbyUI
    participant RPC as SessionRpcHub
    participant GSM as GameSessionManager
    participant SCM as SessionContainerManager
    participant SC as SessionContainer
    participant GIM as GameInstanceManager
    
    Note over C,GIM: Phase 1: Connexion et Création Session
    
    C->>UI: Clic "Créer Partie"
    UI->>RPC: RequestCreateSessionServerRpc(gameId, sessionName)
    RPC->>GSM: CreateSession(gameId, sessionName)
    GSM->>SCM: CreateSession(sessionId, hostClientId)
    SCM->>SC: new SessionContainer(sessionId)
    SC-->>SC: Initialize()
    SC->>SC: AuthorizeClient(hostClientId)
    SC-->>SCM: container
    SCM-->>GSM: sessionId
    GSM->>GIM: CreateInstance(gameId)
    GSM-->>RPC: success
    RPC-->>C: SessionCreatedClientRpc(sessionId)
    
    Note over C,GIM: Phase 2: Un autre client rejoint
    
    participant C2 as 🖥️ Client 2
    
    C2->>RPC: RequestJoinSessionServerRpc(sessionId)
    RPC->>GSM: JoinSession(sessionId, clientId)
    GSM->>SCM: GetSession(sessionId)
    SCM-->>GSM: container
    GSM->>SC: ValidateAccess(clientId)
    SC-->>GSM: false (pas encore autorisé)
    GSM->>SC: AuthorizeClient(clientId)
    SC->>SC: AddPlayer(clientId, playerName)
    SC-->>GSM: success
    GSM-->>RPC: joinSuccess
    RPC-->>C2: JoinSessionResultClientRpc(true)
    RPC-->>C: PlayerJoinedClientRpc(playerName)
    
    Note over C,GIM: Phase 3: Lancement du jeu
    
    C->>RPC: RequestStartGameServerRpc(sessionId)
    RPC->>GSM: StartGame(sessionId, clientId)
    GSM->>SCM: GetAuthorizedSession(sessionId, clientId)
    SCM->>SC: ValidateAccess(clientId)
    SC-->>SCM: true
    SCM-->>GSM: container
    GSM->>SC: IsHost(clientId)?
    SC-->>GSM: true
    GSM->>SC: SetState(SessionState.Starting)
    GSM->>GIM: StartGame(sessionId)
    GIM->>SC: GetPlayers()
    SC-->>GIM: playerList
    
    loop Pour chaque joueur
        GIM->>GIM: SpawnPawn(player, offset)
        GIM->>SC: RegisterPawn(clientId, pawn)
    end
    
    GSM->>SC: SetState(SessionState.InGame)
    RPC-->>C: GameStartedClientRpc()
    RPC-->>C2: GameStartedClientRpc()
    
    Note over C,GIM: Phase 4: Violation de sécurité (tentative)
    
    participant C3 as ⚠️ Attaquant
    
    C3->>RPC: RequestJoinSessionServerRpc(otherSessionId)
    RPC->>GSM: JoinSession(otherSessionId, attackerId)
    GSM->>SCM: GetAuthorizedSession(otherSessionId, attackerId)
    SCM->>SC: ValidateAccess(attackerId)
    SC-->>SCM: false
    SCM-->>SCM: OnSecurityViolation.Invoke()
    SCM-->>GSM: null
    GSM-->>RPC: accessDenied
    RPC-->>C3: JoinSessionResultClientRpc(false, "Not authorized")
```

## Description des Phases

### Phase 1: Création de Session

1. Client clique "Créer Partie" dans l'UI
2. `SessionRpcHub` transmet la demande au serveur
3. `GameSessionManager` orchestre la création
4. `SessionContainerManager` crée un nouveau conteneur
5. Le Host est automatiquement autorisé
6. Confirmation envoyée au client

### Phase 2: Jonction d'un Client

1. Client 2 demande à rejoindre via RPC
2. Le serveur valide que la session existe
3. Client est autorisé et ajouté aux joueurs
4. Tous les participants sont notifiés

### Phase 3: Lancement du Jeu

1. Host demande le démarrage
2. Validation que le demandeur est bien le host
3. État passe à `Starting` puis `InGame`
4. Pawns spawnés pour chaque joueur
5. Tous les clients reçoivent la notification

### Phase 4: Gestion des Violations

1. Un attaquant tente d'accéder à une session non autorisée
2. `ValidateAccess` retourne `false`
3. Événement `OnSecurityViolation` déclenché
4. Accès refusé avec message d'erreur

## Points Clés de Sécurité

| Mécanisme | Description |
|-----------|-------------|
| `ValidateAccess()` | Vérifie l'autorisation avant chaque opération |
| `GetAuthorizedSession()` | Retourne null si non autorisé |
| `OnSecurityViolation` | Événement pour logging/bannissement |
| Host-only operations | Certaines actions réservées au host |
