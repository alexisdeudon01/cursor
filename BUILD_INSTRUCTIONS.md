# Instructions de Build - Cursor

## 🎯 Objectif

Ce document explique comment générer les deux builds distincts (Client et Serveur) pour le projet Cursor.

## 📋 Prérequis

- Unity 2020.3 LTS ou plus récent installé
- Projet Cursor ouvert dans Unity
- Espace disque suffisant (minimum 500 MB)

## 🔨 Méthode 1: Build via le Menu Unity (Recommandé)

### Option A: Build des Deux (Client + Serveur)

C'est la méthode la plus simple et recommandée pour générer les deux builds en une seule opération.

1. **Ouvrez Unity** avec le projet Cursor
2. **Menu Unity** → `Cursor` → `Build Both (Client + Server)`
3. **Attendez** que les deux builds se terminent
4. **Résultat**: Les builds sont dans:
   - `Builds/Client/CursorClient.exe` (ou .app sur Mac)
   - `Builds/Server/CursorServer.exe` (ou .app sur Mac)

### Option B: Build Client Uniquement

Pour générer uniquement le build client:

1. **Menu Unity** → `Cursor` → `Build Client`
2. **Résultat**: `Builds/Client/CursorClient.exe`

**Caractéristiques du build client:**
- Contient 3 scènes: MainMenu, Gameplay, Settings
- Interface utilisateur complète
- Connexion au serveur
- Gestion des scènes multiples

### Option C: Build Serveur Uniquement

Pour générer uniquement le build serveur:

1. **Menu Unity** → `Cursor` → `Build Server`
2. **Résultat**: `Builds/Server/CursorServer.exe`

**Caractéristiques du build serveur:**
- Contient 1 scène: ServerScene
- Pas d'interface utilisateur
- Autorisation complète activée
- Traitement orienté données

## 🧹 Nettoyage des Builds

Pour supprimer tous les builds existants:

1. **Menu Unity** → `Cursor` → `Clean Builds`
2. Le dossier `Builds/` est complètement supprimé

## 🚀 Lancement des Builds

### Lancer le Serveur

**Windows:**
```cmd
cd Builds\Server
CursorServer.exe
```

**Mac/Linux:**
```bash
cd Builds/Server
./CursorServer.app
```

**Console attendue:**
```
===========================================
CURSOR SERVER - Fully Authorized
===========================================
Server initialization complete
Waiting for authorized client connections...
```

### Lancer le Client

**Windows:**
```cmd
cd Builds\Client
CursorClient.exe
```

**Mac/Linux:**
```bash
cd Builds/Client
./CursorClient.app
```

**Console attendue:**
```
===========================================
CURSOR CLIENT
===========================================
Build Target: [Platform]
Client initialization complete
Ready to connect to server
```

## ⚙️ Configuration de Build

### Modifier les Paramètres de Build

Les paramètres de build sont configurés dans `BuildScript.cs`:

```csharp
// Chemins de build
private const string CLIENT_BUILD_PATH = "Builds/Client/";
private const string SERVER_BUILD_PATH = "Builds/Server/";

// Scènes client
private static string[] GetClientScenes()
{
    return new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Gameplay.unity",
        "Assets/Scenes/Settings.unity"
    };
}

// Scènes serveur
private static string[] GetServerScenes()
{
    return new string[]
    {
        "Assets/Scenes/ServerScene.unity"
    };
}
```

### Symboles de Compilation

Le système de build configure automatiquement les symboles:

- **Build Client**: `CLIENT_BUILD` est défini
- **Build Serveur**: `SERVER_BUILD` est défini

Ces symboles permettent la compilation conditionnelle:

```csharp
#if SERVER_BUILD
    // Code serveur uniquement
#elif CLIENT_BUILD
    // Code client uniquement
#else
    // Code partagé ou éditeur
#endif
```

## 📊 Tailles de Build Estimées

