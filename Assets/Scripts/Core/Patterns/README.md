# Design Patterns - Core Framework

Comprehensive base classes and interfaces for common design patterns used throughout the codebase.

## 📂 Location
`Assets/Scripts/Core/Patterns/`

## 📚 Available Patterns

### 1. Singleton Pattern (`Singleton.cs`)

Three singleton variants for different use cases:

#### `Singleton<T>` - Persistent Singleton
- **Usage**: Global managers that survive scene loads
- **Features**: Thread-safe, DontDestroyOnLoad, auto-instantiation
- **Example**: NetworkManager, GameSessionManager, UIManager

```csharp
public class MyManager : Singleton<MyManager>
{
    protected override void OnInitialize()
    {
        // Called once on creation
    }
    
    protected override void OnCleanup()
    {
        // Called on destruction
    }
}

// Usage
MyManager.Instance.DoSomething();
if (MyManager.HasInstance) { }
```

#### `PersistentSingleton<T>` - Explicitly Persistent
- Same as Singleton but enforces DontDestroyOnLoad

#### `SceneSingleton<T>` - Scene-Specific Singleton
- **Usage**: Scene-level managers that should reset on scene load
- **Example**: LevelManager, SceneUI

```csharp
public class LevelManager : SceneSingleton<LevelManager>
{
    // Automatically destroyed on scene change
}
```

---

### 2. Factory Pattern (`Factory.cs`)

Multiple factory implementations for object creation:

#### `IFactory<T>` - Simple Factory Interface
```csharp
public class PlayerFactory : Factory<Player>
{
    public override Player Create()
    {
        var player = new Player();
        OnCreated(player); // Optional hook
        return player;
    }
}
```

#### `RegistryFactory<TKey, TProduct>` - Registry-Based Factory
- **Usage**: Game objects, UI elements, network prefabs
- **Features**: Register creators by ID, type-safe

```csharp
var factory = new RegistryFactory<string, IGameDefinition>();

// Register
factory.Register("circleGame", () => new CircleGameDefinition());
factory.Register("squareGame", () => new SquareGameDefinition());

// Create
var game = factory.Create("circleGame");
```

**Current Usage**:
- GameRegistry uses this pattern internally
- Could be used for NetworkPrefabs management
- UI popup factory

#### `PoolFactory<T>` - Object Pooling
- **Usage**: Reduce GC pressure for frequently created/destroyed objects
- **Features**: Max size, get/release callbacks, prewarm

```csharp
var bulletPool = new PoolFactory<Bullet>(
    creator: () => Instantiate(bulletPrefab),
    onGet: bullet => bullet.gameObject.SetActive(true),
    onRelease: bullet => bullet.gameObject.SetActive(false),
    maxSize: 100
);

// Prewarm pool
bulletPool.Prewarm(20);

// Get from pool
var bullet = bulletPool.Get();

// Return to pool
bulletPool.Release(bullet);
```

#### `LazyFactory<T>` - Lazy Initialization
- Creates instance only when first accessed

```csharp
var lazyDb = new LazyFactory<Database>(() => new Database());
var db = lazyDb.Get(); // Created here
```

---

### 3. Command Pattern (`Command.cs`)

Encapsulate actions with undo/redo support:

#### `ICommand` - Base Interface
```csharp
public class MovePlayerCommand : Command
{
    private Vector3 _oldPosition;
    private Vector3 _newPosition;
    private Transform _player;
    
    public MovePlayerCommand(Transform player, Vector3 newPos)
    {
        _player = player;
        _newPosition = newPos;
    }
    
    protected override void OnExecute()
    {
        _oldPosition = _player.position;
        _player.position = _newPosition;
    }
    
    protected override void OnUndo()
    {
        _player.position = _oldPosition;
    }
}
```

#### `CommandInvoker` - Command Management
- Execute commands with history
- Undo/Redo support
- Command queuing
- History size limit

```csharp
var invoker = new CommandInvoker(maxHistorySize: 100);

// Execute immediately
invoker.Execute(new MovePlayerCommand(player, newPos));

// Queue for later
invoker.Queue(new AttackCommand());
invoker.Queue(new DefendCommand());
invoker.ProcessQueue();

// Undo/Redo
if (invoker.CanUndo)
    invoker.Undo();
    
if (invoker.CanRedo)
    invoker.Redo();
```

#### `CompositeCommand` - Multiple Commands as One
```csharp
var combo = new CompositeCommand();
combo.Add(new MoveCommand());
combo.Add(new RotateCommand());
combo.Add(new ShootCommand());

invoker.Execute(combo); // Execute all
invoker.Undo(); // Undo all in reverse order
```

#### `MacroCommand` - Record and Replay
```csharp
var macro = new MacroCommand();
macro.StartRecording();
macro.Record(new JumpCommand());
macro.Record(new RunCommand());
macro.Record(new SlideCommand());
macro.StopRecording();

// Replay the macro
invoker.Execute(macro);
```

