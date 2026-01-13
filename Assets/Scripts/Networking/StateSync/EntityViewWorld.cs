using Core.Maps;
using Core.StateSync;
using Networking.StateSync;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Client-side registry of spawned entity views.
///
/// Data-oriented refactor (Option 1): entities are spawned locally from replicated DTO commands.
/// </summary>
public class EntityViewWorld : MonoBehaviour
{
    public static EntityViewWorld Instance { get; private set; }

    /// <summary>
    /// Raised when the local client's pawn view becomes available.
    /// </summary>
    public event Action<Transform> OnLocalPawnAvailable;

        private readonly Dictionary<FixedString64Bytes, MonoBehaviour> views = new Dictionary<FixedString64Bytes, MonoBehaviour>();

    public static EntityViewWorld EnsureInstance()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("EntityViewWorld");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<EntityViewWorld>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearAll()
    {
        foreach (var kvp in views)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        views.Clear();
    }

    public bool TryGetLocalPawnTransform(out Transform transform)
    {
        transform = null;
        if (NetworkManager.Singleton == null)
            return false;

        ulong localId = NetworkManager.Singleton.LocalClientId;

        foreach (var kvp in views)
        {
            if (kvp.Value == null) continue;

            if (kvp.Value is PlayerPawn square && square.OwnerClientId == localId)
            {
                transform = square.transform;
                return true;
            }
            if (kvp.Value is CirclePawn circle && circle.OwnerClientId == localId)
            {
                transform = circle.transform;
                return true;
            }
        }

        return false;
    }

    public void ApplySpawn(GameCommandDto command)
    {
        if (command.Type != GameCommandType.SpawnEntity)
            return;

        if (command.EntityId.IsEmpty)
            return;

        var entityId = command.EntityId;

        // Create if missing
        if (!views.TryGetValue(entityId, out var view) || view == null)
        {
            view = CreateViewForPrefabType((PawnPrefabType)command.PrefabType);
            if (view == null)
                return;

            view.gameObject.name = $"Entity_{entityId.ToString()}";
            views[entityId] = view;
        }

        var config = GameCommandClient.Instance != null ? GameCommandClient.Instance.CurrentConfig : null;
        var worldPos = GridMapUtils.CellToWorld(config, command.CellX, command.CellY);

        // Apply metadata + transform
        if (view is PlayerPawn squarePawn)
        {
            squarePawn.ApplySpawnData(command.OwnerClientId, command.DisplayName, command.ColorIndex);
            squarePawn.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
        }
        else if (view is CirclePawn circlePawn)
        {
            circlePawn.ApplySpawnData(command.OwnerClientId, command.DisplayName, command.ColorIndex);
            circlePawn.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
        }

        // Notify local
        if (NetworkManager.Singleton != null && command.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            OnLocalPawnAvailable?.Invoke(view.transform);
        }
    }

    public void ApplyUpdate(GameCommandDto command)
    {
        if (command.Type != GameCommandType.UpdateEntity)
            return;

        if (command.EntityId.IsEmpty)
            return;

        if (!views.TryGetValue(command.EntityId, out var view) || view == null)
            return;

        var config = GameCommandClient.Instance != null ? GameCommandClient.Instance.CurrentConfig : null;
        var worldPos = GridMapUtils.CellToWorld(config, command.CellX, command.CellY);
        view.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
    }

    public void ApplyRemove(GameCommandDto command)
    {
        if (command.Type != GameCommandType.RemoveEntity)
            return;

        if (command.EntityId.IsEmpty)
            return;

        if (!views.TryGetValue(command.EntityId, out var view) || view == null)
            return;

        Destroy(view.gameObject);
        views.Remove(command.EntityId);
    }

    private static MonoBehaviour CreateViewForPrefabType(PawnPrefabType prefabType)
    {
        var rootName = prefabType == PawnPrefabType.Circle ? "Circle" : "Square";
        var go = new GameObject(rootName);

        switch (prefabType)
        {
            case PawnPrefabType.Circle:
                return go.AddComponent<CirclePawn>();
            case PawnPrefabType.Square:
            default:
                return go.AddComponent<PlayerPawn>();
        }
    }
}
