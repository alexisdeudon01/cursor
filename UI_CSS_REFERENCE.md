# 🎨 Session Lobby - Référence Complète des Classes CSS

## 📋 Index des Classes

### Layout Principal
- [lobby-root](#lobby-root)
- [header](#header)
- [main-content](#main-content)
- [footer](#footer)

### Panneaux
- [panel](#panel)
- [sessions-panel](#sessions-panel)
- [log-panel](#log-panel)

### Sessions
- [sessions-list](#sessions-list)
- [session-item](#session-item)
- [create-session](#create-session)

### Popup
- [popup-overlay](#popup-overlay)
- [session-popup](#session-popup)
- [popup-header](#popup-header)
- [popup-info](#popup-info)
- [popup-actions](#popup-actions)

### Joueurs
- [popup-players-list](#popup-players-list)
- [popup-player-item](#popup-player-item)

### Flow Guide (Nouveau)
- [flow-step-container](#flow-step-container)
- [action-required](#action-required)
- [flow-success](#flow-success)

---

## 📖 Détails des Classes

### Layout Principal

#### `.lobby-root`
Conteneur principal de l'interface lobby.

```css
.lobby-root {
    flex-grow: 1;
    background-color: rgb(25, 25, 30);
    padding: 20px;
}
```

**Usage** :
```xml
<ui:VisualElement name="lobby-root" class="lobby-root">
    <!-- Contenu lobby -->
</ui:VisualElement>
```

---

#### `.header`
Barre d'en-tête avec titre et statut de connexion.

```css
.header {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding-bottom: 20px;
    border-bottom-width: 2px;
    border-bottom-color: rgb(75, 75, 90);
    margin-bottom: 20px;
}
```

**Sous-classes** :
- `.title-container` - Conteneur titre + icône
- `.title-icon` - Icône du jeu (🎮)
- `.title` - Texte du titre
- `.status-container` - Conteneur statut
- `.connection-status` - Badge de connexion

**Exemple** :
```xml
<ui:VisualElement name="header" class="header">
    <ui:VisualElement class="title-container">
        <ui:Label text="🎮" class="title-icon"/>
        <ui:Label text="Game Lobby" class="title"/>
    </ui:VisualElement>
    <ui:Label text="● Connected" class="connection-status"/>
</ui:VisualElement>
```

---

#### `.main-content`
Zone principale contenant les panneaux sessions et console.

```css
.main-content {
    flex-grow: 1;
    flex-direction: row;
}
```

**Responsive** :
```css
@media (max-width: 800px) {
    .main-content {
        flex-direction: column;
    }
}
```

---

### Création de Session

#### `.create-session`
Zone de création de nouvelle session avec bordure bleue mise en évidence.

```css
.create-session {
    padding: 20px;
    background-color: rgb(45, 45, 55);
    border-radius: 10px;
    margin-bottom: 12px;
    border-width: 2px;
    border-color: rgb(75, 140, 230);
}
```

**Sous-classes** :
- `.create-session-title` - Titre "📝 Create New Session"
- `.session-form` - Formulaire de création
- `.session-name-field` - Champ nom de session
- `.creation-hint` - Zone de hint visuel
- `.hint-text` - Texte du hint

**Exemple Complet** :
```xml
<ui:VisualElement class="create-session">
    <ui:Label text="📝 Create New Session" class="create-session-title"/>
    <ui:VisualElement class="session-form">
        <ui:TextField name="SessionNameField" label="Session Name:" class="session-name-field"/>
        <ui:VisualElement class="creation-hint">
            <ui:Label text="💡 Tip: Choose a descriptive name" class="hint-text"/>
        </ui:VisualElement>
    </ui:VisualElement>
    <ui:Button text="+ Create Session" class="create-btn"/>
</ui:VisualElement>
```

**Effets** :
- Hover sur `.create-btn` → Scale 1.02
- Active → Scale 0.98
- Disabled → Couleur grisée

---

### Configuration de Jeu (Popup)

#### `.game-selection-container`
Zone de sélection du type de jeu dans la popup.

```css
.game-selection-container {
    padding: 16px;
    background-color: rgba(75, 140, 230, 0.1);
    border-radius: 10px;
    border-width: 1px;
    border-color: rgba(75, 140, 230, 0.3);
    margin-bottom: 16px;
}
```

**Sous-classes** :
- `.popup-config-title` - Titre "🎯 Game Configuration"
- `.popup-game-dropdown` - Dropdown de sélection

**Exemple** :
```xml
<ui:VisualElement class="game-selection-container">
    <ui:Label text="🎯 Game Configuration" class="popup-config-title"/>
    <ui:DropdownField label="Game Type:" class="popup-game-dropdown"/>
</ui:VisualElement>
```

**Couleur** : Fond bleu clair (thème configuration)

---

#### `.ready-status-container`
Zone d'affichage du statut des joueurs prêts.

```css
.ready-status-container {
    padding: 16px;
    background-color: rgba(75, 190, 115, 0.08);
    border-radius: 10px;
    border-width: 1px;
    border-color: rgba(75, 190, 115, 0.25);
}
```

**Sous-classes** :
- `.popup-ready-title` - Titre "👥 Ready Status"
- `.ready-progress-container` - Conteneur barre de progression
- `.popup-ready-count` - Compteur "X/Y ready"
- `.ready-progress-bar` - Barre de progression fond
- `.ready-progress-fill` - Barre de progression remplie

**Exemple Complet** :
```xml
<ui:VisualElement class="ready-status-container">
    <ui:Label text="👥 Ready Status" class="popup-ready-title"/>
    <ui:VisualElement class="ready-progress-container">
        <ui:Label name="PopupReadyCount" text="2/4 ready" class="popup-ready-count"/>
        <ui:VisualElement class="ready-progress-bar">
            <ui:VisualElement class="ready-progress-fill" style="width: 50%;"/>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:VisualElement>
```

**Animation** :
```css
.ready-progress-fill {
    transition-duration: 0.3s;
}
```

**Couleurs Dynamiques** (CSS Code) :
- 0-33% : Gris `rgb(100, 100, 120)`
- 34-66% : Orange `rgb(200, 150, 50)`
- 67-100% : Vert `rgb(75, 190, 115)`

---

### Cartes Joueurs Enrichies

#### `.popup-player-item`
Carte individuelle d'un joueur dans la popup.

```css
.popup-player-item {
    flex-direction: row;
    align-items: center;
    padding: 14px 16px;
    margin: 5px 0;
    background-color: rgb(50, 55, 65);
    border-radius: 10px;
    border-width: 1px;
    border-color: rgb(65, 70, 85);
}
```

**Sous-classes** :
- `.popup-player-name` - Nom du joueur
- `.popup-player-badge` - Badge (hôte/prêt)
- `.popup-player-host` - Badge hôte (👑)
- `.popup-player-ready` - Badge prêt (✓)

**Structure Type** :
```
┌─────────────────────────────────────────┐
│ 👤 PlayerName (You)  👑 Host  ✓ Ready  │
└─────────────────────────────────────────┘
```

**Code C# (CreatePlayerCard)** :
```csharp
var card = new VisualElement();
card.AddToClassList("popup-player-item");

// Icône + Nom
var nameLabel = new Label(isReady ? "✓ " : "○ " + playerName);
nameLabel.AddToClassList("popup-player-name");
card.Add(nameLabel);

// Badge Hôte
if (isHost) {
    var hostBadge = new Label("👑 Host");
    hostBadge.AddToClassList("popup-player-badge");
    hostBadge.AddToClassList("popup-player-host");
    card.Add(hostBadge);
}

// Badge Prêt
if (isReady) {
    var readyBadge = new Label("✓ Ready");
    readyBadge.AddToClassList("popup-player-badge");
    readyBadge.AddToClassList("popup-player-ready");
    card.Add(readyBadge);
}
```

**Couleurs** :
- Badge Hôte : Jaune `rgb(255, 200, 75)`
- Badge Prêt : Vert `rgb(100, 210, 140)`
- Joueur Local : Bordure bleue (ajoutée dynamiquement)

---

### Boutons d'Action

#### `.popup-actions`
Conteneur des boutons Ready et Start Game.

```css
.popup-actions {
    flex-direction: row;
    justify-content: space-between;
    margin-bottom: 16px;
    padding: 16px;
    background-color: rgba(60, 60, 75, 0.5);
    border-radius: 10px;
    border-width: 1px;
    border-color: rgb(75, 75, 90);
}
```

**Sous-classes** :
- `.popup-ready-btn` - Bouton Ready (vert)
- `.popup-ready-btn.unready` - Variant Unready (orange)
- `.popup-start-btn` - Bouton Start Game (violet)

**Effets** :
```css
.popup-ready-btn:hover {
    background-color: rgb(90, 205, 130);
    scale: 1.02;
}
```

**États** :
1. **Ready** → Background vert, texte "Ready"
2. **Unready** → Background orange, texte "Unready"
3. **Start Disabled** → Background gris, non cliquable
4. **Start Enabled** → Background violet, avec glow (`.ready-glow`)

**Animation Pulse** :
```css
.popup-start-btn.ready-glow {
    box-shadow: 0 0 20px rgba(140, 90, 205, 0.5);
    animation-duration: 2s;
}

.pulse-animation {
    animation-name: pulse;
    animation-duration: 1.5s;
    animation-iteration-count: infinite;
}
```

---

### Flow Guide (Nouveau)

#### `.flow-step-container`
Conteneur pour les étapes du guide de flow.

```css
.flow-step-container {
    padding: 12px 16px;
    background-color: rgba(100, 130, 180, 0.12);
    border-radius: 8px;
    border-left-width: 4px;
    border-left-color: rgb(100, 130, 180);
    margin-bottom: 16px;
}
```

**Sous-classes** :
- `.flow-step-number` - Numéro de l'étape
- `.flow-step-title` - Titre de l'étape
- `.flow-step-description` - Description détaillée

**Exemple** :
```xml
<ui:VisualElement class="flow-step-container">
    <ui:Label text="1" class="flow-step-number"/>
    <ui:Label text="Create Session" class="flow-step-title"/>
    <ui:Label text="Enter a name and click Create" class="flow-step-description"/>
</ui:VisualElement>
```

---

#### `.action-required`
Indicateur d'action requise (orange).

```css
.action-required {
    padding: 10px 14px;
    background-color: rgba(255, 200, 75, 0.15);
    border-radius: 8px;
    border-left-width: 3px;
    border-left-color: rgb(255, 200, 75);
    margin-top: 12px;
}
```

**Usage** :
```xml
<ui:VisualElement class="action-required">
    <ui:Label text="⚠️ Waiting for all players to be ready" class="action-required-text"/>
</ui:VisualElement>
```

---

#### `.flow-success`
Indicateur de succès (vert).

```css
.flow-success {
    padding: 10px 14px;
    background-color: rgba(75, 190, 115, 0.15);
    border-radius: 8px;
    border-left-width: 3px;
    border-left-color: rgb(75, 190, 115);
    margin-top: 12px;
}
```

**Usage** :
```xml
<ui:VisualElement class="flow-success">
    <ui:Label text="✅ All players are ready!" class="flow-success-text"/>
</ui:VisualElement>
```

---

#### `.flow-waiting`
Indicateur d'attente (violet).

```css
.flow-waiting {
    padding: 10px 14px;
    background-color: rgba(140, 90, 205, 0.15);
    border-radius: 8px;
    border-left-width: 3px;
    border-left-color: rgb(140, 90, 205);
    margin-top: 12px;
}
```

**Usage** :
```xml
<ui:VisualElement class="flow-waiting">
    <ui:Label text="⏳ Starting game..." class="flow-waiting-text"/>
</ui:VisualElement>
```

---

## 🎨 Palette de Couleurs

### Couleurs Principales
| Couleur | RGB | Usage |
|---------|-----|-------|
| **Fond Principal** | `rgb(25, 25, 30)` | Background lobby |
| **Fond Panel** | `rgb(38, 38, 45)` | Background panneaux |
| **Fond Sombre** | `rgb(30, 30, 38)` | ScrollViews, inputs |
| **Bordure** | `rgb(60, 60, 75)` | Bordures par défaut |
| **Texte Principal** | `rgb(255, 255, 255)` | Texte blanc |
| **Texte Secondaire** | `rgb(180, 180, 190)` | Labels, hints |
| **Texte Disabled** | `rgb(100, 100, 110)` | Éléments désactivés |

### Couleurs Thématiques
| Thème | RGB | Usage |
|-------|-----|-------|
| **Bleu (Primaire)** | `rgb(75, 140, 230)` | Boutons principaux, création |
| **Vert (Succès)** | `rgb(75, 190, 115)` | Ready, validation, succès |
| **Orange (Attention)** | `rgb(200, 150, 50)` | Unready, warnings |
| **Violet (Action)** | `rgb(140, 90, 205)` | Start game, actions importantes |
| **Rouge (Erreur)** | `rgb(180, 70, 70)` | Leave, disconnect, erreurs |
| **Jaune (Hôte)** | `rgb(255, 200, 75)` | Badge hôte, warnings |

---

## 🔧 Utilisation avec C#

### Ajouter une Classe
```csharp
element.AddToClassList("class-name");
```

### Retirer une Classe
```csharp
element.RemoveFromClassList("class-name");
```

### Toggle une Classe
```csharp
element.ToggleInClassList("class-name");
```

### Vérifier une Classe
```csharp
bool hasClass = element.ClassListContains("class-name");
```

### Modifier Inline (Overrides CSS)
```csharp
element.style.width = 200; // pixels
element.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
element.style.display = DisplayStyle.None;
```

---

## 📦 Classes Utilitaires

### Affichage
```css
.hidden { display: none; }
.visible { display: flex; }
```

### Animations
```css
.pulse-animation         /* Pulse infini */
.animating              /* Transition en cours */
.ready-glow             /* Effet glow */
```

### États
```css
.valid                  /* État valide */
.invalid                /* État invalide */
.selected               /* Élément sélectionné */
.hosting                /* Session hébergée */
.ready-to-start         /* Prêt à démarrer */
```

---

## 🎯 Exemples d'Usage Complets

### Création de Session avec Validation
```xml
<ui:VisualElement class="create-session">
    <ui:Label text="📝 Create New Session" class="create-session-title"/>
    <ui:VisualElement class="session-form">
        <ui:TextField name="SessionName" class="session-name-field valid"/>
        <ui:VisualElement class="creation-hint">
            <ui:Label text="💡 Good name!" class="hint-text"/>
        </ui:VisualElement>
    </ui:VisualElement>
    <ui:Button text="+ Create Session" class="create-btn"/>
</ui:VisualElement>
```

### Popup de Session Complète
```xml
<ui:VisualElement class="popup-overlay">
    <ui:VisualElement class="session-popup">
        <!-- Header -->
        <ui:VisualElement class="popup-header">
            <ui:Label text="My Awesome Game" class="popup-title"/>
            <ui:Button text="Back" class="popup-close-btn"/>
        </ui:VisualElement>
        
        <!-- Configuration -->
        <ui:VisualElement class="popup-info">
            <ui:VisualElement class="game-selection-container">
                <ui:Label text="🎯 Game Configuration" class="popup-config-title"/>
                <ui:DropdownField label="Game Type:" class="popup-game-dropdown"/>
            </ui:VisualElement>
            
            <!-- Ready Status -->
            <ui:VisualElement class="ready-status-container">
                <ui:Label text="👥 Ready Status" class="popup-ready-title"/>
                <ui:VisualElement class="ready-progress-container">
                    <ui:Label text="3/4 ready" class="popup-ready-count"/>
                    <ui:VisualElement class="ready-progress-bar">
                        <ui:VisualElement class="ready-progress-fill" style="width: 75%;"/>
                    </ui:VisualElement>
                </ui:VisualElement>
            </ui:VisualElement>
        </ui:VisualElement>
        
        <!-- Players -->
        <ui:Label text="Players in Lobby" class="popup-section-title"/>
        <ui:ScrollView class="popup-players-scroll">
            <ui:VisualElement class="popup-players-list">
                <!-- Player cards go here -->
            </ui:VisualElement>
        </ui:ScrollView>
        
        <!-- Actions -->
        <ui:VisualElement class="popup-actions">
            <ui:Button text="Ready" class="popup-ready-btn"/>
            <ui:Button text="Start Game" class="popup-start-btn"/>
        </ui:VisualElement>
        
        <ui:Button text="Leave Session" class="popup-leave-btn"/>
    </ui:VisualElement>
</ui:VisualElement>
```

---

## 📖 Références

**Fichiers Sources** :
- `Assets/UI Toolkit/SessionLobby.uss` - Styles principaux
- `Assets/UI Toolkit/SessionLobby_FlowGuide.uss` - Styles flow guide
- `Assets/UI Toolkit/SessionLobby.uxml` - Structure UI

**Documentation Associée** :
- [UI_GAME_CREATION_FLOW.md](UI_GAME_CREATION_FLOW.md) - Flow complet
- [UI_IMPROVEMENTS_COMPLETE.md](UI_IMPROVEMENTS_COMPLETE.md) - Historique améliorations

---

**Dernière mise à jour** : 7 janvier 2026  
**Version** : 3.0
