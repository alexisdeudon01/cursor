# Architecture Cursor - Documentation Technique

## Vue d'Ensemble

Le projet Cursor implémente une architecture client-serveur complète pour Unity, avec un système d'autorisation robuste et un traitement orienté données.

## Diagramme d'Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CURSOR SYSTEM                            │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐              ┌──────────────────────────┐
│      CLIENT BUILD        │              │      SERVER BUILD         │
│   (CursorClient.exe)     │◄────────────►│   (CursorServer.exe)     │
└──────────────────────────┘              └──────────────────────────┘
          │                                          │
          │                                          │
          ▼                                          ▼

┌──────────────────────────┐              ┌──────────────────────────┐
│    CLIENT COMPONENTS     │              │    SERVER COMPONENTS      │
├──────────────────────────┤              ├──────────────────────────┤
│                          │              │                          │
│  ClientNetworkManager    │              │  ServerAuthManager       │
│  - ConnectToServer()     │              │  - AuthorizeClient()     │
│  - SendData()            │              │  - ValidateToken()       │
│  - RequestData()         │              │  - RevokeAuthorization() │
│                          │              │                          │
│  SceneController         │              │  AuthorizedDataManager   │
│  - LoadMainMenu()        │              │  - StoreData()           │
│  - LoadGameplay()        │              │  - RetrieveData()        │
│  - LoadSettings()        │              │  - DeleteData()          │
│                          │              │  - QueueDataOperation()  │
│  ClientInitializer       │              │                          │
│  - Initialize client     │              │  ServerConfig            │
│  - Setup connections     │              │  - Port: 7777            │
│                          │              │  - MaxConnections: 100   │
└──────────────────────────┘              │  - Full Authorization    │
                                          │                          │
                                          │  ServerInitializer       │
                                          │  - Initialize server     │
                                          │  - Setup auth system     │
                                          └──────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      SHARED COMPONENTS                           │
├─────────────────────────────────────────────────────────────────┤
│  NetworkProtocol - Shared protocol definitions                   │
│  CursorUtilities - Common utilities                             │
└─────────────────────────────────────────────────────────────────┘
```

## Flux de Communication

### 1. Connexion et Autorisation

```
CLIENT                          NETWORK                         SERVER
  │                                │                              │
  │  ConnectToServer()             │                              │
  ├───────────────────────────────►│                              │
  │                                │  Connection Request          │
  │                                ├─────────────────────────────►│
  │                                │                              │
  │                                │  Authentication Required     │
  │                                │◄─────────────────────────────┤
  │                                │                              │
  │  Send Credentials              │                              │
  ├───────────────────────────────►│  Authorization Request       │
  │                                ├─────────────────────────────►│
  │                                │                              │
  │                                │  AuthorizeClient()           │
  │                                │  Generate Token              │
  │                                │                              │
  │                                │  Token + Authorization OK    │
  │  OnAuthorized()                │◄─────────────────────────────┤
  │◄───────────────────────────────┤                              │
  │                                │                              │
  │  ✓ AUTHORIZED                  │                              │
  │                                │                              │
```

### 2. Échange de Données (Autorisé)

```
CLIENT                          NETWORK                         SERVER
  │                                │                              │
  │  SendData(key, value)          │                              │
  ├───────────────────────────────►│  Data + Token                │
  │                                ├─────────────────────────────►│
  │                                │                              │
  │                                │  ValidateToken()             │
  │                                │  IsAuthorized() = true       │
  │                                │                              │
  │                                │  StoreData()                 │
  │                                │  [Data stored successfully]  │
  │                                │                              │
  │                                │  Acknowledgment              │
  │  Data sent successfully        │◄─────────────────────────────┤
  │◄───────────────────────────────┤                              │
  │                                │                              │
```

### 3. Récupération de Données (Autorisé)

```
CLIENT                          NETWORK                         SERVER
  │                                │                              │
  │  RequestData(key)              │                              │
  ├───────────────────────────────►│  Request + Token             │
  │                                ├─────────────────────────────►│
  │                                │                              │
  │                                │  ValidateToken()             │
  │                                │  IsAuthorized() = true       │
  │                                │                              │
  │                                │  RetrieveData(key)           │
  │                                │  [Data retrieved]            │
  │                                │                              │
  │                                │  Data Response               │
  │  Data received                 │◄─────────────────────────────┤
  │◄───────────────────────────────┤                              │
  │                                │                              │