**Current Usage**:
- `IPlayerCommand` extends this pattern (see `Assets/Scripts/Core/Commands/IPlayerCommand.cs`)
- `MovePlayerCommand`, `PlayerActionCommand`
- `CommandInvoker` used in GameContainer

---

### 4. Observer Pattern (`Observer.cs`)

Decouple event producers from consumers:

#### `IObserver<T>` / `ISubject<T>` - Classic Observer
```csharp
public class HealthObserver : IObserver<int>
{
    public void OnNotify(int health)
    {
        Debug.Log($"Health changed: {health}");
    }
}

var healthSubject = new Subject<int>();
healthSubject.Attach(new HealthObserver());
healthSubject.Notify(100); // Notifies all observers
```

#### `EventSubject<T>` - Event-Based Observer
```csharp
var scoreChanged = new EventSubject<int>();

// Subscribe
scoreChanged.Subscribe(score => Debug.Log($"Score: {score}"));

// Notify
scoreChanged.Notify(500);
```

#### `ObservableProperty<T>` - Property Change Notification
```csharp
var health = new ObservableProperty<int>(100);

// Subscribe to changes
health.Subscribe((oldValue, newValue) => 
    Debug.Log($"Health: {oldValue} → {newValue}")
);

// Change value (triggers notification)
health.Value = 80;
```

#### `ObservableCollection<T>` - Collection Change Notification
```csharp
var inventory = new ObservableCollection<Item>();

inventory.OnItemAdded += item => Debug.Log($"Added: {item}");
inventory.OnItemRemoved += item => Debug.Log($"Removed: {item}");

inventory.Add(new Item("Sword"));
inventory.Remove(itemToRemove);
```

#### `MessageBus` - Global Event Bus
- **Usage**: Decoupled communication between systems
- **Features**: Type-safe, singleton pattern

```csharp
// Define message types
public struct PlayerDiedMessage { public string PlayerName; }

// Subscribe
MessageBus.Instance.Subscribe<PlayerDiedMessage>(msg => 
    Debug.Log($"{msg.PlayerName} died")
);

// Publish
MessageBus.Instance.Publish(new PlayerDiedMessage { PlayerName = "Alice" });
```

**Current Usage**:
- SessionContainer events (OnPlayerJoined, OnPlayerLeft, OnStateChanged)
- UIManager.OnStateChanged
- GameRegistry.GameRegistered

---

### 5. State Machine Pattern (`StateMachine.cs`)

Manage complex state transitions:

#### `EnhancedStateMachine<TState>` - Feature-Rich State Machine
- **Features**: History, validation, allowed transitions, callbacks

```csharp
public enum GameState { Menu, Playing, Paused, GameOver }

var stateMachine = new EnhancedStateMachine<GameState>(GameState.Menu);

// Configure states
stateMachine.Configure(GameState.Menu)
    .OnEnter(() => Debug.Log("Entering Menu"))
    .OnExit(() => Debug.Log("Exiting Menu"))
    .OnUpdate(() => { /* Update menu */ })
    .AllowTransitionsTo(GameState.Playing);

stateMachine.Configure(GameState.Playing)
    .OnEnter(() => StartGame())
    .OnUpdate(() => UpdateGame())
    .AllowTransitionsTo(GameState.Paused, GameState.GameOver);

// Transition
stateMachine.TransitionTo(GameState.Playing);

// Update (call in Update())
stateMachine.Update();

// Events
stateMachine.OnStateChanged += (oldState, newState) => 
    Debug.Log($"{oldState} → {newState}");
```

#### `HierarchicalStateMachine<TState>` - Nested States
```csharp
var mainMachine = new HierarchicalStateMachine<GameState>(GameState.Playing);

// Add sub-state machine for Playing state
var combatMachine = new EnhancedStateMachine<CombatState>(CombatState.Idle);
mainMachine.AddSubStateMachine(GameState.Playing, combatMachine);
```

#### `FiniteStateMachine<TState>` - Explicit State Classes
```csharp
public class MenuState : IState
{
    public void OnEnter() { /* Setup menu */ }
    public void OnExit() { /* Cleanup menu */ }
    public void OnUpdate() { /* Update menu */ }
}

var fsm = new FiniteStateMachine<GameState>(GameState.Menu);
fsm.RegisterState(GameState.Menu, new MenuState());
fsm.RegisterState(GameState.Playing, new PlayingState());

fsm.TransitionTo(GameState.Playing);
fsm.Update();
```

**Current Usage**:
- `StateMachine<TState>` (simpler version in `Assets/Scripts/Core/StateMachine.cs`)
- SessionLobbyUI uses StateMachine<LobbyState>
- SessionContainer.SessionState enum
- UIManager.UIState

---

## 🔄 Migration Guide

### Migrating Existing Code

#### Example: GameSessionManager → Singleton
**Before**:
```csharp
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }
    
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
```

**After**:
```csharp
public class GameSessionManager : Singleton<GameSessionManager>
{
    protected override void OnInitialize()
    {
        // Initialization code
    }
}
```

