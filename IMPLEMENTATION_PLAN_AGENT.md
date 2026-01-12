# Plan d'Implémentation - Alignement avec l'Agent NGO Dedicated Server

## Vue d'ensemble

Ce plan aligne le projet Unity 2D avec l'architecture définie dans l'agent `cursor-ngo-dedicated-server.md` pour créer un système client/serveur autoritaire avec builds séparés.

## État actuel vs Architecture cible

### ✅ Composants existants

- `SessionRpcHub` - RPC hub (existe, à refactoriser)
- `SessionContainer` - FSM avec états (existe)
- `SessionContainerManager` - Gestionnaire multi-sessions (existe)
- `GameSessionManager` - Gestion des sessions (existe)
- `GameInstanceManager` - Gestion des instances de jeu (existe)
- `PlayerInputHandler` - Handler d'inputs client (existe)
- `NetworkBootstrap` - Bootstrap réseau (existe, à séparer)

### ❌ Composants manquants

- `ConnectionController` - Gestion des connexions client-to-session
- `ServerBootstrap` - Bootstrap serveur dédié
- `ClientBootstrap` - Bootstrap client séparé
- Assembly Definitions pour séparation Client/Server/Shared
- DTOs/Structs partagés pour messages réseau compacts
- Séparation claire via `#if UNITY_SERVER`

## Tâches détaillées

### Phase 1 : Structure modulaire et séparation Client/Server

#### 1.1 Créer Assembly Definitions

**Fichiers à créer :**

- `Assets/Scripts/Networking/Client/Client.asmdef`
- `Assets/Scripts/Networking/Server/Server.asmdef`
- `Assets/Scripts/Networking/Shared.asmdef` (pour RpcHandlers, Sessions, Connections, Player)

**Configuration :**

```json
// Client.asmdef
{
  "name": "Networking.Client",
  "references": ["Unity.Netcode.Runtime", "Networking.Shared"],
  "includePlatforms": [],
  "excludePlatforms": ["Server"]
}

// Server.asmdef
{
  "name": "Networking.Server",
  "references": ["Unity.Netcode.Runtime", "Networking.Shared"],
  "includePlatforms": ["Server"],
  "excludePlatforms": []
}

// Shared.asmdef
{
  "name": "Networking.Shared",
  "references": ["Unity.Netcode.Runtime"],
  "includePlatforms": [],
  "excludePlatforms": []
}
```

#### 1.2 Créer ConnectionController

**Fichier :** `Assets/Scripts/Networking/Server/ConnectionController.cs`

**Responsabilités :**

- Gérer les connexions client
- Mapper `clientId → sessionName`
- Valider les connexions avant assignation à une session
- Gérer les déconnexions et nettoyage

**Interface :**

```csharp
public class ConnectionController : MonoBehaviour
{
    public bool TryAssignToSession(ulong clientId, string sessionName);
    public string GetClientSession(ulong clientId);
    public void HandleClientDisconnect(ulong clientId);
    public bool IsClientConnected(ulong clientId);
}
```

#### 1.3 Séparer NetworkBootstrap en ServerBootstrap et ClientBootstrap

**ServerBootstrap.cs** (`Assets/Scripts/Networking/Server/ServerBootstrap.cs`)

- Initialisation serveur headless (`-batchmode -nographics`)
- Parsing arguments ligne de commande (`-port`, `-maxplayers`)
- Spawn de `SessionRpcHub`
- Création de `GameSessionManager`
- Initialisation de `ConnectionController`

**ClientBootstrap.cs** (`Assets/Scripts/Networking/Client/ClientBootstrap.cs`)

- Initialisation client
- Connexion via `UnityTransport.SetConnectionData(ip, port)`
- Gestion UI de connexion
- Chargement de la scène Menu après connexion

#### 1.4 Ajouter directives `#if UNITY_SERVER`

**Fichiers à modifier :**

