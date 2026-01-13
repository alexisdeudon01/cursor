# TheBestClient - Auto-Evolving Game

Jeu 2D avec serveur full authoritative, auto-amélioré par EvoAgentX.

# Agent Auto-Évolutif - TheBestClient

> **Version**: 1.0.0  \
> **Auto-Amélioration**: Activée via EvoAgentX  \
> **Rollback**: Automatique si régression détectée

## 1. Mission

Développer un jeu 2D client-serveur avec serveur **full authoritative** et architecture **Data-Oriented Design**.

## 2. Principes fondamentaux

| Principe | Règle |
| --- | --- |
| **Serveur Autoritaire** | TOUTE la logique sur le serveur |
| **Client Passif** | Envoie inputs, reçoit état, affiche |
| **DOD** | Structs pour données réseau |
| **Single Executable** | UN seul build, distinction par arguments |
| **NGO Only** | Netcode for GameObjects uniquement |
| **No Third-Party** | Aucun service tiers (Unity Services, etc.) |

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

## 5. Fichiers Unity & structure basée sur `.unity`

### Ce qu'est un fichier `.unity`

Un fichier `.unity` est une scène Unity sérialisée (format YAML) qui décrit **la structure réelle** de la scène (GameObjects, composants, liens, valeurs). **La structure d’une scène doit être déduite uniquement du contenu du fichier `.unity`** : aucune hypothèse externe ni documentation ne doit remplacer ce qui est effectivement sérialisé.

### Règle de structure

- ✅ La structure d’une scène se détermine **exclusivement** depuis le fichier `.unity`.
- ❌ Pas d’inférence depuis des scripts ou des docs si la scène ne le reflète pas.
- ✅ Les fichiers `.unity` et `.prefab` sont **binaires/serialisés** et **en lecture seule** pour l’agent (pas d’édition automatique).

### Types de fichiers principaux

| Fichier | Rôle | Notes |
| --- | --- | --- |
| `.unity` | Scènes Unity | Structure réelle de la scène (YAML sérialisé) |
| `.prefab` | Prefabs Unity | Modèles d’objets sérialisés |
| `.asset` | ScriptableObjects/Assets | Configs data-driven |
| `.cs` | Scripts C# | Logique métier / réseau / UI |
| `.uxml` | UI Toolkit | Structure UI |
| `.uss` | UI Toolkit | Styles UI |
| `.meta` | Métadonnées Unity | GUID + import settings |

### Qu’est-ce qu’un `.meta` ?

Le fichier `.meta` est généré par Unity pour **lier un asset à un GUID stable**. Il contient aussi des paramètres d’import. **Ne jamais supprimer un `.meta`** : Unity perdrait les références.

## 6. Règles serveur

```csharp
// ✅ CORRECT - Dans ServerBootstrap.cs
NetworkManager.Singleton.StartServer();

// ❌ INTERDIT
NetworkManager.Singleton.StartHost();  // Jamais !
```

Le serveur :
- Charge `Server.unity` uniquement
- Valide tous les inputs via `RpcHandlers`
- Gère les sessions via `GameSessionManager`
- Aucun rendu graphique (headless possible)

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

## 10. Auto-Amélioration

### Métriques d'évaluation

| Métrique | Poids | Description |
| --- | --- | --- |
| Server Authority | 20% | `StartServer()`, pas `StartHost()` |
| NGO Compliance | 20% | Netcode only, pas de services tiers |
| Single Build | 10% | Un seul exécutable |
| UI Toolkit | 15% | UXML + USS |
| DOD Compliance | 15% | Structs `INetworkSerializable` |
| Network Flow | 10% | Séquence correcte via `RpcHandlers` |
| Build Success | 10% | Compilation OK |

### Ce que l'agent peut améliorer

- ✅ Scripts C# dans `Assets/Scripts/`
- ✅ Fichiers UXML dans `Assets/UI Toolkit/`
- ✅ Fichiers USS dans `Assets/UI Toolkit/`
- ✅ Documentation
- ✅ `Packages/manifest.json` (Unity Registry only)
- ❌ Fichiers Unity binaires (`.unity`, `.prefab`) - lecture seule
- ❌ Services tiers - JAMAIS

### Checklist avant commit

- [ ] Pas de `StartHost()` → utilise `StartServer()`
- [ ] Pas de `using Unity.Services.*`
- [ ] Données réseau = structs avec `INetworkSerializable`
- [ ] UI = UXML + USS
- [ ] Packages = Unity Registry uniquement

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

## 12. Workflow d'amélioration automatique

- Exécuté **toutes les heures** via GitHub Actions.
- Calcul du coût total à chaque run.
- **Si le total mensuel dépasse 200 €**, les étapes lourdes sont arrêtées.
- Les vérifications Docker/Evaluation sont protégées pour éviter les erreurs récurrentes.

## 13. Tests & variables « smart »

### Propositions de variables smart

- `SIM_SEED` : seed de simulation pour reproduire un état réseau.
- `NET_TICK_RATE` : fréquence de tick réseau (profilage perf).
- `INPUT_BUFFER_MS` : latence simulée pour stress test.
- `RPC_BUDGET` : limite max d’appels RPC par frame.
- `STATE_DELTA_THRESHOLD` : seuil de diff avant envoi d’un delta.
- `SPAWN_BATCH_SIZE` : taille de batch pour spawns.
- `SYNC_BACKPRESSURE_MS` : délai minimal entre snapshots.

### Algorithmes à proposer via Codex

1. **Diff-based snapshotting** : envoi uniquement des deltas d’état.
2. **Compression des bitfields** : packing des flags réseau.
3. **Prediction + reconciliation** : côté client, uniquement visuel.
4. **Interest management** : filtrer les entités par zone.
5. **Batching RPC** : regrouper les RPC par tick.
6. **State buckets** : prioriser les entités critiques.

### Évaluation par Codex

- Sélectionner un modèle d’IA via Codex pour évaluer la pertinence des changements.
- Retour possible :
  - ✅ *Amélioration validée*
  - ⚠️ *Amélioration partielle*
  - ❌ *Régression détectée (rollback)*

## Usage

```bash
# Serveur (headless)
./TheBestGame --server --port 7777

# Client (graphique)
./TheBestGame --client --ip 192.168.1.1 --port 7777
```

*Dernière mise à jour: AUTO_DATE*  \
*Score actuel: AUTO_SCORE*
