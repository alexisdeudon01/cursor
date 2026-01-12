# Cursor - Guide de Référence Rapide

## 🚀 Démarrage Rapide

### 1. Générer les Builds
```
Unity Menu > Cursor > Build Both (Client + Server)
```

### 2. Lancer le Serveur
```bash
./Builds/Server/CursorServer.exe
```

### 3. Lancer le Client
```bash
./Builds/Client/CursorClient.exe
```

## 📚 API Principale

### Serveur - Autorisation
```csharp
// Autoriser un client
ServerAuthManager.Instance.AuthorizeClient(clientId, credentials);

// Vérifier si autorisé
ServerAuthManager.Instance.IsAuthorized(clientId);

// Révoquer
ServerAuthManager.Instance.RevokeAuthorization(clientId);
```

### Serveur - Gestion des Données
```csharp
// Stocker
AuthorizedDataManager.Instance.StoreData(clientId, "key", data);

// Récupérer
object data = AuthorizedDataManager.Instance.RetrieveData(clientId, "key");

// Supprimer
AuthorizedDataManager.Instance.DeleteData(clientId, "key");
```

### Client - Connexion
```csharp
// Se connecter au serveur
ClientNetworkManager.Instance.ConnectToServer();

// Vérifier le statut
bool connected = ClientNetworkManager.Instance.IsConnected;
bool authorized = ClientNetworkManager.Instance.IsAuthorized;

// Envoyer des données
ClientNetworkManager.Instance.SendData("key", data);
```

### Client - Scènes
```csharp
// Charger des scènes
SceneController.Instance.LoadMainMenu();
SceneController.Instance.LoadGameplay();
SceneController.Instance.LoadSettings();
SceneController.Instance.LoadScene("NomDeScene");
```

## 🔧 Configuration

### ServerConfig (ScriptableObject)
- `Port` : Port du serveur (défaut: 7777)
- `MaxConnections` : Connexions max (défaut: 100)
- `RequireFullAuthorization` : Autorisation requise
- `TokenExpirationSeconds` : Expiration token (défaut: 3600)
- `DataSyncIntervalMs` : Intervalle de sync (défaut: 100)

## 📁 Structure des Fichiers

```
cursor/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity (Client)
│   │   ├── Gameplay.unity (Client)
│   │   ├── Settings.unity (Client)
│   │   └── ServerScene.unity (Serveur)
│   └── Scripts/
│       ├── Server/ (Code serveur uniquement)
│       ├── Client/ (Code client uniquement)
│       ├── Shared/ (Code partagé)
│       └── Editor/ (Outils Unity Editor)
├── Builds/
│   ├── Client/ (Build client)
│   └── Server/ (Build serveur)
└── ProjectSettings/
```

## 🎯 Cas d'Usage Courants

### Cas 1: Nouveau Client se Connecte
1. Client démarre et appelle `ConnectToServer()`
2. Client envoie une requête d'autorisation
3. Serveur valide et génère un token
4. Client reçoit le token et est autorisé
5. Client peut maintenant échanger des données

### Cas 2: Synchronisation de Données
1. Client envoie des données via `SendData()`
2. Serveur valide l'autorisation
3. Serveur stocke les données via `AuthorizedDataManager`
4. Données sont disponibles pour récupération ultérieure

### Cas 3: Changement de Scène (Client)
1. Appeler `SceneController.Instance.LoadScene("NomScene")`
2. La scène se charge automatiquement
3. Les managers persistent entre les scènes (DontDestroyOnLoad)

## ⚠️ Points Importants

### Serveur
- ✅ Toutes les opérations nécessitent une autorisation
- ✅ Les tokens sont générés automatiquement
- ✅ Le serveur valide chaque requête
- ✅ Traitement orienté données avec file d'attente

### Client
- ✅ Doit être autorisé avant d'envoyer des données
- ✅ Les managers sont des singletons
- ✅ Les événements signalent les changements d'état
- ✅ Gestion automatique de la reconnexion

### Builds
- ✅ Client inclut 3 scènes (MainMenu, Gameplay, Settings)
- ✅ Serveur inclut 1 scène (ServerScene)
- ✅ Symboles de compilation séparent le code
- ✅ Assembly Definitions optimisent la compilation

## 🔍 Debug

### Console Serveur
```
===========================================
CURSOR SERVER - Fully Authorized
===========================================
Server initialization complete
Waiting for authorized client connections...
```

### Console Client
```
===========================================
CURSOR CLIENT
===========================================
Build Target: [Platform]
Client initialization complete
Ready to connect to server
```

## 📞 Événements Disponibles

### Client
- `OnAuthorized` - Déclenché quand autorisé
- `OnAuthorizationFailed` - Déclenché si échec
- `OnDisconnected` - Déclenché à la déconnexion

## 🛠️ Commandes Utiles

### Unity Editor
```
Cursor > Build Client          # Build client uniquement
Cursor > Build Server          # Build serveur uniquement
Cursor > Build Both           # Build les deux
Cursor > Clean Builds         # Nettoyer les builds
```

## 💡 Conseils

1. **Testez en éditeur** avant de build
2. **Utilisez ServerConfig** pour la configuration
3. **Surveillez les logs** pour le debug
4. **Gérez les événements** pour la réactivité
5. **Validez l'autorisation** avant chaque opération

## 📖 Documentation Complète

Voir `README.md` pour la documentation complète et les exemples détaillés.
