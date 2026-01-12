using UnityEngine;

/// <summary>
/// Command pattern interface for player actions.
/// Allows encapsulating player requests and executing them in game context.
/// </summary>
public interface IPlayerCommand
{
    /// <summary>
    /// Execute the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Undo the command (optional, for replay systems).
    /// </summary>
    void Undo();

    /// <summary>
    /// Client ID that issued this command.
    /// </summary>
    ulong ClientId { get; }
}

/// <summary>
/// Network-serializable command data.
/// Used to send commands from client to server.
/// </summary>
public enum CommandType : byte
{
    Move = 0,
    Jump = 1,
    Shoot = 2,
    Interact = 3
}

public struct NetworkCommandData
{
    public CommandType Type;
    public ulong ClientId;
    public Vector2 Direction;
    public float Speed;
    public string ActionType;
}

/// <summary>
/// Command for player movement.
/// </summary>
public class MovePlayerCommand : IPlayerCommand
{
    public ulong ClientId { get; private set; }
    private readonly IPlayerCommandContext context;
    private readonly Vector2 direction;
    private readonly float speed;

    public MovePlayerCommand(IPlayerCommandContext context, ulong clientId, Vector2 direction, float speed = 5f)
    {
        this.context = context;
        ClientId = clientId;
        this.direction = direction;
        this.speed = speed;
    }

    private static IServerMovablePawn FindMovablePawn(GameObject pawnObject)
    {
        if (pawnObject == null)
            return null;

        // Using GetComponents<MonoBehaviour>() avoids relying on Unity's support for interface
        // generic GetComponent calls across different Unity versions.
        var behaviours = pawnObject.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IServerMovablePawn movable)
                return movable;
        }

        return null;
    }

    public void Execute()
    {
        var pawnObject = context?.GetPlayerPawnObject(ClientId);
        if (pawnObject != null)
        {
            var movable = FindMovablePawn(pawnObject);
            if (movable != null)
                movable.Move(direction);
            else
                pawnObject.transform.position += new Vector3(direction.x, 0f, direction.y) * speed * Time.deltaTime;

            Debug.Log($"[MovePlayerCommand] Client {ClientId} moved by {direction}");
        }
    }

    public void Undo()
    {
        // For replay: move in opposite direction
        var pawnObject = context?.GetPlayerPawnObject(ClientId);
        if (pawnObject != null)
        {
            Vector3 movement = new Vector3(-direction.x, 0f, -direction.y) * speed * Time.deltaTime;
            pawnObject.transform.position += movement;
        }
    }
}

/// <summary>
/// Command for player action (shoot, interact, etc.).
/// </summary>
public class PlayerActionCommand : IPlayerCommand
{
    public ulong ClientId { get; private set; }
    private readonly IPlayerCommandContext context;
    private readonly string actionType;

    public PlayerActionCommand(IPlayerCommandContext context, ulong clientId, string actionType)
    {
        this.context = context;
        ClientId = clientId;
        this.actionType = actionType;
    }

    public void Execute()
    {
        var pawnObject = context?.GetPlayerPawnObject(ClientId);
        if (pawnObject != null)
        {
            Debug.Log($"[PlayerActionCommand] Client {ClientId} performed action: {actionType}");

            // Execute action based on type
            switch (actionType)
            {
                case "jump":
                    // Implement jump logic
                    break;
                case "shoot":
                    // Implement shoot logic
                    break;
                case "interact":
                    // Implement interact logic
                    break;
            }
        }
    }

    public void Undo()
    {
        // Most actions cannot be undone
        Debug.LogWarning($"[PlayerActionCommand] Cannot undo action: {actionType}");
    }
}

/// <summary>
/// Command invoker - manages command execution queue.
/// </summary>
public class CommandInvoker
{
    private readonly System.Collections.Generic.Queue<IPlayerCommand> commandQueue = new System.Collections.Generic.Queue<IPlayerCommand>();
    private readonly System.Collections.Generic.Stack<IPlayerCommand> commandHistory = new System.Collections.Generic.Stack<IPlayerCommand>();

    public void ExecuteCommand(IPlayerCommand command)
    {
        command.Execute();
        commandHistory.Push(command);
    }

    public void QueueCommand(IPlayerCommand command)
    {
        commandQueue.Enqueue(command);
    }

    public void ProcessQueue()
    {
        while (commandQueue.Count > 0)
        {
            var command = commandQueue.Dequeue();
            ExecuteCommand(command);
        }
    }

    public void UndoLastCommand()
    {
        if (commandHistory.Count > 0)
        {
            var command = commandHistory.Pop();
            command.Undo();
        }
    }
}
