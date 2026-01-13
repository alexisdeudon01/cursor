using Unity.Netcode;
using UnityEngine;

public struct PlayerInputDTO : INetworkSerializable
{
    public Vector2 Movement;
    public bool ActionPressed;
    public uint SequenceNumber;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Movement);
        serializer.SerializeValue(ref ActionPressed);
        serializer.SerializeValue(ref SequenceNumber);
    }
}
