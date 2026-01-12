using Networking.StateSync;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Makes the camera follow the local player's pawn.
/// 
/// Data-oriented refactor (Option 1): pawns are NOT NetworkObjects, so we discover the local pawn
/// through EntityViewWorld / GameCommandClient rather than via NetworkObject ownership.
/// </summary>
public class CameraFollowPawn : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float height = 15f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool snapOnStart = true;

    private Transform targetPawn;
    private bool initialized;

    private void LateUpdate()
    {
        if (NetworkManager.Singleton == null)
            return;

        // Only run on clients (including host client).
        if (!NetworkManager.Singleton.IsClient)
            return;

        if (!initialized)
        {
            TryFindLocalPawn();
            return;
        }

        if (targetPawn == null)
        {
            initialized = false;
            return;
        }

        FollowTarget();
    }

    private void TryFindLocalPawn()
    {
        // Preferred: query the replicated view world.
        if (GameCommandClient.Instance != null && GameCommandClient.Instance.TryGetLocalPawnTransform(out var pawnTransform))
        {
            SetTarget(pawnTransform);
            return;
        }

        if (EntityViewWorld.Instance != null && EntityViewWorld.Instance.TryGetLocalPawnTransform(out pawnTransform))
        {
            SetTarget(pawnTransform);
            return;
        }

        // Fallback: scan scene.
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        pawnTransform = FindLocalPawnTransform(FindObjectsByType<PlayerPawn>(FindObjectsSortMode.None), localClientId)
                        ?? FindLocalPawnTransform(FindObjectsByType<CirclePawn>(FindObjectsSortMode.None), localClientId);

        if (pawnTransform != null)
        {
            SetTarget(pawnTransform);
        }
    }

    private void FollowTarget()
    {
        Vector3 targetPos = new Vector3(targetPawn.position.x, height, targetPawn.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    private void SnapToTarget()
    {
        if (targetPawn == null)
            return;

        transform.position = new Vector3(targetPawn.position.x, height, targetPawn.position.z);
    }

    /// <summary>
    /// Manually set the target pawn (alternative to auto-find).
    /// </summary>
    public void SetTarget(Transform pawn)
    {
        targetPawn = pawn;
        initialized = (targetPawn != null);

        if (initialized && snapOnStart)
            SnapToTarget();
    }

    private static Transform FindLocalPawnTransform(PlayerPawn[] pawns, ulong clientId)
    {
        foreach (var pawn in pawns)
        {
            if (pawn != null && pawn.OwnerClientId == clientId)
                return pawn.transform;
        }
        return null;
    }

    private static Transform FindLocalPawnTransform(CirclePawn[] pawns, ulong clientId)
    {
        foreach (var pawn in pawns)
        {
            if (pawn != null && pawn.OwnerClientId == clientId)
                return pawn.transform;
        }
        return null;
    }
}
