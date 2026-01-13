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

---

## 3. Architecture UN SEUL BUILD

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
        NetworkManager.Singleton.StartServer();
    }
    else
    {
        // Charge le menu client (graphique)
        SceneManager.LoadScene("MainMenu");
        // StartClient() appelé après connexion UI
    }
}
```

### Build unique

```yaml
# Le build contient TOUTES les scènes
unity-builder:
  buildName: TheBestGame
  # Pas de buildTarget séparé pour client/serveur
```

---

## 4. Client Full Graphique - UI TOOLKIT

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
Assets/
├── UI/
│   ├── Styles/
│   │   ├── MainTheme.uss          ← Thème principal
│   │   ├── Buttons.uss            ← Styles boutons
│   │   └── Common.uss             ← Styles partagés
│   ├── Templates/
│   │   ├── MainMenu.uxml          ← Menu principal
│   │   ├── Lobby.uxml             ← Écran lobby
│   │   ├── GameHUD.uxml           ← HUD en jeu
│   │   └── Components/
│   │       ├── PlayerCard.uxml    ← Composant réutilisable
│   │       └── LobbyItem.uxml     ← Item liste lobby
│   └── Scripts/
│       ├── MainMenuController.cs
│       ├── LobbyController.cs
│       └── GameHUDController.cs
```

### Exemple UXML

```xml
<!-- MainMenu.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="container">
        <ui:Label text="TheBestGame" class="title"/>
        <ui:TextField name="player-name" label="Nom"/>
        <ui:Button name="btn-connect" text="Connexion" class="btn-primary"/>
    </ui:VisualElement>
</ui:UXML>
```

### Exemple USS

```css
/* MainTheme.uss */
.container {
    flex-grow: 1;
    justify-content: center;
    align-items: center;
    background-color: rgb(30, 30, 40);
}

.title {
    font-size: 48px;
    color: rgb(255, 255, 255);
    -unity-font-style: bold;
}

.btn-primary {
    padding: 15px 30px;
    background-color: rgb(66, 135, 245);
    border-radius: 8px;
    color: white;
}

.btn-primary:hover {
    background-color: rgb(100, 160, 255);
}
```

### Controller C#

```csharp
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private TextField playerNameField;
    private Button connectButton;
    
    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        playerNameField = root.Q<TextField>("player-name");
        connectButton = root.Q<Button>("btn-connect");
        
        connectButton.clicked += OnConnectClicked;
    }
    
    private void OnConnectClicked()
    {
        string playerName = playerNameField.value;
        // Envoyer au serveur...
    }
}
```

---

## 5. Découverte de structure

L'agent découvre la structure en **LISANT** les fichiers Unity :

```
EditorBuildSettings.asset → Liste des scènes
       ↓
Scene.unity → Hierarchy, GUIDs composants
       ↓
Script.cs.meta → GUID → chemin script
       ↓
Script.cs → Analyse code
```

**JAMAIS** assumer les chemins. Toujours tracer via GUIDs.

---

## 6. Règles serveur

```csharp
// ✅ CORRECT
NetworkManager.Singleton.StartServer();

// ❌ INTERDIT
NetworkManager.Singleton.StartHost();
```

Le serveur :
- Charge ServerScene uniquement
- Valide tous les inputs
- Simule la physique
- Gère l'état du jeu
- Aucun rendu graphique (headless possible)

---

## 7. Flux réseau

```
1. Client démarre → MainMenu.unity
2. Client saisit nom + IP → UI Toolkit
3. Client connecte → StartClient()
4. Serveur demande nom
5. Client envoie nom
6. Client demande lobbies → Lobby.unity
7. Serveur envoie liste
8. Client rejoint/crée lobby
9. Partie démarre → Game.unity
10. Boucle: Input → Validation → État → Rendu
```

---

## 8. CLI

```bash
# Serveur (headless, pas de graphiques)
./TheBestGame --server --port 7777

# Client (full graphique UI Toolkit)
./TheBestGame --client --ip 192.168.1.1 --port 7777
```

---

## 9. Auto-Amélioration

Cet agent s'améliore automatiquement via EvoAgentX :

1. **Analyse** : Évalue le code vs ces règles
2. **Score** : Calcule un score de conformité
3. **Amélioration** : Propose des modifications
4. **Validation** : Compare score avant/après
5. **Rollback** : Annule si régression

### Métriques d'évaluation

| Métrique | Poids | Description |
|----------|-------|-------------|
| Server Authority | 25% | Pas de logique client |
| Single Build | 15% | Un seul exécutable |
| UI Toolkit | 20% | UXML + USS uniquement |
| Structure Discovery | 15% | Lecture fichiers Unity |
| Network Flow | 15% | Séquence correcte |
| Build Success | 10% | Compilation OK |

### Historique des versions

| Version | Date | Score | Changements |
|---------|------|-------|-------------|
| 1.0.0 | AUTO | - | Version initiale |

---

## 10. Ce que l'agent peut modifier

- ✅ Scripts C# dans Assets/Scripts/
- ✅ Fichiers UXML dans Assets/UI/
- ✅ Fichiers USS dans Assets/UI/
- ✅ Ce fichier agent.md (lui-même)
- ✅ Workflow CI/CD
- ✅ Documentation
- ❌ Fichiers Unity binaires (.unity, .prefab) - lecture seule

---

*Dernière mise à jour: 2026-01-13 21:52*
*Score actuel: AUTO_SCORE*
