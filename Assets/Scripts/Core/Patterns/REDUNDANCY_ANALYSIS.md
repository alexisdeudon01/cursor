# Code Redundancy Analysis & Refactoring Opportunities

## 🔍 Analysis Date
January 2026

## 📊 Summary

### Identified Redundant Patterns

| Pattern | Occurrences | Lines | Priority | Refactoring Class |
|---------|-------------|-------|----------|-------------------|
| **Singleton Pattern** | 14+ | ~140 | ⚠️ HIGH | `Singleton<T>` |
| **Factory Pattern** | 3+ | ~50 | 🔸 MEDIUM | `RegistryFactory<K,V>` |
| **Logging** | 100+ | ~300 | ⚠️ HIGH | `NetworkLogger` |
| **Session Validation** | 8+ | ~80 | 🔸 MEDIUM | `SessionValidator` |
| **Client Resolution** | 3+ | ~30 | 🔸 MEDIUM | `NetworkPlayerResolver` |

---

## 🎯 Pattern 1: Singleton Pattern Duplication

### Current State

**14+ classes** implement identical singleton logic:

```csharp
// Repeated in 14+ classes
public static ClassName Instance { get; private set; }

private void Awake()
{
    if (Instance != null)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### Files Affected
1. `SessionRpcHub.cs` (13)
2. `SessionLobbyUI.cs` (14)
3. `NetworkClientRegistry.cs` (13)
4. `GameDebugUI.cs` (10)
5. `UIManager.cs` (11)
6. `ClientBootstrap.cs` (14)
7. `ServerBootstrap.cs` (13)
8. `GameSessionManager.cs` (10)
9. `PlayerManager.cs` (9)
10. `NetworkBootstrap.cs` (17)
11. `GameManager.cs` (10)
12. `SessionPawnVisibility.cs` (12)
13. `PlayerInputHandler.cs` (13)
14. `GameInstanceManager.cs` (13)
15. `ToastNotification.cs` (~40 lines with FindOrCreateInstance)

**Total Lines**: ~140 lines (9-10 lines × 14 classes)

### Refactoring Solution

**Use**: `Singleton<T>` base class from `Core.Patterns.Singleton`

**Example Migration**:
```csharp
// BEFORE (9 lines)
public class SessionRpcHub : NetworkBehaviour
{
    public static SessionRpcHub Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

// AFTER (0 lines boilerplate)
using Core.Patterns;

public class SessionRpcHub : Singleton<SessionRpcHub>
{
    protected override void OnInitialize()
    {
        // Custom initialization (if needed)
    }
}
```

### Benefits
- ✅ **140 lines removed** from codebase
- ✅ **Thread-safe** singleton implementation
- ✅ **Consistent pattern** across all managers
- ✅ **OnInitialize/OnCleanup hooks** for lifecycle management
- ✅ **HasInstance check** to avoid null references

### Effort
- **Time**: 2-3 hours for all 14 classes
- **Risk**: ⚠️ LOW (well-tested pattern)
- **Testing**: Verify each singleton still works after migration

---

## 🎯 Pattern 2: Factory Pattern Redundancy

### Current State

Multiple places implement similar registry/factory logic:

#### GameRegistry.cs (Good Implementation)
```csharp
private static readonly Dictionary<string, IGameDefinition> games = ...;

public static bool Register(IGameDefinition game)
{
    if (game == null) return false;
    if (games.ContainsKey(game.GameId)) return false;
    games[game.GameId] = game;
    return true;
}

public static IGameDefinition GetGame(string gameId)
{
    games.TryGetValue(gameId, out var game);
    return game;
}
```

#### Similar Pattern in NetworkPrefabs (if used)
- Could benefit from `RegistryFactory<string, NetworkObject>`

### Refactoring Solution

**Option 1**: Keep as-is (GameRegistry is well-implemented)

**Option 2**: Extract to `RegistryFactory<string, IGameDefinition>`:
```csharp
public static class GameRegistry
{
    private static readonly RegistryFactory<string, IGameDefinition> _factory = 
        new RegistryFactory<string, IGameDefinition>(allowOverwrite: false);
        
    public static bool Register(IGameDefinition game) => 
        _factory.Register(game.GameId, () => game);
        
