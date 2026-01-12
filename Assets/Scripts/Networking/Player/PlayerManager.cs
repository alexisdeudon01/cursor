using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//PLayers list
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private Dictionary<ulong, PlayerData> players = new();

    private void Awake()
    {
        Debug.Log("[PlayerManager] Awake");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerManager] Duplicate → DESTROY");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        Debug.Log("[PlayerManager] READY (server authority)");
    }

    // ============================================================
    // MAIN API (SERVER ONLY)
    // ============================================================

    public void RegisterPlayer(ulong clientId, string pseudo)
    {
        // SERVER ONLY: Player registry is server-authoritative
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[PlayerManager] RegisterPlayer ignored on client");
            return;
        }

        try
        {
            Debug.Log($"[PlayerManager] RegisterPlayer(client={clientId}, name={pseudo})");

            players[clientId] = new PlayerData
            {
                clientId = clientId,
                name = pseudo
            };
        }
        catch (Exception ex)
        {
            Debug.LogError("[PlayerManager] ❌ Exception in RegisterPlayer()");
            Debug.LogException(ex);
        }
    }

    public PlayerData GetPlayer(ulong clientId)
    {
        players.TryGetValue(clientId, out var data);
        return data;
    }

    public void RemovePlayer(ulong clientId)
    {
        // SERVER ONLY
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            return;

        if (players.Remove(clientId))
            Debug.Log($"[PlayerManager] Player removed → {clientId}");
    }
}

[Serializable]
public class PlayerData
{
    public ulong clientId;
    public string name;
}
