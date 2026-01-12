using Unity.Collections;
using Unity.Netcode;

namespace Core.Networking
{
    /// <summary>
    /// Interface for accessing player name.
    /// Implemented by Networking layer to avoid circular dependency.
    /// </summary>
    public interface IPlayerNameProvider
    {
        NetworkVariable<FixedString64Bytes> NameAgent { get; }
    }
}
