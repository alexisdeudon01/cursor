---
name: agent
model: fast
---

# Agent Auto-Évolutif - TheBestClient

> **Version**: 1.0.0  
> **Auto-Amélioration**: Activée via EvoAgentX  
> **Rollback**: Automatique si régression détectée

---

## 1. Mission

Développer un jeu 2D client-serveur avec serveur **full authoritative** et architecture **Data-Oriented Design**.

---

## 2. Principes fondamentaux

| Principe | Règle |
|----------|-------|
| **Serveur Autoritaire** | TOUTE la logique sur le serveur |
| **Client Passif** | Envoie inputs, reçoit état, affiche |
| **DOD** | Structs pour données réseau |
| **Single Executable** | UN seul build, distinction par arguments |
| **NGO Only** | Netcode for GameObjects uniquement |
| **No Third-Party** | Aucun service tiers (Unity Services, etc.) |

---

## 3. Règles Réseau - NETCODE FOR GAMEOBJECTS (NGO)

### ⚠️ RÈGLE CRITIQUE : NGO UNIQUEMENT

**UTILISER :**
- ✅ `Unity.Netcode` (Netcode for GameObjects)
- ✅ `Unity.Netcode.Transports.UTP` (Unity Transport)
- ✅ Packages du repo Unity officiel uniquement

**INTERDIT :**
- ❌ Unity Services (Relay, Lobby, Matchmaking, etc.)
- ❌ Unity Gaming Services (UGS)
- ❌ Photon, Mirror, FishNet ou autres solutions réseau
- ❌ Services cloud tiers (AWS, Firebase, etc.)
- ❌ Tout package non-officiel Unity

### Packages autorisés (Unity Registry uniquement)

```json
{
  "dependencies": {
    "com.unity.netcode.gameobjects": "2.x.x",
    "com.unity.transport": "2.x.x",
    "com.unity.collections": "2.x.x",
    "com.unity.burst": "1.x.x",
    "com.unity.mathematics": "1.x.x",
    "com.unity.inputsystem": "1.x.x",
    "com.unity.ui": "2.x.x",
    "com.unity.render-pipelines.universal": "17.x.x"
  }
}
```


---

## 4. Architecture UN SEUL BUILD

### Structure des scènes existantes

```
Assets/Scenes/
├── Server.unity       ← Chargée si --server (headless)
├── Client.unity       ← Chargée si --client (entrée)
├── Menu.unity         ← Menu principal
└── Game.unity         ← Scène de jeu
```

### Bootstrap existant

Le projet utilise déjà un système de bootstrap :
- `ServerBootstrap.cs` : Démarre le serveur
- `ClientBootstrap.cs` : Démarre le client
- `NetworkBootstrap.cs` : Configuration réseau

---



---

## 6. Règles serveur

```csharp
// ✅ CORRECT - Dans ServerBootstrap.cs
NetworkManager.Singleton.StartServer();

// ❌ INTERDIT
NetworkManager.Singleton.StartHost();  // Jamais !
```

Le serveur :
- Charge Server.unity uniquement
- Valide tous les inputs via RpcHandlers
- Gère les sessions via GameSessionManager
- Aucun rendu graphique (headless possible)

---

## 7. Data-Oriented Design (DOD)

### Structs existantes dans le projet

```csharp
// Assets/Scripts/Networking/Data/
PlayerNetworkData.cs   // Données joueur réseau
ClientNetworkData.cs   // Données client

// Assets/Scripts/Core/StateSync/
GameCommandProtocol.cs // Protocole commandes
MapConfigData.cs       // Configuration map
```

---

## 8. Prefabs réseau existants

```
Assets/Prefabs/Network/
├── NetworkManagerRoot.prefab   ← NetworkManager principal
├── SessionRpcHub.prefab        ← Hub RPC
├── NetworkBootstrapUI.prefab   ← UI bootstrap
└── Square.prefab               ← Prefab test

Assets/Prefabs/Pawns/
└── CirclePawn.prefab           ← Pawn joueur
```

---

## 9. Games existants

Le projet supporte plusieurs types de jeux :

```
Assets/Scripts/Games/
├── CircleGame/
│   ├── CircleGameDefinition.cs
│   └── CirclePawn.cs
└── SquareGame/
    └── SquareGameDefinition.cs

Assets/Resources/Games/
├── CircleGame.asset
└── SquareGame.asset
```

---

## 10. Auto-Amélioration

### Métriques d'évaluation

| Métrique | Poids | Description |
|----------|-------|-------------|
| Server Authority | 20% | StartServer(), pas StartHost() |
| NGO Compliance | 20% | Netcode only, pas de services tiers |
| Single Build | 10% | Un seul exécutable |
| UI Toolkit | 15% | UXML + USS |
| DOD Compliance | 15% | Structs INetworkSerializable |
| Network Flow | 10% | Séquence correcte via RpcHandlers |
| Build Success | 10% | Compilation OK |

### Ce que l'agent peut améliorer

- ✅ Scripts C# dans Assets/Scripts/
- ✅ Fichiers UXML dans Assets/UI Toolkit/
- ✅ Fichiers USS dans Assets/UI Toolkit/
- ✅ Ce fichier agent.md (lui-même)
- ✅ Packages/manifest.json (Unity Registry only)
- ✅ Documentation
- ❌ Fichiers Unity binaires (.unity, .prefab) - lecture seule
- ❌ Services tiers - JAMAIS

### Checklist avant commit

- [ ] Pas de `StartHost()` → utilise `StartServer()`
- [ ] Pas de `using Unity.Services.*`
- [ ] Données réseau = structs avec `INetworkSerializable`
- [ ] UI = UXML + USS
- [ ] Packages = Unity Registry uniquement

---

## 11. Structure du projet

```
Assets/
├── Editor/                 ← Scripts éditeur
├── Prefabs/
│   ├── Network/           ← Prefabs réseau
│   ├── Pawns/             ← Prefabs joueurs
│   └── UI/                ← Prefabs UI
├── Resources/
│   ├── Games/             ← Définitions jeux
│   └── NetworkConfig.asset
├── Scenes/
│   ├── Server.unity       ← Serveur headless
│   ├── Client.unity       ← Client graphique
│   ├── Menu.unity
│   └── Game.unity
├── Scripts/
│   ├── Core/              ← Logique métier
│   ├── Game/              ← Gameplay
│   ├── Games/             ← Définitions jeux
│   ├── Networking/        ← NGO complet
│   ├── Service/           ← Services locaux
│   └── UI/                ← Controllers UI
├── Settings/              ← URP, Build Profiles
├── TextMesh Pro/          ← Fonts
└── UI Toolkit/            ← UXML + USS
```

---

*Dernière mise à jour: AUTO_DATE*
*Score actuel: AUTO_SCORE*
