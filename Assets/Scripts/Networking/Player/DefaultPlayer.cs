using Core.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class DefaultPlayer : NetworkBehaviour, IPlayerNameProvider
{
    // Player name synced to everyone - SERVER AUTHORITATIVE
    private NetworkVariable<FixedString64Bytes> _nameAgent = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server  // Changed: Only server can write
    );

    // Implementation of IPlayerNameProvider interface
    public NetworkVariable<FixedString64Bytes> NameAgent => _nameAgent;

    [SerializeField] private NetworkObject squarePrefab;

    private void OnEnable()
    {
        _nameAgent.OnValueChanged += OnNameChanged;
    }

    private void OnDisable()
    {
        _nameAgent.OnValueChanged -= OnNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DontDestroyOnLoad(gameObject);

        if (IsServer)
        {
            RegisterPlayer(_nameAgent.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            PlayerManager.Instance?.RemovePlayer(OwnerClientId);
        }
    }

    /// <summary>
    /// [Client] Request to change player name. Server validates and applies.
    /// </summary>
    [ServerRpc]
    public void SetNameServerRpc(string newName)
    {
        // Server-side validation
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning($"[DefaultPlayer] Rejected empty name from client {OwnerClientId}");
            return;
        }

        // Sanitize: trim and limit length
        newName = newName.Trim();
        if (newName.Length > 32)
            newName = newName.Substring(0, 32);

        // TODO: Add profanity filter, character validation, etc.

        _nameAgent.Value = new FixedString64Bytes(newName);
        Debug.Log($"[DefaultPlayer] Client {OwnerClientId} name set to: {newName}");
    }

    private void OnNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        if (IsServer)
        {
            RegisterPlayer(current);
        }
    }

    private void RegisterPlayer(FixedString64Bytes name)
    {
        PlayerManager.Instance?.RegisterPlayer(OwnerClientId, name.ToString());
    }

    public NetworkObject SpawnSquare(Vector3 position, NetworkObject overridePrefab = null)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[DefaultPlayer] SpawnSquare ignored on client");
            return null;
        }

        var prefabToUse = overridePrefab != null ? overridePrefab : squarePrefab;

        if (prefabToUse == null)
        {
            Debug.LogWarning("[DefaultPlayer] No square prefab assigned");
            return null;
        }

        var instance = Instantiate(prefabToUse, position, Quaternion.identity);
        var netObj = instance.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("[DefaultPlayer] Square prefab missing NetworkObject component");
            Destroy(instance.gameObject);
            return null;
        }

        netObj.SpawnWithOwnership(OwnerClientId);
        return netObj;
    }
}
