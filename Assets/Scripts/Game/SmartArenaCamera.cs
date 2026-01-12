using Networking.StateSync;
using UnityEngine;

/// <summary>
/// Smart arena camera for the client Game scene.
/// 
/// Goals:
/// - Always keep the whole arena visible (fit-to-screen), accounting for aspect ratio.
/// - Optionally follow the local player a little (dead-zone + clamped offset),
///   while zooming out if needed so the full arena still stays in view.
/// - Smooth movement and zoom.
/// 
/// This script is auto-added/configured by MapConfigSceneBuilder when MapConfig arrives.
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class SmartArenaCamera : MonoBehaviour
{
    [Header("Fit")]
    [SerializeField] private float margin = 1.15f;
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float maxOrthoSize = 500f;
    [SerializeField] private float cameraHeight = 10f;

    [Header("Follow local player (optional)")]
    [SerializeField] private bool followLocalPlayer = true;
    [SerializeField] private float deadZoneRadius = 1.0f;
    [SerializeField] private float maxFollowOffset = 8f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.20f;
    [SerializeField] private float zoomSmoothTime = 0.25f;

    private Camera cam;

    private MapConfigData config;
    private Vector3 arenaCenter;
    private float arenaHalfWidth;
    private float arenaHalfHeight;

    private float currentOrtho;
    private float orthoVel;
    private Vector3 posVel;
    private float lastAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        lastAspect = cam.aspect;
        currentOrtho = Mathf.Max(minOrthoSize, cam.orthographicSize);
    }

    public void ApplyMapConfig(MapConfigData cfg)
    {
        config = cfg;

        if (cfg == null)
            return;

        cam.orthographic = true;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        RecomputeArena(cfg);
        lastAspect = cam.aspect;

        // Snap initial placement for immediate correct view.
        SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (config == null)
            return;

        var targetCenter = ComputeTargetCenter(out _);
        var ortho = ComputeRequiredOrtho(targetCenter);

        currentOrtho = ortho;
        cam.orthographicSize = currentOrtho;

        transform.position = new Vector3(targetCenter.x, arenaCenter.y + cameraHeight, targetCenter.z);
    }

    private void LateUpdate()
    {
        if (config == null)
            return;

        // Handle window resize / aspect changes
        if (Mathf.Abs(cam.aspect - lastAspect) > 0.0001f)
        {
            lastAspect = cam.aspect;
            RecomputeArena(config);
        }

        var targetCenter = ComputeTargetCenter(out var offset);
        var targetOrtho = ComputeRequiredOrtho(targetCenter);

        // Smooth zoom
        currentOrtho = Mathf.SmoothDamp(currentOrtho, targetOrtho, ref orthoVel, zoomSmoothTime);
        currentOrtho = Mathf.Clamp(currentOrtho, minOrthoSize, maxOrthoSize);
        cam.orthographicSize = currentOrtho;

        // Smooth position
        var targetPos = new Vector3(targetCenter.x, arenaCenter.y + cameraHeight, targetCenter.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVel, positionSmoothTime);
    }

    private void RecomputeArena(MapConfigData cfg)
    {
        arenaCenter = cfg.worldOffset;

        if (cfg.shape == MapShape.Circle)
        {
            arenaHalfWidth = Mathf.Abs(cfg.circleRadius);
            arenaHalfHeight = Mathf.Abs(cfg.circleRadius);
        }
        else
        {
            float width = Mathf.Abs(cfg.mapSize.x);
            float height = Mathf.Abs(cfg.mapSize.z);
            if (Mathf.Approximately(height, 0f))
                height = Mathf.Abs(cfg.mapSize.y);

            arenaHalfWidth = width * 0.5f;
            arenaHalfHeight = height * 0.5f;
        }
    }

    private Vector3 ComputeTargetCenter(out Vector3 offsetFromArenaCenter)
    {
        offsetFromArenaCenter = Vector3.zero;

        if (!followLocalPlayer || EntityViewWorld.Instance == null)
            return arenaCenter;

        if (!EntityViewWorld.Instance.TryGetLocalPawnTransform(out var pawnTf) || pawnTf == null)
            return arenaCenter;

        var pawnPos = pawnTf.position;
        // We only care about XZ plane in top-down
        var delta = new Vector3(pawnPos.x - arenaCenter.x, 0f, pawnPos.z - arenaCenter.z);

        if (delta.magnitude <= deadZoneRadius)
            return arenaCenter;

        delta = Vector3.ClampMagnitude(delta, maxFollowOffset);
        offsetFromArenaCenter = delta;
        return arenaCenter + delta;
    }

    /// <summary>
    /// Compute ortho size required so that the full arena stays visible even if the camera center is offset.
    /// </summary>
    private float ComputeRequiredOrtho(Vector3 cameraCenter)
    {
        float aspect = Mathf.Max(0.0001f, cam.aspect);

        float offsetX = Mathf.Abs(cameraCenter.x - arenaCenter.x);
        float offsetZ = Mathf.Abs(cameraCenter.z - arenaCenter.z);

        // To keep full arena visible:
        // Vertical coverage: ortho >= halfHeight + |offsetZ|
        // Horizontal coverage: ortho*aspect >= halfWidth + |offsetX| => ortho >= (halfWidth + |offsetX|)/aspect
        float requiredV = arenaHalfHeight + offsetZ;
        float requiredH = (arenaHalfWidth + offsetX) / aspect;

        float ortho = Mathf.Max(minOrthoSize, Mathf.Max(requiredV, requiredH));
        ortho *= margin;
        return Mathf.Clamp(ortho, minOrthoSize, maxOrthoSize);
    }
}
