# Cursor - Unity Client-Server Architecture

Un projet Unity avec une architecture client-serveur complète, comprenant un serveur entièrement autorisé et un client multi-scènes.

## 🎯 Fonctionnalités

### Serveur Autorisé Complet
- **ServerAuthManager** - Gestion complète de l'autorisation des clients
- **AuthorizedDataManager** - Traitement orienté données avec autorisation complète
- **ServerConfig** - Configuration flexible du serveur
- Système de tokens sécurisés pour toutes les connexions
- Validation et révocation des autorisations en temps réel

### Client Multi-Scènes
- **MainMenu** - Scène de menu principal
- **Gameplay** - Scène de jeu
- **Settings** - Scène de paramètres
- **ClientNetworkManager** - Gestion réseau côté client
- **SceneController** - Contrôle des transitions de scènes

### Architecture Modulaire
- **Shared** - Code partagé entre client et serveur
- **Assembly Definitions** - Séparation claire entre client, serveur et code partagé
- **Build Script** - Système de build automatisé pour générer deux exécutables distincts

## 🏗️ Structure du Projet

```
Assets/
├── Scenes/
│   ├── MainMenu.unity      # Scène menu principal (Client)
│   ├── Gameplay.unity      # Scène de jeu (Client)
│   ├── Settings.unity      # Scène de paramètres (Client)
│   └── ServerScene.unity   # Scène serveur dédiée
├── Scripts/
│   ├── Server/
│   │   ├── ServerAuthManager.cs         # Gestionnaire d'autorisation
│   │   ├── ServerConfig.cs              # Configuration serveur
│   │   ├── AuthorizedDataManager.cs     # Gestion des données
│   │   ├── ServerInitializer.cs         # Initialisation serveur
│   │   └── CursorServer.asmdef
│   ├── Client/
│   │   ├── ClientNetworkManager.cs      # Réseau client
│   │   ├── SceneController.cs           # Contrôle des scènes
│   │   ├── ClientInitializer.cs         # Initialisation client
│   │   └── CursorClient.asmdef
│   ├── Shared/
│   │   ├── NetworkProtocol.cs           # Protocole réseau partagé
│   │   └── CursorShared.asmdef
│   └── Editor/
│       ├── BuildScript.cs               # Scripts de build
│       └── CursorEditor.asmdef
```

## 🚀 Installation

### Prérequis
- Unity 2020.3 LTS ou plus récent
- .NET Framework 4.x

### Étapes d'installation
1. Clonez le dépôt :
   ```bash
   git clone https://github.com/alexisdeudon01/cursor.git
   ```

2. Ouvrez le projet dans Unity Hub

3. Le projet est prêt à être utilisé !

## 🔨 Génération des Builds

### Via le Menu Unity

#### Build Client
1. Ouvrez Unity
2. Allez dans le menu `Cursor > Build Client`
3. Le client sera généré dans `Builds/Client/`

#### Build Serveur
1. Ouvrez Unity
2. Allez dans le menu `Cursor > Build Server`
3. Le serveur sera généré dans `Builds/Server/`

#### Build les Deux
1. Ouvrez Unity
2. Allez dans le menu `Cursor > Build Both (Client + Server)`
3. Les deux builds seront générés automatiquement

### Nettoyage
- Menu `Cursor > Clean Builds` pour supprimer tous les builds existants

## 📋 Utilisation

### Démarrage du Serveur

1. Lancez l'exécutable serveur depuis `Builds/Server/`
2. Le serveur s'initialisera automatiquement avec :
   - Port par défaut : 7777
   - Autorisation complète activée
   - Traitement orienté données activé

### Démarrage du Client

1. Lancez l'exécutable client depuis `Builds/Client/`
2. Le client démarrera sur la scène MainMenu
3. Utilisez `ClientNetworkManager` pour vous connecter au serveur :
   ```csharp
   ClientNetworkManager.Instance.ConnectToServer();
   ```

## 🔐 Système d'Autorisation

### Côté Serveur
```csharp
// Autoriser un client
bool authorized = ServerAuthManager.Instance.AuthorizeClient(clientId, credentials);

// Valider un token
bool valid = ServerAuthManager.Instance.ValidateToken(token);

// Révoquer l'autorisation
ServerAuthManager.Instance.RevokeAuthorization(clientId);
```

### Gestion des Données Orientées
```csharp
// Stocker des données (nécessite autorisation)
AuthorizedDataManager.Instance.StoreData(clientId, "key", data);

// Récupérer des données (nécessite autorisation)
object data = AuthorizedDataManager.Instance.RetrieveData(clientId, "key");

// Supprimer des données (nécessite autorisation)
bool deleted = AuthorizedDataManager.Instance.DeleteData(clientId, "key");
```

## 🎮 Contrôle des Scènes (Client)

```csharp
// Charger une scène spécifique
SceneController.Instance.LoadMainMenu();
SceneController.Instance.LoadGameplay();
SceneController.Instance.LoadSettings();

// Charger une scène par nom
SceneController.Instance.LoadScene("MainMenu");

// Charger de manière asynchrone
SceneController.Instance.LoadSceneAsync("Gameplay");
```

## ⚙️ Configuration

### Configuration du Serveur
Créez une ressource `ServerConfig` :
1. Clic droit dans le dossier Assets
2. `Create > Cursor > Server Configuration`
3. Configurez les paramètres :
   - Port
   - Max Connections
   - Token Expiration
   - Data Sync Interval

### Symboles de Build
Le système de build configure automatiquement :
- `SERVER_BUILD` pour les builds serveur
- `CLIENT_BUILD` pour les builds client

## 🧪 Test en Éditeur

### Test du Serveur
1. Ouvrez la scène `ServerScene.unity`
2. Cliquez sur Play
3. Le serveur s'initialisera en mode éditeur

### Test du Client
1. Ouvrez la scène `MainMenu.unity`
2. Cliquez sur Play
3. Le client s'initialisera en mode éditeur

## 📝 Architecture Technique

### Pattern Singleton
Tous les gestionnaires principaux utilisent le pattern Singleton :
- `ServerAuthManager`
- `AuthorizedDataManager`
- `ClientNetworkManager`
- `SceneController`

### Définitions d'Assembly
Le projet utilise des Assembly Definitions pour :
- Séparer le code client et serveur
- Optimiser les temps de compilation
- Faciliter la maintenance

### Protocole Réseau
Le protocole réseau partagé définit :
- Types de messages standardisés
- Taille maximale des paquets
- Version du protocole

## 🔄 Workflow de Développement

1. **Développement du serveur** : Modifiez les scripts dans `Assets/Scripts/Server/`
2. **Développement du client** : Modifiez les scripts dans `Assets/Scripts/Client/`
3. **Code partagé** : Ajoutez du code commun dans `Assets/Scripts/Shared/`
4. **Test** : Testez en mode éditeur
5. **Build** : Utilisez le menu Cursor pour générer les builds
6. **Déploiement** : Distribuez les exécutables client et serveur séparément

## 🤝 Contribution

Les contributions sont les bienvenues ! N'hésitez pas à :
1. Fork le projet
2. Créer une branche pour votre fonctionnalité
3. Commit vos changements
4. Push vers la branche
5. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT.

## 👥 Auteur

Développé pour Cursor - Architecture Client-Serveur Unity

## 📞 Support

Pour toute question ou problème, ouvrez une issue sur GitHub.