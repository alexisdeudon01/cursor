using System;

namespace Core.Games
{
    /// <summary>
    /// Lightweight monitoring snapshot for a session container.
    /// (Pure metadata; does not expose any gameplay state.)
    /// </summary>
    public readonly struct SessionStats
    {
        public readonly string SessionName;
        public readonly string SessionId;
        public readonly int PlayerCount;
        public readonly ulong HostClientId;
        public readonly string State;
        public readonly string GameId;
        public readonly DateTime CreatedAtUtc;

        public SessionStats(string sessionName, string sessionId, int playerCount, ulong hostClientId, string state, string gameId, DateTime createdAtUtc)
        {
            SessionName = sessionName;
            SessionId = sessionId;
            PlayerCount = playerCount;
            HostClientId = hostClientId;
            State = state;
            GameId = gameId;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