    public static IGameDefinition GetGame(string gameId) => 
        _factory.IsRegistered(gameId) ? _factory.Create(gameId) : null;
}
```

### Benefits
- ✅ **Consistent factory pattern** across systems
- ✅ **Reusable for other registries** (UI, Prefabs, etc.)
- ⚠️ **May be overkill** for GameRegistry (already clean)

### Effort
- **Time**: 1 hour per registry
- **Risk**: ⚠️ LOW-MEDIUM
- **Testing**: Ensure registration/retrieval still works

---

## 🎯 Pattern 3: Logging Redundancy

### Current State

**100+ occurrences** of manual logging:

```csharp
// Pattern 1: Handler logging (PlayerMovementHandler, etc.)
Debug.Log($"[PlayerMovementHandler] Message");

// Pattern 2: Session logging (GameSessionManager, etc.)
Debug.Log($"[GameSessionManager] Message");
ServerBootstrap.LogSession("event", sessionName, clientId);

// Pattern 3: Validation logging (BaseValidator, etc.)
Debug.LogWarning($"[{GetType().Name}] Validation failed");
```

### Issues
- ❌ **Inconsistent format**: Some use `[Class]`, some use `[Component:ID]`
- ❌ **No log levels**: Everything is `Debug.Log` or `Debug.LogWarning`
- ❌ **No filtering**: Can't disable specific subsystems
- ❌ **No file logging**: Only console output
- ❌ **Repeated string interpolation**: Performance impact

### Refactoring Solution

**Create**: `NetworkLogger` utility class

```csharp
namespace Core.Utilities
{
    public enum LogLevel { Debug, Info, Warning, Error }
    
    public static class NetworkLogger
    {
        private static readonly Dictionary<string, bool> _enabledSubsystems = new();
        
        public static void Log(string subsystem, string message, LogLevel level = LogLevel.Info)
        {
            if (!IsEnabled(subsystem)) return;
            
            string prefix = $"[{subsystem}]";
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log($"{prefix} {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{prefix} {message}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"{prefix} {message}");
                    break;
            }
        }
        
        public static void LogSession(string subsystem, string sessionId, string message)
        {
            Log($"{subsystem}:{sessionId}", message);
        }
        
