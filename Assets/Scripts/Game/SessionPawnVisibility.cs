using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Legacy compatibility shim.
/// 
/// In the data-oriented refactor (Option 1) player pawns are NOT NetworkObjects, and there is
/// only one active game scene per client, so session-based pawn filtering is unnecessary.
/// 
/// This component is kept to avoid breaking UI/scripts that still call SetLocalSession / ClearLocalSession.
/// </summary>
public class SessionPawnVisibility : MonoBehaviour
{
    public static SessionPawnVisibility Instance { get; private set; }

    private string localSessionName;
    private readonly Dictionary<ulong, string> pawnSessions = new Dictionary<ulong, string>();

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

    public void SetLocalSession(string sessionName)
    {
        localSessionName = sessionName;
        // No filtering required in Option 1.
    }

    public void ClearLocalSession()
    {
        localSessionName = null;
        // No filtering required in Option 1.
    }

    public void RegisterPawn(ulong networkObjectId, string sessionName)
    {
        // Kept for compatibility with Option 2. In Option 1 this is not used.
        pawnSessions[networkObjectId] = sessionName;
    }

    public void UnregisterPawn(ulong networkObjectId)
    {
        pawnSessions.Remove(networkObjectId);
    }

    public void UpdateAllPawnVisibility()
    {
        // No filtering required in Option 1.
    }

    public string GetPawnSession(ulong networkObjectId)
    {
        return pawnSessions.TryGetValue(networkObjectId, out var session) ? session : localSessionName;
    }
}
