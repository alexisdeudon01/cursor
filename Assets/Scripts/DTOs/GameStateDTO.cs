using Unity.Netcode;
using Unity.Collections;

public struct GameStateDTO : INetworkSerializable
{
    public int Tick;
    public FixedString64Bytes GamePhase;
    public int PlayerCount;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref GamePhase);
        serializer.SerializeValue(ref PlayerCount);
    }
}
