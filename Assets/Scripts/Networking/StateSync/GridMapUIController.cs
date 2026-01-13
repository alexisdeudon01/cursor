using Core.Maps;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class GridMapUIController : MonoBehaviour
{
    public static GridMapUIController Instance { get; private set; }

    [SerializeField] private string uxmlResourcePath = "UI/GridMapOverlay";
    [SerializeField] private string ussResourcePath = "UI/GridMapOverlay";

    private UIDocument uiDocument;
    private VisualElement gridRoot;
    private GridMapData currentData;

    public static GridMapUIController EnsureInstance()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GridMapUI");
        Instance = go.AddComponent<GridMapUIController>();
        DontDestroyOnLoad(go);
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
        InitializeUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Render(GridMapData data)
    {
        currentData = data;
        BuildGrid();
    }

    public void Clear()
    {
        currentData = null;
        BuildGrid();
    }

    private void InitializeUI()
    {
        uiDocument = gameObject.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (uiDocument.panelSettings == null)
        {
            var settings = Resources.FindObjectsOfTypeAll<PanelSettings>();
            if (settings != null && settings.Length > 0)
            {
                uiDocument.panelSettings = settings[0];
            }
        }

        var root = uiDocument.rootVisualElement;
        root.Clear();

        var tree = Resources.Load<VisualTreeAsset>(uxmlResourcePath);
        if (tree != null)
        {
            tree.CloneTree(root);
        }
        else
        {
            var fallback = new VisualElement { name = "grid-root" };
            fallback.AddToClassList("grid-root");
            root.Add(fallback);
        }

        var sheet = Resources.Load<StyleSheet>(ussResourcePath);
        if (sheet != null)
        {
            root.styleSheets.Add(sheet);
        }

        gridRoot = root.Q<VisualElement>("grid-root");
        if (gridRoot == null)
        {
            gridRoot = root;
        }

        gridRoot.style.position = Position.Absolute;
        gridRoot.style.left = 0;
        gridRoot.style.right = 0;
        gridRoot.style.top = 0;
        gridRoot.style.bottom = 0;
        gridRoot.pickingMode = PickingMode.Ignore;
    }

    private void BuildGrid()
    {
        if (gridRoot == null)
            return;

        gridRoot.Clear();

        if (currentData == null)
            return;

        int width = Mathf.Max(1, currentData.config.width);
        int height = Mathf.Max(1, currentData.config.height);

        for (int y = height - 1; y >= 0; y--)
        {
            var row = new VisualElement();
            row.AddToClassList("grid-row");
            row.style.height = Length.Percent(100f / height);
            row.style.width = Length.Percent(100f);
            row.pickingMode = PickingMode.Ignore;

            for (int x = 0; x < width; x++)
            {
                var cell = new VisualElement();
                cell.AddToClassList("grid-cell");
                cell.style.width = Length.Percent(100f / width);
                cell.pickingMode = PickingMode.Ignore;

                var type = currentData.GetCell(x, y);
                switch (type)
                {
                    case GridCellType.Wall:
                        cell.AddToClassList("grid-cell--wall");
                        break;
                    case GridCellType.Spawn:
                        cell.AddToClassList("grid-cell--spawn");
                        break;
                    case GridCellType.Goal:
                        cell.AddToClassList("grid-cell--goal");
                        break;
                }

                cell.userData = new Vector2Int(x, y);
                row.Add(cell);
            }

            gridRoot.Add(row);
        }
    }
}
