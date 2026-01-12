using TMPro;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Square pawn view (CLIENT ONLY).
///
/// Dedicated server + full authoritative:
/// - The server simulates movement.
/// - The client renders a lightweight view for each replicated entity.
/// 
/// Requirement: Square should display ONLY a name label (child).
/// </summary>
public class PlayerPawn : MonoBehaviour
{
    public ulong OwnerClientId { get; private set; }
    public FixedString64Bytes PlayerName { get; private set; }
    public int ColorIndex { get; private set; }

    [Header("Name Label")]
    [SerializeField] private Vector3 nameOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float nameFontSize = 3f;

    [Header("Follow / Facing")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private float squareSize = 0.9f;

    private TextMeshPro nameLabel;
    private MeshRenderer meshRenderer;

    private readonly Color[] playerColors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        new Color(1f, 0.5f, 0f),
        Color.white
    };

    private void Awake()
    {
        EnsureVisuals();
    }

    private void LateUpdate()
    {
        if (faceCamera && nameLabel != null && Camera.main != null)
        {
            // Billboard so text faces the camera.
            var cam = Camera.main.transform;
            var toCam = cam.position - nameLabel.transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
                nameLabel.transform.rotation = Quaternion.LookRotation(toCam, cam.up);
        }
    }

    public void ApplySpawnData(ulong ownerClientId, FixedString64Bytes playerName, int colorIndex)
    {
        OwnerClientId = ownerClientId;
        PlayerName = playerName;
        ColorIndex = colorIndex;

        EnsureVisuals();
        ApplyColor();
        UpdateLabel();
    }

    private void EnsureVisuals()
    {
        if (meshRenderer == null)
            CreateSquareVisual();
        if (nameLabel == null)
            CreateLabel();
    }

    private void CreateSquareVisual()
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "SquareVisual";
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = Vector3.up * 0.01f;
        quad.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        quad.transform.localScale = new Vector3(squareSize, squareSize, 1f);

        var collider = quad.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        meshRenderer = quad.GetComponent<MeshRenderer>();
        var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        meshRenderer.material = new Material(shader);
    }

    private void CreateLabel()
    {
        if (nameLabel != null)
            return;

        // Label is a child of the Square root.
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(transform, false);
        labelGO.transform.localPosition = nameOffset;

        nameLabel = labelGO.AddComponent<TextMeshPro>();
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.fontSize = nameFontSize;
        nameLabel.textWrappingMode = TextWrappingModes.NoWrap;
        nameLabel.text = "";
    }

    private void ApplyColor()
    {
        if (meshRenderer == null)
            return;

        int idx = Mathf.Abs(ColorIndex) % playerColors.Length;
        meshRenderer.material.color = playerColors[idx];
    }

    private void UpdateLabel()
    {
        if (nameLabel == null)
            return;

        nameLabel.text = PlayerName.IsEmpty ? "Player" : PlayerName.ToString();

        int idx = Mathf.Abs(ColorIndex) % playerColors.Length;
        nameLabel.color = playerColors[idx];
    }
}
