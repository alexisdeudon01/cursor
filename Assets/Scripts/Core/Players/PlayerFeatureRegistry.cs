using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Registry for player feature prefabs/components.
/// Features registered here will be automatically attached to new players.
/// </summary>
public static class PlayerFeatureRegistry
{
    private static readonly List<Type> featureTypes = new List<Type>();
    private static readonly List<GameObject> featurePrefabs = new List<GameObject>();

    /// <summary>
    /// Register a feature type to be added to all players.
    /// The component will be added when the player spawns.
    /// </summary>
    /// <typeparam name="T">The feature type (must implement IPlayerFeature)</typeparam>
    public static void RegisterFeatureType<T>() where T : MonoBehaviour, IPlayerFeature
    {
        if (!featureTypes.Contains(typeof(T)))
        {
            featureTypes.Add(typeof(T));
            Debug.Log($"[PlayerFeatureRegistry] Registered feature type: {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Register a prefab that contains player features.
    /// The prefab will be instantiated and parented to the player.
    /// </summary>
    public static void RegisterFeaturePrefab(GameObject prefab)
    {
        if (prefab != null && !featurePrefabs.Contains(prefab))
        {
            featurePrefabs.Add(prefab);
            Debug.Log($"[PlayerFeatureRegistry] Registered feature prefab: {prefab.name}");
        }
    }

    /// <summary>
    /// Attach all registered features to a player object.
    /// Called by the player spawn system.
    /// </summary>
    public static void AttachFeatures(GameObject playerObject, ulong clientId)
    {
        if (playerObject == null) return;

        // Add feature components
        foreach (var type in featureTypes)
        {
            if (playerObject.GetComponent(type) == null)
            {
                var feature = playerObject.AddComponent(type) as IPlayerFeature;
                Debug.Log($"[PlayerFeatureRegistry] Attached {type.Name} to player {clientId}");
            }
        }

        // Instantiate feature prefabs
        foreach (var prefab in featurePrefabs)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, playerObject.transform);
            
            // Initialize any local features
            foreach (var feature in instance.GetComponents<LocalPlayerFeature>())
            {
                feature.Initialize(clientId);
            }
        }
    }

    /// <summary>
    /// Clear all registered features.
    /// </summary>
    public static void Clear()
    {
        featureTypes.Clear();
        featurePrefabs.Clear();
    }
}