        public static void EnableSubsystem(string subsystem) { }
        public static void DisableSubsystem(string subsystem) { }
        public static bool IsEnabled(string subsystem) => true; // Default
    }
}
```

**Usage**:
```csharp
// BEFORE
Debug.Log($"[PlayerMovementHandler] Client {clientId} moved");

// AFTER
NetworkLogger.Log("PlayerMovement", $"Client {clientId} moved");
```

### Benefits
- ✅ **Consistent logging format**
- ✅ **Filterable by subsystem**
- ✅ **Log levels** (Debug, Info, Warning, Error)
- ✅ **Easy to extend** (file logging, remote logging, etc.)
- ✅ **Performance**: Can disable expensive logging in production

### Effort
- **Time**: 4-6 hours to migrate all logging
- **Risk**: ⚠️ MEDIUM (need to ensure all logs still appear)
- **Testing**: Verify logs in development and production builds

---

## 🎯 Pattern 4: Session Validation Redundancy

### Current State

**8+ occurrences** of similar validation logic:

```csharp
// Pattern 1: In SessionRpcHub handlers
if (GameSessionManager.Instance == null)
{
    Debug.LogError("GameSessionManager not available");
    return;
}

var session = GameSessionManager.Instance.GetSecureContainer(sessionName);
if (session == null)
{
    Debug.LogWarning($"Session '{sessionName}' not found");
    return;
}

if (!session.ValidateAccess(clientId, "Operation"))
{
    return;
}
```

### Files Affected
- `SessionLifecycleHandler.cs`
- `GameStartHandler.cs`
- `PlayerMovementHandler.cs`
- `SceneLoadHandler.cs`
- `SessionQueryHandler.cs`
- `BaseValidator.cs` (already has some helpers)

### Refactoring Solution

**Enhance**: `BaseValidator` or create `SessionValidator`

```csharp
public static class SessionValidator
{
    public static (bool isValid, SessionContainer session) ValidateSessionAccess(
        string sessionName, 
        ulong clientId, 
        string operation)
    {
        // Check GameSessionManager exists
        if (GameSessionManager.Instance == null)
        {
            Debug.LogError($"[SessionValidator] GameSessionManager not available for '{operation}'");
            return (false, null);
        }
        
        // Get session
        var session = GameSessionManager.Instance.GetSecureContainer(sessionName);
        if (session == null)
        {
            Debug.LogWarning($"[SessionValidator] Session '{sessionName}' not found for '{operation}'");
            return (false, null);
        }
        
        // Validate access
        if (!session.ValidateAccess(clientId, operation))
        {
            Debug.LogWarning($"[SessionValidator] Client {clientId} unauthorized for '{operation}' in '{sessionName}'");
            return (false, null);
        }
        
        return (true, session);
    }
}
```

**Usage**:
```csharp
// BEFORE (8-10 lines)
if (GameSessionManager.Instance == null) return;
var session = GameSessionManager.Instance.GetSecureContainer(sessionName);
if (session == null) return;
if (!session.ValidateAccess(clientId, "Move")) return;
// ... operation

// AFTER (2 lines)
var (isValid, session) = SessionValidator.ValidateSessionAccess(sessionName, clientId, "Move");
if (!isValid) return;
// ... operation
```

### Benefits
- ✅ **80 lines removed** (10 lines × 8 occurrences)
- ✅ **Consistent validation** across handlers
- ✅ **Centralized error messages**
- ✅ **Easy to extend** (e.g., add permission checks)

### Effort
- **Time**: 2-3 hours
- **Risk**: ⚠️ LOW
- **Testing**: Verify all validation still works

---

## 🎯 Pattern 5: Client/Player Resolution

### Current State

**3+ occurrences** of `ResolvePlayerName` logic:

```csharp
// In GameStartHandler.cs
private string ResolvePlayerName(ulong clientId)
{
    if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
    {
        var spawnManager = NetworkManager.Singleton.SpawnManager;
        if (spawnManager.SpawnedObjects.TryGetValue(clientId, out var playerObj))
        {
            var playerScript = playerObj.GetComponent<DefaultPlayer>();
            if (playerScript != null)
                return playerScript.PlayerName.Value.ToString();
        }
    }
    return $"Player{clientId}";
}
```

**Similar logic** in:
- `GameStartHandler.cs`
- `GameInstanceManager.cs`
- Other handlers that need player info

### Refactoring Solution

**Create**: `NetworkPlayerResolver` utility class

```csharp
public static class NetworkPlayerResolver
{
    public static string GetPlayerName(ulong clientId)
    {
        var player = GetPlayerComponent<DefaultPlayer>(clientId);
        return player != null ? player.PlayerName.Value.ToString() : $"Player{clientId}";
    }
    
    public static T GetPlayerComponent<T>(ulong clientId) where T : Component
    {
        if (NetworkManager.Singleton?.SpawnManager == null)
            return null;
            
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientId, out var playerObj))
        {
            return playerObj.GetComponent<T>();
        }
        
        return null;
    }
    
    public static NetworkObject GetPlayerNetworkObject(ulong clientId)
    {
        return NetworkManager.Singleton?.SpawnManager?.SpawnedObjects.TryGetValue(clientId, out var obj) == true 
            ? obj 
            : null;
    }
}
```

**Usage**:
```csharp
// BEFORE (10 lines)
string playerName;
if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
{
    var spawnManager = NetworkManager.Singleton.SpawnManager;
    if (spawnManager.SpawnedObjects.TryGetValue(clientId, out var playerObj))
    {
        var playerScript = playerObj.GetComponent<DefaultPlayer>();
        if (playerScript != null)
            playerName = playerScript.PlayerName.Value.ToString();
    }
}
playerName = $"Player{clientId}";

