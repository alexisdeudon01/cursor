using UnityEngine;

/// <summary>
/// Identifie logiquement un prefab au runtime.
/// Sert à détecter et détruire les doublons du même prefab.
/// </summary>
[DisallowMultipleComponent]
public class PrefabIdentity : MonoBehaviour
{
    [SerializeField]
    private string prefabId = "NETWORK_MANAGER_ROOT";

    public string PrefabId => prefabId;
}