- `SessionRpcHub.cs` - Séparer logique serveur/client
- `GameSessionManager.cs` - Isoler code serveur
- `GameInstanceManager.cs` - Isoler code serveur
- Tous les fichiers avec logique serveur critique

**Pattern :**

```csharp
#if UNITY_SERVER
    // Code serveur uniquement
    private void ValidateMovement(ulong clientId, Vector3 position) { }
#endif

#if !UNITY_SERVER
    // Code client uniquement
    private void UpdateClientUI() { }
#endif
```

### Phase 2 : Architecture réseau et DTOs

#### 2.1 Créer DTOs partagés

**Dossier :** `Assets/Scripts/Networking/Shared/Data/`

**Fichiers à créer :**

- `GameStateSnapshot.cs` - Snapshot complet de l'état du jeu
- `SnapshotEntity.cs` - Entité dans le snapshot (position, état)
- `PlayerInputDTO.cs` - DTO pour inputs joueur
- `SessionUpdateDTO.cs` - DTO pour mises à jour de session

**Structure exemple :**

```csharp
[Serializable]
public struct GameStateSnapshot
{
    public float timestamp;
    public SnapshotEntity[] entities;
    public int sessionId;
}

[Serializable]
public struct SnapshotEntity
{
    public ulong networkId;
    public Vector3 position;
    public Quaternion rotation;
    public byte state;
}
```

#### 2.2 Refactoriser SessionRpcHub

**Objectif :** Orchestrer les flows session/game selon l'agent

**Méthodes à organiser :**

- ServerRPCs : `CreateSession`, `JoinSession`, `StartGame`, `RequestMove`
- ClientRPCs : `SyncSessions`, `StartGame`, `UpdateGameState`
- Validation serveur pour chaque RPC

**Structure cible :**

```csharp
public class SessionRpcHub : NetworkBehaviour
{
    // Session Management RPCs
    [ServerRpc(RequireOwnership = false)]
    private void CreateSessionServerRpc(string sessionName, ServerRpcParams rpcParams) { }
    
    [ServerRpc(RequireOwnership = false)]
    private void JoinSessionServerRpc(string sessionName, ServerRpcParams rpcParams) { }
    
    // Game Flow RPCs
    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc(string sessionName, ServerRpcParams rpcParams) { }
    
    // Client notifications
    [ClientRpc]
    private void SyncSessionsClientRpc(GameSession[] sessions) { }
    
    [ClientRpc]
    private void StartGameClientRpc(string sessionName, string gameId, Vector3 worldOffset) { }
}
```

### Phase 3 : Validation serveur autoritaire

#### 3.1 Valider SessionContainer FSM

**Vérifier les transitions :**

- `CreateSession → Lobby` ✅
- `StartGame() → Starting → InGame` ✅
- `EndGame()/InitFailed → Ended → Dispose()` ✅
- `AddPlayer/RemovePlayer` pendant Lobby ✅
- `HandleMovement()` pendant InGame ✅

**Fichier :** `Assets/Scripts/Core/Games/SessionContainer.cs`

#### 3.2 Assurer validation serveur

**Fichiers à vérifier/modifier :**

- `PlayerInputHandler.cs` - S'assurer qu'il envoie uniquement des intentions
- Tous les handlers de mouvement - Validation serveur (cooldowns, bounds, collisions)
- `GameInstanceManager.cs` - Validation spawn/despawn serveur

**Pattern de validation :**

```csharp
[ServerRpc(RequireOwnership = false)]
private void RequestMoveServerRpc(Vector2 direction, ServerRpcParams rpcParams)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    
    // Validation serveur
    if (!ValidateMovement(clientId, direction))
        return;
    
    // Appliquer mouvement autoritaire
    ApplyMovement(clientId, direction);
}
```

#### 3.3 Valider PlayerInputHandler

**Fichier :** `Assets/Scripts/Game/PlayerInputHandler.cs`

**Vérifications :**

- ✅ N'envoie que des intentions (inputs bruts)
- ✅ Ne calcule jamais l'état final
- ✅ Le serveur calcule et synchronise l'état

