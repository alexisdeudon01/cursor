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
    "com.unity.ui": "2.x.x"
  }
}
```

### Composants NGO à utiliser

```csharp
// ✅ CORRECT - Composants NGO
using Unity.Netcode;

public class MyNetworkBehaviour : NetworkBehaviour
{
    // Variables synchronisées
    private NetworkVariable<int> score = new();
    
    // Listes synchronisées
    private NetworkList<PlayerData> players;
    
    // RPCs
    [ServerRpc]
    private void SendInputServerRpc(PlayerInputData input) { }
    
    [ClientRpc]
    private void UpdateStateClientRpc(GameStateData state) { }
}
```

### NetworkManager - Configuration manuelle

```csharp
// ✅ CORRECT - Connexion directe IP
var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
transport.SetConnectionData("192.168.1.1", 7777);
NetworkManager.Singleton.StartClient();

// ❌ INTERDIT - Unity Relay
// RelayService.Instance.CreateAllocationAsync()  // NON !
// NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData()  // NON !
```

### Lobby/Matchmaking - Implémentation custom

```csharp
// ✅ CORRECT - Lobby géré par le serveur autoritaire
public struct LobbyData : INetworkSerializable
{
    public FixedString64Bytes lobbyName;
    public int playerCount;
    public int maxPlayers;
    public bool isPublic;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref lobbyName);
        serializer.SerializeValue(ref playerCount);
        serializer.SerializeValue(ref maxPlayers);
        serializer.SerializeValue(ref isPublic);
    }
}

// Le serveur gère les lobbies en mémoire
private NetworkList<LobbyData> lobbies;

// ❌ INTERDIT
// LobbyService.Instance.CreateLobbyAsync()  // NON !
// await Lobbies.Instance.QuickJoinLobbyAsync()  // NON !
```

---

## 4. Architecture UN SEUL BUILD

### ⚠️ RÈGLE CRITIQUE : UN SEUL PROJET UNITY, UN SEUL BUILD

La distinction Client/Serveur se fait par :
1. **Arguments CLI** : `--server` ou `--client`
2. **Scènes différentes** chargées au runtime

### Structure des scènes

```
Assets/Scenes/
├── ServerScene.unity      ← Chargée si --server
├── MainMenu.unity         ← Chargée si --client (entrée)
├── Lobby.unity            ← Client: sélection lobby
├── Game.unity             ← Client: jeu en cours
└── ...                    ← Autres scènes client
```

### Logique de démarrage

```csharp
void Start()
{
    if (IsServerMode())
    {
        // Charge UNIQUEMENT la scène serveur
        SceneManager.LoadScene("ServerScene");
        NetworkManager.Singleton.StartServer();  // ✅ PAS StartHost()
    }
    else
    {
        // Charge le menu client (graphique)
        SceneManager.LoadScene("MainMenu");
        // StartClient() appelé après saisie IP dans UI
    }
}
```

---

## 5. Client Full Graphique - UI TOOLKIT

### ⚠️ RÈGLE : Le client utilise EXCLUSIVEMENT UI Toolkit (UXML + USS)

**PAS de :**
- ❌ Unity UI (Canvas, Button legacy)
- ❌ IMGUI (OnGUI)
- ❌ TextMeshPro standalone

**UNIQUEMENT :**
- ✅ UXML (structure)
- ✅ USS (styles)
- ✅ UIDocument component
- ✅ VisualElement API en C#

### Structure UI Toolkit

```
Assets/UI/
├── Styles/
│   ├── MainTheme.uss
│   └── Common.uss
├── Templates/
│   ├── MainMenu.uxml
│   ├── Lobby.uxml
│   ├── GameHUD.uxml
│   └── Components/
│       ├── PlayerCard.uxml
│       └── LobbyItem.uxml
└── Scripts/
    ├── MainMenuController.cs
    ├── LobbyController.cs
    └── GameHUDController.cs
```

---

## 6. Règles serveur

```csharp
// ✅ CORRECT
NetworkManager.Singleton.StartServer();