// AFTER (1 line)
string playerName = NetworkPlayerResolver.GetPlayerName(clientId);
```

### Benefits
- ✅ **30 lines removed** (10 lines × 3 occurrences)
- ✅ **Null-safe** player resolution
- ✅ **Reusable** across systems
- ✅ **Generic component access** (`GetPlayerComponent<T>`)

### Effort
- **Time**: 1-2 hours
- **Risk**: ⚠️ LOW
- **Testing**: Verify player name resolution

---

## 📋 Refactoring Priority Matrix

### Phase 1: Quick Wins (Low Risk, High Impact)
1. ✅ **Singleton Pattern** - Migrate all 14 classes
   - **Impact**: 140 lines removed
   - **Time**: 2-3 hours
   - **Risk**: LOW

2. ✅ **NetworkPlayerResolver** - Create utility class
   - **Impact**: 30 lines removed
   - **Time**: 1-2 hours
   - **Risk**: LOW

3. ✅ **SessionValidator** - Consolidate validation logic
   - **Impact**: 80 lines removed
   - **Time**: 2-3 hours
   - **Risk**: LOW

**Total Phase 1**: 250 lines removed, 5-8 hours

### Phase 2: Medium Effort (Medium Risk, Medium Impact)
4. 🔸 **NetworkLogger** - Centralize logging
   - **Impact**: Consistent logging, filterable
   - **Time**: 4-6 hours
   - **Risk**: MEDIUM

5. 🔸 **RegistryFactory** - Refactor GameRegistry (optional)
   - **Impact**: Pattern consistency
   - **Time**: 1-2 hours
   - **Risk**: LOW-MEDIUM

**Total Phase 2**: 5-8 hours

### Phase 3: Long-term (High Effort, High Impact)
6. 🔹 **Command Pattern** - Extend usage beyond movement
   - **Impact**: Undo/redo for all actions
   - **Time**: 10-15 hours
   - **Risk**: MEDIUM-HIGH

7. 🔹 **Observer Pattern** - Replace direct event calls with MessageBus
   - **Impact**: Decoupled systems
   - **Time**: 8-12 hours
   - **Risk**: MEDIUM

8. 🔹 **State Machine** - Migrate to EnhancedStateMachine
   - **Impact**: Better state management
   - **Time**: 5-8 hours per system
   - **Risk**: MEDIUM

**Total Phase 3**: 23-35 hours

---

## 🎯 Total Impact

### Code Reduction
- **Phase 1**: ~250 lines removed
- **Phase 2**: ~100 lines removed (indirect)
- **Phase 3**: ~300-500 lines removed (indirect)

**Total**: **~650-850 lines** cleaner codebase

### Maintainability Improvement
- ✅ **Consistent patterns** across all systems
- ✅ **Reduced duplication** = fewer bugs
- ✅ **Easier onboarding** for new developers
- ✅ **Better testability** with base classes
- ✅ **Clearer architecture** with documented patterns

---

## 📝 Refactoring Checklist

### For Each Pattern Migration

1. **Preparation**
   - [ ] Read pattern documentation
   - [ ] Identify all occurrences
   - [ ] Create backup branch

2. **Implementation**
   - [ ] Create/import base class
   - [ ] Migrate one file as test
   - [ ] Verify functionality
   - [ ] Migrate remaining files
   - [ ] Run all tests

3. **Validation**
   - [ ] Code compiles
   - [ ] All tests pass
   - [ ] Manual testing in editor
   - [ ] Performance check (no regression)

4. **Documentation**
   - [ ] Update code comments
   - [ ] Update architecture docs
   - [ ] Add to CHANGELOG.md

---

## 🚀 Getting Started

### Recommended Order

1. **Start with Singleton Pattern** (easiest, safest)
   - Pick one class (e.g., `ToastNotification`)
   - Migrate to `Singleton<T>`
   - Test thoroughly
   - Apply to remaining 13 classes

2. **Then NetworkPlayerResolver** (small utility)
   - Create the class
   - Migrate one usage
   - Test
   - Replace remaining usages

3. **Then SessionValidator** (medium complexity)
   - Create validator class
   - Migrate one handler
   - Test
   - Replace in all handlers

4. **Finally NetworkLogger** (largest impact)
   - Create logger class
   - Enable for one subsystem
   - Test filtering/performance
   - Migrate all logging

---

## ⚠️ Important Notes

### Testing Strategy
- **Unit tests**: For all utility classes
- **Integration tests**: For migrated singletons
- **Manual tests**: Run game in editor
- **Performance tests**: Profile before/after

### Rollback Plan
- Keep backup branch
- Commit each pattern separately
- Document breaking changes
- Easy to revert individual migrations

### Team Communication
- Announce refactoring in team chat
- Document in pull requests
- Pair review for critical systems
- Update onboarding docs

---

**Analysis Completed**: January 2026  
**Priority**: Phase 1 (Quick Wins) recommended  
**Estimated Total Effort**: 5-8 hours for Phase 1, 33-43 hours for all phases
