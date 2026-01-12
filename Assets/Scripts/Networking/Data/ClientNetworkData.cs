using System;

/// <summary>
/// Data structure for a connected network client.
/// Tracks player information, session membership, and activity.
/// </summary>
public class ClientNetworkData
{
    // Identity
    public ulong ClientId { get; }
    public string PlayerName { get; set; }

    // Session membership
    public string CurrentSessionId { get; set; }

    // Timestamps
    public DateTime ConnectedAt { get; }
    public DateTime LastActivity { get; set; }

    // State
    public bool IsReady { get; set; }

    // Network objects
    public Unity.Netcode.NetworkObject PlayerObject { get; set; }
    public Unity.Netcode.NetworkObject CurrentPawn { get; set; }

    // Statistics
    public int MessagesReceived { get; set; }
    public int MessagesSent { get; set; }
    public float TotalPlayTime => (float)(DateTime.UtcNow - ConnectedAt).TotalSeconds;

    public ClientNetworkData(ulong clientId)
    {
        ClientId = clientId;
        ConnectedAt = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
        PlayerName = $"Player {clientId}";
    }

    /// <summary>
    /// Update last activity timestamp.
    /// </summary>
    public void RecordActivity()
    {
        LastActivity = DateTime.UtcNow;
        MessagesReceived++;
    }

    /// <summary>
    /// Check if client is in a session.
    /// </summary>
    public bool IsInSession => !string.IsNullOrEmpty(CurrentSessionId);

    /// <summary>
    /// Get idle time in seconds.
    /// </summary>
    public float IdleTime => (float)(DateTime.UtcNow - LastActivity).TotalSeconds;

    public override string ToString()
    {
        return $"Client {ClientId} ({PlayerName}) - Session: {CurrentSessionId ?? "none"}, Ready: {IsReady}";
    }
}
