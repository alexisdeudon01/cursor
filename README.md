# TheBestClient - Auto-Evolving Game

Jeu 2D avec serveur full authoritative, auto-amélioré par EvoAgentX.

## Architecture

### UN SEUL BUILD

- **Serveur** (`--server`): Charge `ServerScene`, logique uniquement, headless possible
- **Client** (`--client`): Charge `MainMenu`, full graphique UI Toolkit

### UI Toolkit

Le client utilise **exclusivement** UI Toolkit :
- `.uxml` pour la structure
- `.uss` pour les styles
- Pas de Canvas legacy

## Usage

```bash
# Serveur (headless)
./TheBestGame --server --port 7777

# Client (graphique)
./TheBestGame --client --ip 192.168.1.1 --port 7777
```

## Structure

```
Assets/
├── Scenes/
│   ├── ServerScene.unity    ← Serveur uniquement
│   ├── MainMenu.unity       ← Client: entrée
│   ├── Lobby.unity          ← Client: lobbies
│   └── Game.unity           ← Client: jeu
├── UI/
│   ├── Styles/*.uss         ← Styles UI Toolkit
│   ├── Templates/*.uxml     ← Layouts UI Toolkit
│   └── Scripts/*.cs         ← Controllers UI
└── Scripts/
    ├── Network/             ← Réseau
    └── Game/                ← Logique jeu
```

## Auto-Amélioration

Le projet s'améliore automatiquement toutes les heures via GitHub Actions + EvoAgentX.
Rollback automatique si régression détectée.

## Voir les runs

https://github.com/alexisdeudon01/cursor/actions
