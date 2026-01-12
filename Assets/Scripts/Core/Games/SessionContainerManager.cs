using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Games
{
/// <summary>
/// Centralized manager for all session containers.
/// Provides complete isolation between sessions with validation at every access point.
/// Thread-safe for concurrent session operations.
/// NOT a MonoBehaviour - used as a plain class.
/// </summary>
public class SessionContainerManager : IDisposable
{
    // ============================================
    //   CONFIGURATION
    // ============================================
    
    private int maxConcurrentSessions = 100;
    private float sessionOffsetSpacing = 50f;
    
    // ============================================
    //   CONTAINER STORAGE (Thread-safe)
    // ============================================
    
    private readonly ConcurrentDictionary<string, SessionContainer> sessionsByName = 
        new ConcurrentDictionary<string, SessionContainer>();
    
    private readonly ConcurrentDictionary<ulong, string> clientToSession = 
        new ConcurrentDictionary<ulong, string>();
    
    private int nextSessionIndex = 0;
    private readonly object indexLock = new object();
    private bool disposed = false;
    
	    // ============================================
	    //   EVENTS
	    // ============================================
	    
	    public event Action<SessionContainer> OnSessionCreated;
	    public event Action<string> OnSessionDestroyed;
    
    // ============================================
    //   CONSTRUCTOR
    // ============================================
    
    public SessionContainerManager(int maxSessions = 100, float offsetSpacing = 50f)
    {
        maxConcurrentSessions = maxSessions;
        sessionOffsetSpacing = offsetSpacing;
        Debug.Log($"[SessionContainerManager] Initialized (max={maxSessions}, spacing={offsetSpacing})");
    }
    
    // ============================================
    //   SESSION LIFECYCLE
    // ============================================
    
    /// <summary>
    /// Create a new isolated session container.
    /// </summary>
    public SessionContainer CreateSession(string sessionName, int maxPlayers, ulong hostClientId)
    {
        if (disposed)
        {
            Debug.LogError("[SessionContainerManager] Manager is disposed!");
            return null;
        }
        
        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("[SessionContainerManager] Session name cannot be empty");
            return null;
        }
        
        if (sessionsByName.ContainsKey(sessionName))
        {
            Debug.LogWarning($"[SessionContainerManager] Session '{sessionName}' already exists");
            return null;
        }
        
        if (sessionsByName.Count >= maxConcurrentSessions)
        {
            Debug.LogError($"[SessionContainerManager] Max sessions ({maxConcurrentSessions}) reached");
            return null;
        }
        
        // Calculate isolated world offset
        Vector3 worldOffset;
        lock (indexLock)
        {
            worldOffset = new Vector3(nextSessionIndex * sessionOffsetSpacing, 0, 0);
            nextSessionIndex++;
        }
        
        var container = new SessionContainer(sessionName, worldOffset, sessionName);
        container.MaxPlayers = maxPlayers;
        // Host becomes first member (and thus HostClientId) in the container
        container.AddPlayer(hostClientId);
        
        // Register events for monitoring
        container.OnError += HandleContainerError;
        
        if (!sessionsByName.TryAdd(sessionName, container))
        {
            container.Dispose();
            return null;
        }
        
        // Map host to session
        clientToSession[hostClientId] = sessionName;
        
        Debug.Log($"[SessionContainerManager] Created session '{sessionName}' (offset: {worldOffset})");
        OnSessionCreated?.Invoke(container);
        
        return container;
    }
    
    /// <summary>
    /// Destroy a session and cleanup all resources.
    /// </summary>
    public bool DestroySession(string sessionName)
    {
        if (!sessionsByName.TryRemove(sessionName, out var container))
            return false;
        
        // Remove all client mappings
        foreach (var player in container.GetAllPlayers())
        {
            clientToSession.TryRemove(player, out _);
        }
        
        container.OnError -= HandleContainerError;
        container.Dispose();
        
        Debug.Log($"[SessionContainerManager] Destroyed session '{sessionName}'");
        OnSessionDestroyed?.Invoke(sessionName);
        
        return true;
    }
    
    // ============================================
    //   SECURE ACCESS
    // ============================================
    
    /// <summary>
    /// Get session container by name (no auth check - use for joining).
    /// </summary>
    public SessionContainer GetSession(string sessionName)
    {
        return sessionsByName.TryGetValue(sessionName, out var container) ? container : null;
    }
    
    /// <summary>
    /// Get the session a client belongs to.
    /// </summary>
    public string GetClientSession(ulong clientId)
    {
        return clientToSession.TryGetValue(clientId, out var sessionName) ? sessionName : null;
    }
    
    /// <summary>
    /// Check if session exists.
    /// </summary>
    public bool HasSession(string sessionName)
    {
        return sessionsByName.ContainsKey(sessionName);
    }
    
    // ============================================
    //   PLAYER OPERATIONS (with validation)
    // ============================================
    
    /// <summary>
    /// Add a player to a session with full validation.
    /// </summary>
    public bool JoinSession(string sessionName, ulong clientId, string playerName)
    {
        var container = GetSession(sessionName);
        if (container == null)
        {
            Debug.LogWarning($"[SessionContainerManager] Session '{sessionName}' not found");
            return false;
        }
        
        // Check if client is already in another session
        if (clientToSession.TryGetValue(clientId, out var existingSession))
        {
            if (existingSession != sessionName)
            {
                Debug.LogWarning($"[SessionContainerManager] Client {clientId} already in session '{existingSession}'");
                return false;
            }
        }
        
        if (!container.AddPlayer(clientId))
            return false;
        
        clientToSession[clientId] = sessionName;
        return true;
    }
    
    /// <summary>
    /// Remove a player from their current session.
    /// </summary>
    public bool LeaveSession(ulong clientId)
    {
        if (!clientToSession.TryRemove(clientId, out var sessionName))
            return false;
        
        var container = GetSession(sessionName);
        container?.RemovePlayer(clientId);
        
        return true;
    }
    
    // ============================================
    //   CROSS-SESSION VALIDATION
    // ============================================
    
    /// <summary>
    /// Validate that a client belongs to a specific session.
    /// </summary>
    public bool ValidateClientSession(ulong clientId, string sessionName)
    {
        if (!clientToSession.TryGetValue(clientId, out var actualSession))
            return false;
        return actualSession == sessionName;
    }
    
    /// <summary>
    /// Validate that two clients are in the same session.
    /// </summary>
    public bool AreClientsInSameSession(ulong client1, ulong client2)
    {
        if (!clientToSession.TryGetValue(client1, out var session1))
            return false;
        if (!clientToSession.TryGetValue(client2, out var session2))
            return false;
        return session1 == session2;
    }
    
    /// <summary>
    /// Validate that a world position belongs to a session.
    /// </summary>
    public bool IsPositionInSession(string sessionName, Vector3 position)
    {
        var container = GetSession(sessionName);
        return container?.ValidatePositionInBounds(position) ?? false;
    }
    
    // ============================================
    //   MONITORING & STATS
    // ============================================
    
    public int GetSessionCount() => sessionsByName.Count;
    
    public int GetTotalClientCount() => clientToSession.Count;
    
    public IEnumerable<string> GetAllSessionNames() => sessionsByName.Keys;
    
    public List<SessionStats> GetAllSessionStats()
    {
        var stats = new List<SessionStats>();
        foreach (var container in sessionsByName.Values)
        {
            stats.Add(container.GetStats());
        }
        return stats;
    }
    
    // ============================================
    //   CLIENT DISCONNECT HANDLING
    // ============================================
    
    /// <summary>
    /// Handle client disconnection - remove from all sessions.
    /// </summary>
    public void HandleClientDisconnect(ulong clientId)
    {
        if (clientToSession.TryRemove(clientId, out var sessionName))
        {
            var container = GetSession(sessionName);
            container?.RemovePlayer(clientId);
            Debug.Log($"[SessionContainerManager] Client {clientId} disconnected from session '{sessionName}'");
        }
    }
    
    // ============================================
    //   ERROR HANDLING
    // ============================================
    
    private void HandleContainerError(SessionContainer container, string error)
    {
        Debug.LogWarning($"[SessionContainerManager] Container error in '{container.SessionName}': {error}");
    }
    
    // ============================================
    //   DISPOSE
    // ============================================
    
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        
        foreach (var container in sessionsByName.Values)
        {
            container.OnError -= HandleContainerError;
            container.Dispose();
        }
        
        sessionsByName.Clear();
        clientToSession.Clear();
        
        Debug.Log("[SessionContainerManager] Disposed");
    }
}
}
