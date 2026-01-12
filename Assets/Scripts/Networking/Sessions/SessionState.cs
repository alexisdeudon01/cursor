using System.Collections.Generic;

/// <summary>
/// Server-side session state.
/// </summary>
public class SessionState
{
    public string Name;
    public ulong Creator;
    public HashSet<ulong> Players = new HashSet<ulong>();
    public HashSet<ulong> Ready = new HashSet<ulong>();
    
    /// <summary>
    /// The selected game type ID for this session.
    /// Set by the session creator before starting.
    /// </summary>
    public string SelectedGameId;
}