| Build | Taille Estimée | Scènes | Composants |
|-------|---------------|--------|------------|
| Client | ~50-100 MB | 3 | UI + Network + Scenes |
| Server | ~30-50 MB | 1 | Network + Auth + Data |

## 🔍 Vérification des Builds

### Checklist Post-Build

Après génération des builds, vérifiez:

- [ ] Le dossier `Builds/Client/` existe
- [ ] Le dossier `Builds/Server/` existe
- [ ] `CursorClient.exe` est présent et exécutable
- [ ] `CursorServer.exe` est présent et exécutable
- [ ] Les fichiers de données Unity sont présents (Data/)
- [ ] Les logs de build ne contiennent pas d'erreurs

### Test Rapide

1. **Lancez le serveur** en premier
2. **Vérifiez** que le message "Server initialization complete" apparaît
3. **Lancez le client**
4. **Vérifiez** que le message "Client initialization complete" apparaît
5. **Dans le client**, testez la connexion au serveur

## 🐛 Dépannage

### Problème: "Build failed"

**Solutions:**
1. Vérifiez que toutes les scènes existent dans `Assets/Scenes/`
2. Vérifiez qu'il n'y a pas d'erreurs de compilation
3. Nettoyez les builds existants (`Cursor > Clean Builds`)
4. Redémarrez Unity

### Problème: "Scenes not found"

**Solutions:**
1. Ouvrez `ProjectSettings/EditorBuildSettings.asset`
2. Vérifiez que toutes les scènes sont listées
3. Vérifiez que les chemins des scènes sont corrects
4. Régénérez les scènes si nécessaire

### Problème: "Assembly definition errors"

**Solutions:**
1. Vérifiez que tous les fichiers `.asmdef` sont présents
2. Vérifiez les références entre assemblies
3. Recompilez le projet dans Unity
4. Redémarrez Unity

### Problème: Build très lent

**Causes possibles:**
- Premier build (normal)
- Antivirus qui scanne les fichiers
- Peu d'espace disque

**Solutions:**
1. Attendez patiemment pour le premier build
2. Ajoutez le dossier Unity aux exclusions de l'antivirus
3. Libérez de l'espace disque
4. Fermez les applications lourdes

## 📦 Distribution

### Préparer pour Distribution

#### Client

Incluez dans le package client:
```
Client/
├── CursorClient.exe
├── CursorClient_Data/
├── MonoBleedingEdge/
├── UnityCrashHandler64.exe
└── UnityPlayer.dll
```

#### Serveur

Incluez dans le package serveur:
```
Server/
├── CursorServer.exe
├── CursorServer_Data/
├── MonoBleedingEdge/
├── UnityCrashHandler64.exe
└── UnityPlayer.dll
```

### Notes de Déploiement

1. **Client**: Distribuez à tous les utilisateurs
2. **Serveur**: Installez sur un serveur dédié
3. **Configuration**: Le serveur écoute sur le port 7777 par défaut
4. **Pare-feu**: Ouvrez le port 7777 (TCP/UDP)

## 📝 Historique des Versions

### Version 0.1.0 (Actuelle)
- Build client avec 3 scènes
- Build serveur avec autorisation complète
- Système de build automatisé
- Documentation complète

## 🔗 Ressources

- **Documentation complète**: `README.md`
- **Référence rapide**: `QUICK_REFERENCE.md`
- **Architecture**: `ARCHITECTURE.md`

## 💡 Conseils

1. **Toujours** générer les deux builds ensemble pour assurer la compatibilité
2. **Testez** les builds avant distribution
3. **Conservez** les logs de build pour le dépannage
4. **Documentez** toute modification du processus de build
5. **Versionnez** vos builds pour traçabilité

## 📞 Support

En cas de problème avec le build:
1. Vérifiez cette documentation
2. Consultez les logs Unity
3. Vérifiez les erreurs de compilation
4. Ouvrez une issue sur GitHub si nécessaire

---

**Dernière mise à jour**: Janvier 2026
**Auteur**: Cursor Development Team
