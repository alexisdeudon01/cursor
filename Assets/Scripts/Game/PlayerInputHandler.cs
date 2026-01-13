using Unity.Netcode;
using UnityEngine;
using Networking.StateSync;
using Unity.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles player input and sends movement requests to the server.
/// Client-only component, created automatically when a game starts.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance { get; private set; }

    private string _sessionName;

    [Header("Networking")]
    [SerializeField, Min(1f)] private float sendRateHz = 20f;
    private GridDirection _lastSentDir = GridDirection.None;
    private float _nextSendTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Set the session name for this player's input.
    /// </summary>
    public void SetSession(string sessionName)
    {
        _sessionName = sessionName;
        Debug.Log($"[PlayerInputHandler] Session set: {sessionName}");
    }

    /// <summary>
    /// Ensure the input handler exists.
    /// </summary>
    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("PlayerInputHandler");
            go.AddComponent<PlayerInputHandler>();
        }
    }

    private float _lastLogTime = 0f;

private void Update()
{
    // Client-only
    if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        return;

    if (string.IsNullOrEmpty(_sessionName))
    {
        // Log once per second if session not set
        if (Time.time - _lastLogTime > 2f)
        {
            Debug.LogWarning("[PlayerInputHandler] No session set - waiting for game start");
            _lastLogTime = Time.time;
        }
        return;
    }

    if (SessionRpcHub.Instance == null)
        return;

    var dir = ReadInputDirection();
    float now = Time.unscaledTime;

    bool changed = dir != _lastSentDir;
    bool heartbeat = dir != GridDirection.None && now >= _nextSendTime;

    // IMPORTANT: we must send "stop" (GridDirection.None) when input is released, otherwise
    // the server-authoritative sim will keep the last direction forever.
    if (!changed && !heartbeat)
        return;

    FixedString64Bytes sessionUid = default;
    if (GameCommandClient.Instance != null && !GameCommandClient.Instance.CurrentSessionUid.IsEmpty)
    {
        sessionUid = GameCommandClient.Instance.CurrentSessionUid;
    }
    else
    {
        sessionUid = _sessionName;
    }

    var clientVersion = GameCommandClient.Instance != null ? GameCommandClient.Instance.LastAppliedVersion : 0;
    var command = GameCommandFactory.CreateMoveInput(sessionUid, dir, clientVersion);
    SessionRpcHub.Instance.SendGameCommandServerRpc(command);

    _lastSentDir = dir;
    _nextSendTime = now + (1f / Mathf.Max(1f, sendRateHz));
}

    private GridDirection ReadInputDirection()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null)
            return GridDirection.None;

        if (kb.upArrowKey.isPressed || kb.wKey.isPressed || kb.zKey.isPressed)
            return GridDirection.Up;
        if (kb.downArrowKey.isPressed || kb.sKey.isPressed)
            return GridDirection.Down;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed || kb.qKey.isPressed)
            return GridDirection.Left;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed || kb.eKey.isPressed)
            return GridDirection.Right;
        return GridDirection.None;
#else
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z))
            return GridDirection.Up;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            return GridDirection.Down;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
            return GridDirection.Left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.E))
            return GridDirection.Right;
        return GridDirection.None;
#endif
    }
}
