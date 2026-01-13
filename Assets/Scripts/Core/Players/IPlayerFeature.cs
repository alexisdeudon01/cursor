using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Interface for pluggable player features.
/// Implement this to add new functionality to players without modifying core player code.
/// 
/// <para><b>How to add a new player feature:</b></para>
/// <list type="number">
///   <item>Create a class implementing IPlayerFeature</item>
///   <item>Add it as a component to the player prefab, OR</item>
///   <item>Register it dynamically via PlayerFeatureRegistry</item>
/// </list>
/// </summary>
/// <example>
/// <code>
/// public class HealthFeature : NetworkBehaviour, IPlayerFeature
/// {
///     public string FeatureId => "health";
///     public NetworkVariable&lt;int&gt; Health = new(...);
///     
///     public void OnPlayerSpawn(ulong clientId) { Health.Value = 100; }
///     public void OnPlayerDespawn(ulong clientId) { }
/// }
/// </code>
/// </example>
public interface IPlayerFeature
{
    /// <summary>
    /// Unique identifier for this feature.
    /// </summary>
    string FeatureId { get; }

    /// <summary>
    /// Priority for initialization order (lower = earlier).
    /// </summary>
    int Priority => 0;

    /// <summary>
    /// Called when the player object spawns on network.
    /// </summary>
    void OnPlayerSpawn(ulong clientId);

    /// <summary>
    /// Called when the player object despawns.
    /// </summary>
    void OnPlayerDespawn(ulong clientId);
}

/// <summary>
/// Base class for player features that need networking.
/// Inherit from this instead of implementing IPlayerFeature directly
/// when you need NetworkVariables or RPCs.
/// </summary>
public abstract class NetworkPlayerFeature : NetworkBehaviour, IPlayerFeature
{
    public abstract string FeatureId { get; }
    public virtual int Priority => 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnPlayerSpawn(OwnerClientId);
    }

    public override void OnNetworkDespawn()
    {
        OnPlayerDespawn(OwnerClientId);
        base.OnNetworkDespawn();
    }

    public abstract void OnPlayerSpawn(ulong clientId);
    public abstract void OnPlayerDespawn(ulong clientId);
}

/// <summary>
/// Base class for player features that don't need networking.
/// Use for client-only features (effects, UI, sounds).
/// </summary>
public abstract class LocalPlayerFeature : MonoBehaviour, IPlayerFeature
{
    public abstract string FeatureId { get; }
    public virtual int Priority => 0;

    protected ulong OwnerClientId { get; private set; }

    public void Initialize(ulong clientId)
    {
        OwnerClientId = clientId;
        OnPlayerSpawn(clientId);
    }

    private void OnDestroy()
    {
        OnPlayerDespawn(OwnerClientId);
    }

    public abstract void OnPlayerSpawn(ulong clientId);
    public abstract void OnPlayerDespawn(ulong clientId);
}
