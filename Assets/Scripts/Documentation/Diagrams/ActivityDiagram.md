# Session Lifecycle - Diagramme d'Activité

Ce diagramme montre le flux complet du cycle de vie d'une session.

```mermaid
---
title: Session Lifecycle - Activity Diagram
---
flowchart TD
    Start([🎮 Début]) --> CheckHost{Host ou Client?}
    
    CheckHost -->|Host| CreateSession[Créer Session<br/>SessionContainerManager.CreateSession]
    CheckHost -->|Client| RequestJoin[Demander Rejoindre<br/>SessionRpcHub.RequestJoinSessionServerRpc]
    
    CreateSession --> InitContainer[Initialiser Container<br/>SessionContainer.Initialize]
    InitContainer --> AuthorizeHost[Autoriser Host<br/>AuthorizeClient]
    AuthorizeHost --> WaitPlayers{Attendre Joueurs}
    
    RequestJoin --> ValidateClient{Valider Client?}
    ValidateClient -->|Échec| RejectClient[Rejeter Client<br/>OnJoinFailed]
    ValidateClient -->|Succès| AuthorizePlayer[Autoriser Joueur<br/>AuthorizeClient]
    
    RejectClient --> End([❌ Fin Rejet])
    
    AuthorizePlayer --> AddToSession[Ajouter à Session<br/>AddPlayer]
    AddToSession --> NotifyAll[Notifier Tous<br/>PlayerJoinedClientRpc]
    NotifyAll --> WaitPlayers
    
    WaitPlayers -->|Min joueurs atteint| CheckStart{Host lance?}
    WaitPlayers -->|Timeout| Cleanup[Nettoyer Session]
    
    CheckStart -->|Non| WaitPlayers
    CheckStart -->|Oui| StartGame[Démarrer Jeu<br/>State = InGame]
    
    StartGame --> SpawnPawns[Spawn Pawns<br/>Position avec Offset]
    SpawnPawns --> GameLoop{Game Loop}
    
    GameLoop -->|Joueur quitte| RemovePlayer[Retirer Joueur<br/>RevokeClient + RemovePlayer]
    GameLoop -->|Fin partie| EndSession[Fin Session<br/>State = Ended]
    GameLoop -->|Continue| GameLoop
    
    RemovePlayer --> CheckPlayers{Joueurs restants?}
    CheckPlayers -->|Oui| GameLoop
    CheckPlayers -->|Non| EndSession
    
    EndSession --> Cleanup
    Cleanup --> DisposeContainer[Dispose Container<br/>SessionContainer.Dispose]
    DisposeContainer --> End2([✅ Fin Normale])
    
    style Start fill:#4caf50,color:#fff
    style End fill:#f44336,color:#fff
    style End2 fill:#4caf50,color:#fff
    style StartGame fill:#2196f3,color:#fff
    style AuthorizeHost fill:#ff9800,color:#fff
    style AuthorizePlayer fill:#ff9800,color:#fff
```

## Phases du Cycle de Vie

### 1️⃣ Phase de Création
- Le Host crée une session via `SessionContainerManager.CreateSession()`
- Un `SessionContainer` est initialisé avec un ID unique
- Le Host est automatiquement autorisé

### 2️⃣ Phase de Lobby
- Les clients demandent à rejoindre via RPC
- Validation et autorisation de chaque client
- Les joueurs sont ajoutés au conteneur
- Notification broadcast à tous les participants

### 3️⃣ Phase de Jeu
- Le Host lance la partie quand les conditions sont remplies
- Les pawns sont spawnés avec l'offset de session
- Game loop active avec validation continue

### 4️⃣ Phase de Fin
- Déclenchée par fin de partie ou déconnexion
- Révocation de tous les clients
- Nettoyage des ressources
- Dispose du conteneur

## Points de Sécurité

| Point | Validation |
|-------|------------|
| **Join** | `ValidateClient` vérifie les conditions d'entrée |
| **Autorisation** | `AuthorizeClient` ajoute aux clients autorisés |
| **Game Loop** | Validation continue des accès |
| **Cleanup** | `RevokeClient` retire les autorisations |
