using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Games
{
    /// <summary>
    /// Isolated container for a single game session.
    /// Ensures data isolation between sessions (world partitioning via WorldOffset).
    ///
    /// Data-oriented refactor (Option 1):
    /// - SessionContainer tracks ONLY session metadata and the set of clientIds in the session.
    /// - The authoritative simulation state lives in GameInstanceManager (SimWorld).
    /// - No per-player NetworkObjects are stored here.
    /// </summary>
    public sealed class SessionContainer : IDisposable
    {
        // ============================
        //   SESSION IDENTITY
        // ============================

        public string SessionName { get; }
        public string SessionId { get; }
        public Vector3 WorldOffset { get; }
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Optional error signal for session-level failures (used by SessionContainerManager).
        /// </summary>
        public event System.Action<SessionContainer, string> OnError;

        internal void RaiseError(string error)
        {
            OnError?.Invoke(this, error);
        }


        // ============================
        //   PLAYER MANAGEMENT
        // ============================

        private readonly HashSet<ulong> _players = new HashSet<ulong>();
        private readonly object _lock = new object();

        public ulong HostClientId { get; private set; }
        public int PlayerCount { get { lock (_lock) return _players.Count; } }
        public int MaxPlayers { get; set; } = 8;

        // ============================
        //   GAME STATE
        // ============================

        public enum SessionState { Lobby, Starting, InGame, Ended }
        public SessionState State { get; private set; } = SessionState.Lobby;

        public string GameId { get; private set; }
        public IGameDefinition GameDefinition { get; private set; }

        public SessionContainer(string sessionName, Vector3 worldOffset, string sessionId = null)
        {
            SessionName = sessionName;
            WorldOffset = worldOffset;
            SessionId = string.IsNullOrWhiteSpace(sessionId) ? sessionName : sessionId;
            CreatedAt = DateTime.UtcNow;
        }

        public bool AddPlayer(ulong clientId)
        {
            lock (_lock)
            {
                if (_players.Count >= MaxPlayers)
                    return false;

                if (_players.Add(clientId))
                {
                    if (HostClientId == 0)
                        HostClientId = clientId;
                    return true;
                }

                return false;
            }
        }

        public bool RemovePlayer(ulong clientId)
        {
            lock (_lock)
            {
                if (!_players.Remove(clientId))
                    return false;

                if (HostClientId == clientId)
                {
                    HostClientId = 0;
                    foreach (var id in _players)
                    {
                        HostClientId = id;
                        break;
                    }
                }

                return true;
            }
        }

        public bool HasPlayer(ulong clientId)
        {
            lock (_lock) return _players.Contains(clientId);
        }

        public List<ulong> GetAllPlayers()
        {
            lock (_lock) return new List<ulong>(_players);
        }

        public IEnumerable<ulong> GetPlayerIds()
        {
            lock (_lock) return new List<ulong>(_players);
        }

        public bool StartGame(string gameId, IGameDefinition gameDef)
        {
            if (State != SessionState.Lobby)
            {
                Debug.LogWarning($"[SessionContainer] Cannot start game: session '{SessionName}' is not in Lobby state.");
                return false;
            }

            GameId = gameId;
            GameDefinition = gameDef;
            State = SessionState.Starting;

            Debug.Log($"[SessionContainer] Starting game '{gameId}' in session '{SessionName}'");
            return true;
        }

        public void SetGameRunning()
        {
            if (State == SessionState.Starting)
            {
                State = SessionState.InGame;
                Debug.Log($"[SessionContainer] Session '{SessionName}' is now InGame");
            }
        }

        public void EndGame()
        {
            if (State == SessionState.Lobby)
                return;

            State = SessionState.Ended;
            Debug.Log($"[SessionContainer] Session '{SessionName}' ended");
        }

        public bool IsGameRunning() => State == SessionState.InGame;

        public SessionStats GetStats()
        {
            return new SessionStats(
                sessionName: SessionName,
                sessionId: SessionId,
                playerCount: PlayerCount,
                hostClientId: HostClientId,
                state: State.ToString(),
                gameId: GameId,
                createdAtUtc: CreatedAt
            );
        }

        /// <summary>
        /// Coarse validation for session world partitioning (NOT map bounds).
        /// </summary>
        public bool ValidatePositionInBounds(Vector3 position)
        {
            float halfSpace = 25f;
            float padding = 2f;

            Vector3 min = WorldOffset + new Vector3(-halfSpace + padding, -10f, -halfSpace + padding);
            Vector3 max = WorldOffset + new Vector3(halfSpace - padding, 10f, halfSpace - padding);

            return position.x >= min.x && position.x <= max.x &&
                   position.z >= min.z && position.z <= max.z;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _players.Clear();
                HostClientId = 0;
            }
        }
    }
}