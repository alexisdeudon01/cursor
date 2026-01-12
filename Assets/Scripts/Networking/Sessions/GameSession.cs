using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct GameSession : INetworkSerializable, IEquatable<GameSession>
{
    public FixedString64Bytes name;
    public ulong creator;
    public int playerCount;
    public int readyCount;

    public GameSession(string name, ulong creator, int playerCount = 1, int readyCount = 0)
    {
        this.name = new FixedString64Bytes(name);
        this.creator = creator;
        this.playerCount = playerCount;
        this.readyCount = readyCount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref creator);
        serializer.SerializeValue(ref playerCount);
        serializer.SerializeValue(ref readyCount);
    }

    public bool Equals(GameSession other)
    {
        return creator == other.creator && name.Equals(other.name);
    }

    public override string ToString()
    {
        return $"{name.ToString()} ({playerCount} joueurs, prêts: {readyCount})";
    }
}

[Serializable]
public struct SessionPlayerInfo : INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes name;
    public bool ready;
    public bool isCreator;

    public SessionPlayerInfo(ulong clientId, string name, bool ready, bool isCreator)
    {
        this.clientId = clientId;
        this.name = new FixedString64Bytes(name ?? string.Empty);
        this.ready = ready;
        this.isCreator = isCreator;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref ready);
        serializer.SerializeValue(ref isCreator);
    }
}

[Serializable]
public struct SessionDetails : INetworkSerializable
{
    public GameSession session;
    public List<SessionPlayerInfo> players;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref session);

        int count = players != null ? players.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsWriter)
        {
            for (int i = 0; i < count; i++)
            {
                var p = players[i];
                serializer.SerializeValue(ref p);
            }
        }
        else
        {
            if (players == null)
                players = new List<SessionPlayerInfo>(count);
            else
                players.Clear();

            for (int i = 0; i < count; i++)
            {
                var p = new SessionPlayerInfo();
                serializer.SerializeValue(ref p);
                players.Add(p);
            }
        }
    }
}
