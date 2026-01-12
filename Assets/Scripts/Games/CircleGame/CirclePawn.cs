using TMPro;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Circle pawn view.
/// 
/// Data-oriented refactor (Option 1): this is a pure client-side visual.
/// The server simulates movement and replicates the transform via DTO commands.
/// </summary>
public class CirclePawn : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private int segments = 24;

    [SerializeField] private Color[] playerColors = new Color[]
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

    [Header("Name Display")]
    [SerializeField] private Vector3 nameOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float nameFontSize = 3f;
    [SerializeField] private Color nameColor = Color.white;

    public ulong OwnerClientId { get; private set; }
    public FixedString64Bytes PlayerName { get; private set; }
    public int ColorIndex { get; private set; }

    private MeshRenderer meshRenderer;
    private TextMeshPro nameLabel;

    private void Awake()
    {
        EnsureVisuals();
    }

    public void ApplySpawnData(ulong ownerClientId, FixedString64Bytes playerName, int colorIndex)
    {
        OwnerClientId = ownerClientId;
        PlayerName = playerName;
        ColorIndex = colorIndex;

        EnsureVisuals();
        ApplyColor();
        UpdateNameDisplay();
    }

    private void EnsureVisuals()
    {
        if (meshRenderer == null)
            CreateCircleVisual();

        if (nameLabel == null)
            CreateNameLabel();
    }

    private void CreateCircleVisual()
    {
        var circleObj = new GameObject("CircleVisual");
        circleObj.transform.SetParent(transform, false);
        circleObj.transform.localPosition = Vector3.up * 0.01f;

        var meshFilter = circleObj.AddComponent<MeshFilter>();
        meshRenderer = circleObj.AddComponent<MeshRenderer>();
        meshFilter.mesh = BuildCircleMesh(radius, Mathf.Max(3, segments));

        var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        meshRenderer.material = new Material(shader);
    }

    private static Mesh BuildCircleMesh(float radius, int segments)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void CreateNameLabel()
    {
        var nameObj = new GameObject("NameLabel");
        nameObj.transform.SetParent(transform, false);
        nameObj.transform.localPosition = nameOffset;

        nameLabel = nameObj.AddComponent<TextMeshPro>();
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.fontSize = nameFontSize;
        nameLabel.color = nameColor;
        nameLabel.sortingOrder = 10;

        UpdateNameDisplay();
    }

    private void ApplyColor()
    {
        if (meshRenderer == null)
            return;

        if (playerColors != null && playerColors.Length > 0)
        {
            var color = playerColors[Mathf.Abs(ColorIndex) % playerColors.Length];
            meshRenderer.material.color = color;
        }
    }

    private void UpdateNameDisplay()
    {
        if (nameLabel != null)
            nameLabel.text = PlayerName.ToString();
    }

    private void LateUpdate()
    {
        if (nameLabel != null && Camera.main != null)
        {
            var toCam = Camera.main.transform.position - nameLabel.transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
                nameLabel.transform.rotation = Quaternion.LookRotation(toCam);
        }
    }
}