#### Example: GameRegistry → RegistryFactory
**Current** (Already good pattern):
```csharp
public static class GameRegistry
{
    private static readonly Dictionary<string, IGameDefinition> games = ...;
    public static bool Register(IGameDefinition game) { }
    public static IGameDefinition GetGame(string gameId) { }
}
```

**Could be refactored to**:
```csharp
public static class GameRegistry
{
    private static readonly RegistryFactory<string, IGameDefinition> _factory = 
        new RegistryFactory<string, IGameDefinition>(allowOverwrite: false);
        
    public static bool Register(IGameDefinition game) => 
        _factory.Register(game.GameId, () => game);
        
    public static IGameDefinition GetGame(string gameId) => 
        _factory.Create(gameId); // Actually just returns registered instance
}
```

---

## 🎯 Best Practices

### When to Use Each Pattern

#### **Singleton**
✅ **Use**: Global managers (one instance per app)  
❌ **Avoid**: Scene-specific objects, data containers  
**Examples**: NetworkManager, GameSessionManager, UIManager

#### **Factory**
✅ **Use**: Complex object creation, runtime type selection  
❌ **Avoid**: Simple constructors, static data  
**Examples**: Game types, UI popups, network prefabs

#### **Command**
✅ **Use**: Actions with undo/redo, player input, networking  
❌ **Avoid**: Simple method calls, immediate actions  
**Examples**: Player actions, edit operations, macros

#### **Observer**
✅ **Use**: Event notifications, data binding, decoupled systems  
❌ **Avoid**: Direct method calls, tight coupling acceptable  
**Examples**: UI updates, game events, achievement systems

#### **State Machine**
✅ **Use**: Complex state logic, many transitions, history tracking  
❌ **Avoid**: Simple boolean flags, 2-3 states only  
**Examples**: AI behavior, UI flows, game states

---

## 🔍 Code Quality

### Patterns Follow SOLID Principles

1. **Single Responsibility**: Each pattern has one clear purpose
2. **Open/Closed**: Extend via inheritance, closed for modification
3. **Liskov Substitution**: Base classes are substitutable
4. **Interface Segregation**: Focused interfaces (IFactory, IObserver, etc.)
5. **Dependency Inversion**: Depend on abstractions (ICommand, IState, etc.)

### Testing Support

All patterns are designed for easy unit testing:
- **Singleton**: Use `HasInstance` to check before access
- **Factory**: Mock creators for tests
- **Command**: Execute/Undo in isolation
- **Observer**: Subscribe test listeners
- **StateMachine**: Validate transitions

---

## 📖 References

### Existing Implementations to Compare
- `Assets/Scripts/Core/StateMachine.cs` - Simpler state machine
- `Assets/Scripts/Core/Commands/IPlayerCommand.cs` - Command pattern for player actions
- `Assets/Scripts/Core/Games/GameRegistry.cs` - Registry pattern (factory-like)
- All singleton managers (14+ classes)

### Related Documentation
- [ARCHITECTURE_3_LEVEL.md](../../ARCHITECTURE_3_LEVEL.md) - System architecture
- [RPC_LAYER_ARCHITECTURE.md](../../RPC_LAYER_ARCHITECTURE.md) - Handler patterns
- [REFACTORING_EXECUTIVE_SUMMARY.md](../../REFACTORING_EXECUTIVE_SUMMARY.md) - Recent refactoring

---

## 🚀 Next Steps

### Recommended Refactorings

1. **Migrate all singletons** to use `Singleton<T>` base class (14 classes)
2. **Consolidate factory patterns** in GameRegistry, NetworkPrefabs
3. **Extend CommandInvoker** usage beyond PlayerMovementHandler
4. **Add MessageBus** for cross-system communication
5. **Enhance SessionContainer** with EnhancedStateMachine features

### Future Enhancements

- **AsyncCommand**: Commands with async/await support
- **TransactionCommand**: Rollback on failure
- **PooledStateMachine**: State machine pooling
- **SerializableCommand**: Network-serializable commands
- **WeakObserver**: Automatic cleanup for observers

---

## ⚠️ Important Notes

### Backwards Compatibility
- **These are NEW patterns** - existing code continues to work
- **Migration is optional** but recommended for consistency
- **No breaking changes** to existing APIs

### Performance Considerations
- **Singleton**: Minimal overhead, thread-safe locks
- **Factory**: No runtime overhead vs manual creation
- **Command**: Small allocation per command (poolable)
- **Observer**: List iteration cost (use MessageBus for many listeners)
- **StateMachine**: Dictionary lookups (very fast)

### Thread Safety
- **Singleton<T>**: Thread-safe initialization
- **RegistryFactory**: NOT thread-safe (add locks if needed)
- **CommandInvoker**: NOT thread-safe
- **Subject<T>**: NOT thread-safe (Unity is single-threaded)
- **MessageBus**: NOT thread-safe

---

**Created**: January 2026  
**Author**: AI Coding Agent  
**Version**: 1.0.0