// ❌ INTERDIT
NetworkManager.Singleton.StartHost();  // Jamais !
```

Le serveur :
- Charge ServerScene uniquement
- Valide tous les inputs
- Simule la physique
- Gère l'état du jeu (lobbies, parties, scores)
- Gère le matchmaking en mémoire (pas de service externe)
- Aucun rendu graphique (headless possible)

---

## 7. Data-Oriented Design (DOD)

### Structs pour données réseau

```csharp
// ✅ CORRECT - Struct avec INetworkSerializable
public struct PlayerInputData : INetworkSerializable
{
    public float moveX;
    public float moveY;
    public bool jump;
    public bool action;
    public uint tick;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref moveX);
        serializer.SerializeValue(ref moveY);
        serializer.SerializeValue(ref jump);
        serializer.SerializeValue(ref action);
        serializer.SerializeValue(ref tick);
    }
}

// ❌ INTERDIT - Class pour données réseau
public class PlayerInputData { }  // NON !
```

---

## 8. Flux réseau (NGO natif)

```
1. Client démarre → MainMenu.unity (UI Toolkit)
2. Client saisit nom + IP serveur
3. Client configure UnityTransport avec IP:Port
4. Client appelle StartClient()
5. OnClientConnected → Serveur envoie liste lobbies
6. Client choisit/crée lobby (ServerRpc)
7. Serveur valide et met à jour NetworkList<LobbyData>
8. Partie démarre → Synchronisation via NetworkVariables
9. Boucle: Input(ServerRpc) → Validation → État(ClientRpc) → Rendu
```

---

## 9. CLI

```bash
# Serveur (headless, pas de graphiques)
./TheBestGame --server --port 7777

# Client (full graphique UI Toolkit)
./TheBestGame --client --ip 192.168.1.1 --port 7777
```

---

## 10. Packages autorisés à ajouter

L'agent peut ajouter des packages **UNIQUEMENT** du Unity Registry officiel :

```bash
# ✅ AUTORISÉ (Unity Registry)
com.unity.netcode.gameobjects
com.unity.transport
com.unity.collections
com.unity.burst
com.unity.mathematics
com.unity.inputsystem
com.unity.ui
com.unity.textmeshpro
com.unity.addressables
com.unity.localization
com.unity.cinemachine
com.unity.2d.sprite
com.unity.2d.tilemap
com.unity.2d.animation

# ❌ INTERDIT
com.unity.services.*          # Unity Gaming Services
com.unity.multiplayer.*       # Multiplayer Services
Tout package OpenUPM
Tout package GitHub externe
Tout asset payant
```

---

## 11. Auto-Amélioration

Cet agent s'améliore automatiquement via EvoAgentX :

1. **Analyse** : Évalue le code vs ces règles
2. **Score** : Calcule un score de conformité
3. **Amélioration** : Propose des modifications
4. **Validation** : Compare score avant/après
5. **Rollback** : Annule si régression

### Métriques d'évaluation

| Métrique | Poids | Description |
|----------|-------|-------------|
| Server Authority | 20% | Pas de logique client, StartServer() |
| NGO Compliance | 20% | Netcode only, pas de services tiers |
| Single Build | 10% | Un seul exécutable |
| UI Toolkit | 15% | UXML + USS uniquement |
| DOD Compliance | 15% | Structs pour données réseau |
| Network Flow | 10% | Séquence correcte |
| Build Success | 10% | Compilation OK |

---

## 12. Ce que l'agent peut modifier

- ✅ Scripts C# dans Assets/Scripts/
- ✅ Fichiers UXML dans Assets/UI/
- ✅ Fichiers USS dans Assets/UI/
- ✅ Ce fichier agent.md (lui-même)
- ✅ Packages/manifest.json (Unity Registry only)
- ✅ Workflow CI/CD
- ✅ Documentation
- ❌ Fichiers Unity binaires (.unity, .prefab) - lecture seule
- ❌ Services tiers - JAMAIS

---

## 13. Checklist avant commit

- [ ] Pas de `StartHost()` → utilise `StartServer()`
- [ ] Pas de `using Unity.Services.*`
- [ ] Pas de Relay, Lobby Service, UGS
- [ ] Données réseau = structs avec `INetworkSerializable`
- [ ] UI = UXML + USS (pas de Canvas)
- [ ] Packages = Unity Registry uniquement
- [ ] Un seul build, distinction par CLI

---

*Dernière mise à jour: $(date '+%Y-%m-%d %H:%M')*
*Score actuel: AUTO_SCORE*
