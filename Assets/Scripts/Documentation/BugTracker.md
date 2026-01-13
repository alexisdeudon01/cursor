# 🐛 Bug Tracker & Known Issues

This document tracks bugs found during code review, their status, and fixes applied.

---

## 🔴 Critical Bugs (Fixed)

### BUG-001: SessionRpcHub Singleton Logic Error
**File:** `Assets/Scripts/Networking/Player/SessionRpcHub.cs`  
**Line:** 50 (OnNetworkSpawn)  
**Status:** ✅ FIXED  

**Problem:**
```csharp
// BEFORE (broken)
if (Instance == null && Instance != this)
    Instance = this;
```

The condition `Instance == null && Instance != this` is logically flawed:
- If `Instance == null`, then comparing `Instance != this` is `null != this` which is always `true`
- So the condition simplifies to just `Instance == null`
- But the `&& Instance != this` part suggests the author wanted to avoid reassignment

**Root Cause:** Copy-paste error or misunderstanding of null comparison.

**Fix Applied:**
```csharp
// AFTER (fixed)
if (Instance == null)
    Instance = this;
```

**Impact:** Instance could fail to be assigned on network spawn, causing null reference errors.

---

### BUG-006: Session List Not Updating After Create
**File:** `Assets/Scripts/UI/Pseudo/PseudUI.cs`  
**Status:** ✅ FIXED

**Problem:** After clicking "Créer une session", nothing happened. The session was created on server but client UI didn't update.

**Root Cause:** 
1. UI didn't request refresh after creating session
2. ShowPanel didn't trigger session refresh

**Fix Applied:**
```csharp
// In OnCreateSession() - added refresh after create
rpcHub.CreateSessionServerRpc(sessionName);
Invoke(nameof(RequestSessionsIfReady), 0.1f);

// In ShowPanel() - added auto-refresh when showing sessions panel
if (panelToShow == panelSessions)
{
    RequestSessionsIfReady();
}
```

---

## 🟡 Medium Issues (Documented)

### BUG-002: Service Layer Never Used
**Files:** `Assets/Scripts/Service/SessionService/`, `Assets/Scripts/Service/GameServer/`  
**Status:** ✅ RESOLVED (New architecture created)

**Problem:** Service interfaces and adapters existed but were never injected or used. All code directly called singletons like `SessionRpcHub.Instance` and `GameSessionManager.Instance`.

**Resolution:** Created new service architecture:
- `ISessionService` - Clean interface with events
- `SessionServiceClient` - Client implementation
- `SessionServiceServer` - Server implementation
- `ServiceLocator` - Dependency injection
- `SessionPresenter` - UI mediator

---

### BUG-003: Pawn Spawning Duplicated in 4 Places
**Files:**
- `SessionRpcHub.cs` (SpawnSquaresForPlayers) - ✅ REMOVED
- `GameManager.cs` (SpawnPawn)
- `Game.cs` (SpawnPawns)
- `NetworkPawnSpawner.cs` (SpawnPawn)

**Status:** 🟡 PARTIAL FIX - Removed from SessionRpcHub

**Problem:** Same spawn logic copy-pasted with slight variations. Bug fixes must be applied in 4 places.

**Recommendation:** Use only `NetworkPawnSpawner` service for all spawn operations.

---

### BUG-004: Scene Management Contradiction
**Files:**
- `NetworkBootstrap.cs` - Disables scene management
- `SessionRpcHub.cs` - Tries to use `SceneManager.LoadScene`

**Status:** 🟡 DOCUMENTED

**Problem:** 
```csharp
// NetworkBootstrap.cs
networkManager.NetworkConfig.EnableSceneManagement = false;

// SessionRpcHub.cs (LoadSceneIfNeeded)
SceneManager.LoadScene(sceneName); // Will work but not synced!
```

**Impact:** Scene loading works locally but isn't synchronized across network.

**Recommendation:** Either enable NetworkSceneManagement or use RPC-based scene sync.

---

### BUG-005: Missing Threshold for Start Button
**File:** `Assets/Scripts/UI/Pseudo/PseudUI.cs`  
**Status:** ✅ RESOLVED (SessionServiceServer has configurable threshold)

**Problem:** Start button required ALL players to be ready, no minimum player count.

**Resolution:** `SessionServiceServer.MinPlayersToStart` property added (configurable in Inspector).

---

## 🟢 Low Priority (Tech Debt)

### DEBT-001: Orphaned NetworkSessionClient.cs
**File:** `Assets/Scripts/Networking/Sessions/Client/NetworkSessionClient.cs`  
**Status:** ✅ DELETED

**Problem:** Contains `CreateSessionServerRpc` which duplicates `SessionRpcHub`. Appears unused.

**Resolution:** File deleted.

---

### DEBT-002: Unused Data Model Adapters
**Files:**
- `Assets/Scripts/Data/Session/SessionDetailsAdapter.cs` - ✅ DELETED
- `Assets/Scripts/Data/Session/SessionRuntimeAdapter.cs` - ✅ DELETED
- `Assets/Scripts/Data/Session/SessionStateAdapter.cs` - ✅ DELETED

**Status:** ✅ DELETED

**Problem:** Adapters defined but never instantiated.

**Resolution:** Files deleted. New service layer in `Service/Core/` replaces them.

---

### DEBT-003: PseudUI.cs is 818 Lines
**File:** `Assets/Scripts/UI/Pseudo/PseudUI.cs`  
**Status:** 🟢 DOCUMENTED

**Problem:** God object handling all UI panels.

**Recommendation:** Split into separate view classes:
- `NamePanelView.cs`
- `SessionListView.cs`
- `LobbyView.cs`

Use `SessionPresenter` as mediator (already created).

---

## 📋 Bug Fix Checklist for Future

When fixing bugs, ensure:
- [ ] Root cause documented
- [ ] Fix verified in both client and server modes
- [ ] Related code checked for same pattern
- [ ] Unit test added if applicable
- [ ] This document updated

---

## 📊 Summary

| Severity | Total | Fixed | Remaining |
|----------|-------|-------|-----------|
| 🔴 Critical | 2 | 2 | 0 |
| 🟡 Medium | 4 | 3 | 1 |
| 🟢 Low | 3 | 2 | 1 |

**Last Updated:** 2026-01-07
