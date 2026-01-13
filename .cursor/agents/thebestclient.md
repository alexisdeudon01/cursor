# TheBestClient - Agent Technical Specification

> **Self-Improving Project** - Auto-improves via GitHub Actions every 15 minutes  
> **Architecture**: Full Authoritative Server (Data-Oriented Design)  
> **Source of Truth**: Unity project files (.meta, .unity, .asset)  
> **Execution**: GitHub Actions 24/7 (works with all PCs turned off)

---

## 1. Mission

Develop a full authoritative client-server 2D game using Data-Oriented Design (DOD) in Unity 6000.3.0f1.

| Principle | Description |
|-----------|-------------|
| **Server Fully Authoritative** | ALL game logic server-side. Clients are dumb terminals. |
| **Data-Oriented Design** | C# structs for DTOs, no classes for network payloads. |
| **External Game Definitions** | JSON files define games, no code changes needed. |
| **Single Executable** | `./TheBestGame --server` or `--client` |
| **Self-Improvement** | Continuous improvement every 15 minutes via CI/CD. |

---

## 2. Workflow Execution

| Item | Value |
|------|-------|
| **Executor** | GitHub Actions (cloud servers) |
| **Schedule** | Every 15 minutes |
| **Runner** | ubuntu-latest + Docker |
| **Availability** | 24/7 - All PCs can be off |
| **Build** | Unity via Docker (game-ci) |

---

## 3. Self-Improvement Loop

Every 15 minutes:
1. GitHub triggers workflow
2. Checkout code
3. Claude AI analyzes and improves code
4. Docker builds Unity project
5. Commit and push improvements
6. Repeat

---

## 4. Reading Unity Files

Agent discovers structure by READING Unity files:

| File Type | What Agent Extracts |
|-----------|---------------------|
| `.meta` | GUIDs, dependencies |
| `.unity` | Scene hierarchy, components |
| `.prefab` | Networked objects |
| `.asset` | Configuration |
| `EditorBuildSettings.asset` | Scene list |

Discovery: GUID tracing, not path assumptions.

---

## 5. Full Authoritative Server

**Server**: Game state, physics, validation, player management, JSON loading  
**Client**: Input sending, state receiving, rendering only

```csharp
NetworkManager.Singleton.StartServer();  // ✅ Dedicated
NetworkManager.Singleton.StartHost();    // ❌ Never use
```

---

## 6. Network Flow

1. Connect → 2. Name request → 3. Name response → 4. Lobby list request → 5. Lobby list → 6. Join/Create → 7. Confirm → 8. Start → 9. Load JSON → 10. Game loop

---

## 7. DTOs (Structs Only)

| DTO | Direction |
|-----|-----------|
| PlayerInputDTO | Client → Server |
| GameStateDTO | Server → Client |
| LobbyDTO | Server → Client |

---

## 8. CLI

```
./TheBestGame --server [--port 7777]
./TheBestGame --client [--ip X.X.X.X] [--port 7777]
```

---

*Auto-updated by CI/CD workflow*