### Phase 4 : Build pipeline dual

#### 4.1 Configurer Build Profiles Unity

**Client Build :**

- Scènes : Menu, Lobby, Game, Results
- Platform : Windows/Mac/Linux
- Include : Client assembly, Shared assembly
- Exclude : Server assembly

**Server Build :**

- Scènes : Server (headless)
- Platform : Windows/Mac/Linux (headless)
- Include : Server assembly, Shared assembly
- Exclude : Client assembly, UI assemblies
- Build options : `-batchmode -nographics`

#### 4.2 Créer scripts de build

**Fichiers :**

- `Build/scripts/build_client.sh` (Linux)
- `Build/scripts/build_server.sh` (Linux)
- `Build/scripts/build_client.bat` (Windows)
- `Build/scripts/build_server.bat` (Windows)

**Commande exemple :**

```bash
Unity -batchmode -quit -projectPath . -buildTarget Linux64 -executeMethod BuildPipeline.BuildClient
```

### Phase 5 : Tests et validation

#### 5.1 Checklist de validation réseau

**Tests locaux :**

- [ ] Serveur démarre en mode headless
- [ ] Client se connecte au serveur local
- [ ] Création de session fonctionne
- [ ] Join session fonctionne
- [ ] Start game fonctionne
- [ ] Mouvement validé serveur
- [ ] Synchronisation état correcte

**Tests multi-clients :**

- [ ] 2+ clients peuvent se connecter
- [ ] Sessions isolées fonctionnent
- [ ] Pas de fuite de données entre sessions
- [ ] Déconnexion gérée correctement

**Tests serveur dédié :**

- [ ] Serveur démarre sans UI
- [ ] Arguments ligne de commande parsés
- [ ] Logs serveur fonctionnent
- [ ] Performance acceptable (50+ sessions)

#### 5.2 Tests de build

- [ ] Build client génère exécutable fonctionnel
- [ ] Build serveur génère exécutable headless
- [ ] Pas de références croisées Client↔Server
- [ ] Assemblies correctement séparées

### Phase 6 : Documentation

#### 6.1 Documenter architecture alignée

**Fichier :** `Assets/Scripts/Documentation/Architecture_Agent_Alignment.md`

**Contenu :**

- Diagramme de l'architecture cible
- Mapping composants existants → architecture agent
- Guide de séparation Client/Server
- Guide de build dual

#### 6.2 Mettre à jour diagrammes

**Fichiers :**

- `Assets/Scripts/Documentation/Diagrams/class.mmd`
- `Assets/Scripts/Documentation/Diagrams/sequence.mmd`

## Ordre d'exécution recommandé

1. **Phase 1.1** - Assembly Definitions (base pour séparation)
2. **Phase 1.2** - ConnectionController (nécessaire pour mapping)
3. **Phase 1.3** - Séparation Bootstrap (structure de base)
4. **Phase 1.4** - Directives UNITY_SERVER (séparation code)
5. **Phase 2** - DTOs et refactoring RPC (architecture réseau)
6. **Phase 3** - Validation serveur (sécurité/autorité)
7. **Phase 4** - Build pipeline (livrables)
8. **Phase 5** - Tests (validation)
9. **Phase 6** - Documentation (maintenance)

## Critères de succès

- ✅ Builds séparés fonctionnels (client + serveur)
- ✅ Code serveur isolé via assemblies + directives
- ✅ Validation serveur pour toute logique critique
- ✅ Client envoie uniquement des intentions
- ✅ Architecture alignée avec l'agent
- ✅ Tests passent (local + multi-clients)
- ✅ Documentation à jour

## Notes importantes

- **Pas de services externes** : Tout doit être local/déterministe
- **2D uniquement** : Pas de logique 3D
- **NGO uniquement** : Pas d'autres systèmes réseau
- **Multi-scene client** : Lobby, Game, Results séparés
- **Serveur headless** : Pas de rendu, batchmode uniquement