```

## Composants Détaillés

### Client

#### ClientNetworkManager
- **Responsabilité**: Gestion de la connexion et communication avec le serveur
- **Pattern**: Singleton (DontDestroyOnLoad)
- **Propriétés Clés**:
  - `IsConnected`: État de connexion
  - `IsAuthorized`: État d'autorisation
  - `authToken`: Token d'autorisation
- **Méthodes Principales**:
  - `ConnectToServer()`: Établit la connexion
  - `SendData()`: Envoie des données au serveur
  - `RequestData()`: Demande des données
  - `DisconnectFromServer()`: Déconnexion propre
- **Événements**:
  - `OnAuthorized`: Déclenché après autorisation réussie
  - `OnAuthorizationFailed`: Déclenché en cas d'échec
  - `OnDisconnected`: Déclenché à la déconnexion

#### SceneController
- **Responsabilité**: Gestion des transitions de scènes
- **Pattern**: Singleton (DontDestroyOnLoad)
- **Scènes Gérées**:
  - MainMenu: Menu principal
  - Gameplay: Jeu principal
  - Settings: Paramètres
- **Méthodes**:
  - `LoadMainMenu()`: Charge le menu
  - `LoadGameplay()`: Charge le jeu
  - `LoadSettings()`: Charge les paramètres
  - `LoadScene(string)`: Charge une scène par nom
  - `LoadSceneAsync(string)`: Charge de manière asynchrone

#### ClientInitializer
- **Responsabilité**: Initialisation du client au démarrage
- **Actions**:
  - Initialise ClientNetworkManager
  - Configure les événements
  - Prépare le client pour la connexion
- **Compilation Conditionnelle**: Active uniquement pour builds client

### Serveur

#### ServerAuthManager
- **Responsabilité**: Gestion de l'autorisation des clients
- **Pattern**: Singleton (DontDestroyOnLoad)
- **Fonctionnalités**:
  - Autorisation des clients
  - Génération de tokens sécurisés
  - Validation de tokens
  - Révocation d'autorisations
  - Suivi des clients autorisés
- **Structures de Données**:
  - `authorizedClients`: Dictionary<string, AuthorizedClient>
  - `authTokens`: Dictionary<string, string>
- **Méthodes Principales**:
  - `InitializeServer()`: Démarre le serveur
  - `AuthorizeClient()`: Autorise un nouveau client
  - `ValidateToken()`: Valide un token d'accès
  - `IsAuthorized()`: Vérifie le statut d'autorisation
  - `RevokeAuthorization()`: Révoque l'accès
  - `ShutdownServer()`: Arrêt propre du serveur

#### AuthorizedDataManager
- **Responsabilité**: Gestion des données orientées avec autorisation
- **Pattern**: Singleton (DontDestroyOnLoad)
- **Fonctionnalités**:
  - Stockage de données sécurisé
  - Récupération de données autorisée
  - Suppression de données
  - File d'attente d'opérations
  - Traitement orienté données
- **Structures de Données**:
  - `serverDataStore`: Dictionary<string, ServerData>
  - `pendingOperations`: Queue<DataOperation>
- **Méthodes Principales**:
  - `StoreData()`: Stocke des données (nécessite autorisation)
  - `RetrieveData()`: Récupère des données (nécessite autorisation)
  - `DeleteData()`: Supprime des données (nécessite autorisation)
  - `QueueDataOperation()`: Ajoute une opération à la file
  - `ProcessDataOperations()`: Traite les opérations en attente

#### ServerConfig (ScriptableObject)
- **Responsabilité**: Configuration du serveur
- **Paramètres**:
  - `Port`: Port d'écoute (7777)
  - `MaxConnections`: Connexions simultanées max (100)
  - `RequireFullAuthorization`: Autorisation obligatoire (true)
  - `TokenExpirationSeconds`: Durée de vie du token (3600)
  - `EnableDataOrientedProcessing`: Traitement orienté données (true)
  - `DataSyncIntervalMs`: Intervalle de synchronisation (100)
  - `MaxDataPacketSize`: Taille max des paquets (4096)

#### ServerInitializer
- **Responsabilité**: Initialisation du serveur au démarrage
- **Actions**:
  - Initialise ServerAuthManager
  - Initialise AuthorizedDataManager
  - Configure le serveur
  - Prépare le système d'autorisation
- **Compilation Conditionnelle**: Active uniquement pour builds serveur

### Shared (Partagé)

#### NetworkProtocol
- **Responsabilité**: Définitions du protocole réseau
- **Constantes**:
  - `PROTOCOL_VERSION`: Version du protocole
  - `DEFAULT_PORT`: Port par défaut
  - `MAX_PACKET_SIZE`: Taille max des paquets
- **Types de Messages**:
  - `MSG_CONNECT`: Connexion
  - `MSG_DISCONNECT`: Déconnexion
  - `MSG_AUTH_REQUEST`: Requête d'autorisation
  - `MSG_AUTH_RESPONSE`: Réponse d'autorisation
  - `MSG_DATA_STORE`: Stockage de données
  - `MSG_DATA_RETRIEVE`: Récupération de données
  - `MSG_DATA_DELETE`: Suppression de données
  - `MSG_HEARTBEAT`: Signal de vie

#### CursorUtilities
- **Responsabilité**: Utilitaires communs
- **Fonctions**:
  - `GetBuildTarget()`: Retourne la plateforme
  - `IsServerBuild()`: Vérifie si build serveur
  - `IsClientBuild()`: Vérifie si build client
  - `LogBuildInfo()`: Affiche les infos de build

## Système de Build

### BuildScript (Editor)
- **Responsabilité**: Automatisation des builds
- **Fonctionnalités**:
  - Build client avec 3 scènes
  - Build serveur avec 1 scène dédiée
  - Build dual (client + serveur)
  - Nettoyage des builds
- **Menu Unity**:
  - `Cursor > Build Client`: Build client uniquement
  - `Cursor > Build Server`: Build serveur uniquement
  - `Cursor > Build Both`: Build les deux
  - `Cursor > Clean Builds`: Nettoie les builds

### Symboles de Compilation
- **CLIENT_BUILD**: Défini pour builds client
- **SERVER_BUILD**: Défini pour builds serveur
- **UNITY_SERVER**: Défini par Unity pour builds serveur dédiés

### Configuration des Scènes

#### Client Scenes
1. **MainMenu.unity**: Menu principal
   - Point d'entrée client
   - Interface utilisateur
   - Options de connexion

2. **Gameplay.unity**: Jeu principal
   - Logique de jeu
   - Interaction avec serveur
   - Synchronisation des données

3. **Settings.unity**: Paramètres
   - Configuration client
   - Options réseau
   - Préférences utilisateur

#### Server Scene
1. **ServerScene.unity**: Serveur dédié
   - Pas d'interface utilisateur
   - Logique serveur uniquement
   - Gestion des connexions

## Assembly Definitions

### CursorClient.asmdef
- **Namespace**: CursorClient
- **Références**: CursorShared
- **Plateforme**: Toutes sauf serveur
- **Contenu**:
  - ClientNetworkManager
  - SceneController
  - ClientInitializer

### CursorServer.asmdef
- **Namespace**: CursorServer
- **Références**: CursorShared
- **Plateforme**: Serveur uniquement
- **Contenu**:
  - ServerAuthManager
  - AuthorizedDataManager
  - ServerConfig
  - ServerInitializer

### CursorShared.asmdef
- **Namespace**: CursorShared
- **Références**: Aucune
- **Plateforme**: Toutes
- **Contenu**:
  - NetworkProtocol
  - CursorUtilities
  - ExampleUsage

### CursorEditor.asmdef
- **Namespace**: CursorEditor
- **Références**: CursorShared
- **Plateforme**: Editor uniquement
- **Contenu**:
  - BuildScript

## Sécurité

### Mécanismes de Sécurité

1. **Autorisation Obligatoire**
   - Toutes les opérations nécessitent autorisation
   - Tokens générés de manière sécurisée
   - Validation à chaque requête

2. **Tokens Sécurisés**
   - Format: `GUID + Timestamp`
   - Unique par client
   - Expiration configurable

3. **Validation Multi-Niveaux**
   - Validation du token
   - Vérification du statut d'autorisation
   - Contrôle d'accès aux données

4. **Révocation**
   - Possibilité de révoquer à tout moment
   - Nettoyage automatique des tokens

## Performance

### Optimisations

1. **Assembly Definitions**
   - Compilation incrémentale
   - Temps de build réduits
   - Séparation claire du code

2. **Traitement Orienté Données**
   - File d'attente pour opérations asynchrones
   - Traitement par batch
   - Limite de 10 opérations par frame

3. **Singletons avec DontDestroyOnLoad**
   - Persistance entre scènes
   - Pas de réinitialisation
   - Performance optimale

4. **Intervalle de Synchronisation Configurable**
   - Par défaut: 100ms
   - Ajustable via ServerConfig
   - Balance entre réactivité et performance

## Extensibilité

Le système est conçu pour être facilement extensible:

1. **Nouveaux Types de Messages**
   - Ajoutez des constantes dans `NetworkProtocol`
   - Implémentez les handlers correspondants

2. **Nouvelles Scènes**
   - Ajoutez la scène dans Unity
   - Ajoutez une méthode dans `SceneController`
   - Configurez dans `EditorBuildSettings`

3. **Nouveaux Types de Données**
   - Ajoutez des méthodes dans `AuthorizedDataManager`
   - Utilisez la file d'attente pour opérations asynchrones

4. **Configuration Personnalisée**
   - Étendez `ServerConfig` avec nouveaux paramètres
   - Créez des ScriptableObjects personnalisés

## Dépannage

### Problèmes Courants

1. **Client ne peut pas se connecter**
   - Vérifiez que le serveur est démarré
   - Vérifiez l'adresse IP et le port
   - Vérifiez les pare-feu

2. **Autorisation échoue**
   - Vérifiez les logs serveur
   - Assurez-vous que le serveur est initialisé
   - Vérifiez la configuration d'autorisation

3. **Données non stockées**
   - Vérifiez l'autorisation du client
   - Vérifiez les logs pour erreurs
   - Assurez-vous que la clé est valide

4. **Build échoue**
   - Vérifiez que toutes les scènes existent
   - Vérifiez les assembly definitions
   - Nettoyez les builds existants

## Conclusion

Cette architecture fournit une base solide pour un système client-serveur Unity avec:
- ✅ Autorisation complète et sécurisée
- ✅ Traitement orienté données
- ✅ Séparation claire client/serveur
- ✅ Système de build automatisé
- ✅ Extensibilité et maintenabilité
- ✅ Performance optimisée
