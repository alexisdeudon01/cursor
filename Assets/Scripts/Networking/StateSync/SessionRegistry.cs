using Core.StateSync;
using System;
using System.Collections.Generic;

namespace Networking.StateSync
{
    public enum SessionLifecycleState
    {
        Lobby,
        Starting,
        InGame,
        Ended
    }

    public sealed class SessionEntry
    {
        public string SessionUid { get; }
        public string Name { get; set; }
        public string GameTypeUid { get; set; }
        public SessionLifecycleState State { get; set; }
        public List<string> ClientUids { get; } = new List<string>();

        public SessionEntry(string sessionUid, string name)
        {
            SessionUid = sessionUid;
            Name = name;
            State = SessionLifecycleState.Lobby;
        }
    }

    public sealed class SessionRegistry
    {
        private readonly Dictionary<string, SessionEntry> sessions = new Dictionary<string, SessionEntry>();
        private readonly Dictionary<string, string> nameToUid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, SessionEntry> Sessions => sessions;

        public SessionEntry Create(string name, string hostClientUid)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (nameToUid.ContainsKey(name))
            {
                return GetByName(name);
            }

            var uid = Guid.NewGuid().ToString("N");
            var entry = new SessionEntry(uid, name);
            if (!string.IsNullOrEmpty(hostClientUid))
            {
                entry.ClientUids.Add(hostClientUid);
            }

            sessions[uid] = entry;
            nameToUid[name] = uid;
            return entry;
        }

        public bool Remove(string sessionUid)
        {
            if (!sessions.TryGetValue(sessionUid, out var entry))
            {
                return false;
            }

            sessions.Remove(sessionUid);
            if (!string.IsNullOrEmpty(entry.Name))
            {
                nameToUid.Remove(entry.Name);
            }
            return true;
        }

        public SessionEntry GetByUid(string sessionUid)
        {
            if (string.IsNullOrEmpty(sessionUid))
            {
                return null;
            }

            return sessions.TryGetValue(sessionUid, out var entry) ? entry : null;
        }

        public SessionEntry GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return nameToUid.TryGetValue(name, out var uid) ? GetByUid(uid) : null;
        }

        public bool AddClient(string name, string clientUid)
        {
            if (string.IsNullOrEmpty(clientUid))
            {
                return false;
            }

            var entry = GetByName(name);
            if (entry == null)
            {
                return false;
            }

            if (!entry.ClientUids.Contains(clientUid))
            {
                entry.ClientUids.Add(clientUid);
            }
            return true;
        }

        public bool RemoveClient(string name, string clientUid)
        {
            var entry = GetByName(name);
            if (entry == null)
            {
                return false;
            }

            return entry.ClientUids.Remove(clientUid);
        }
    }
}
