using Core.StateSync;
using Core.Maps;
using UnityEngine;

namespace Networking.StateSync
{
    /// <summary>
    /// Client-side scene builder for the replicated arena.
    ///
    /// The builder listens to MapConfigData (sent by the server) and creates:
    /// - a floor mesh
    /// - a border outline
    /// - an orthographic top-down camera (if one exists in the scene)
    /// - grid cell GameObjects
    /// - static game elements
    /// </summary>
    public class MapConfigSceneBuilder : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the MapConfigSceneBuilder.
        /// </summary>
        public static MapConfigSceneBuilder Instance { get; private set; }

        [Header("Visual Settings")]
        [SerializeField] private float borderWidth = 0.1f;
        [SerializeField] private int circleBorderSegments = 96;

        [Header("Static Elements")]
        [SerializeField] private GameElementLibrary gameElementLibrary;

        /// <summary>
        /// The current map configuration data received from the server.
        /// </summary>
        private MapConfigData currentConfig;

        /// <summary>
        /// The current grid data for the map.
        /// </summary>
        private GridMapData currentGrid;

        /// <summary>
        /// Root GameObject that contains all scene elements (floor, border, grid cells).
        /// </summary>
        private GameObject sceneRoot;

        /// <summary>
        /// Ensures that a singleton instance exists, creating one if necessary.
        /// </summary>
        /// <returns>The singleton instance of MapConfigSceneBuilder.</returns>
        public static MapConfigSceneBuilder EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("MapConfigSceneBuilder");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MapConfigSceneBuilder>();
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

            GameCommandClient.OnMapConfigApplied += HandleMapConfigApplied;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameCommandClient.OnMapConfigApplied -= HandleMapConfigApplied;
            }
        }

        private void HandleMapConfigApplied(MapConfigData config)
        {
            currentConfig = config;
            BuildScene(config);
        }

        private void BuildScene(MapConfigData config)
        {
            CleanupScene();

            if (config == null)
                return;

            sceneRoot = new GameObject("MapConfigSceneRoot");
            sceneRoot.transform.position = config.worldOffset;

            var gridWidth = ResolveGridWidth(config);
            var gridHeight = ResolveGridHeight(config);
            var cellSize = Mathf.Max(0.01f, config.cellSize);
            currentGrid = GridMapRepository.LoadOrCreateFallback(config.mapName, gridWidth, gridHeight, cellSize, config.worldOffset);
            if (config.shape == MapShape.Circle)
            {
                GridMapUtils.ApplyCircleMask(currentGrid, config.circleRadius);
            }

            switch (config.shape)
            {
                case MapShape.Circle:
                    BuildCircleArena(config);
                    break;
                case MapShape.Rect:
                default:
                    BuildRectArena(config);
                    break;
            }

            BuildGridCells(config, currentGrid);
            BuildStaticGameElements(config, currentGrid);
#if !UNITY_SERVER
            GridMapUIController.EnsureInstance().Render(currentGrid);
#endif

            SetupCamera(config);
        }

        private void BuildRectArena(MapConfigData config)
        {
            float width = Mathf.Abs(config.mapSize.x);
            float height = Mathf.Abs(config.mapSize.z);
            if (Mathf.Approximately(height, 0f))
                height = Mathf.Abs(config.mapSize.y);

            // Floor (Plane is 10x10 in Unity units)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(sceneRoot.transform, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(width / 10f, 1f, height / 10f);

            // Remove collider for visuals-only
            var collider = floor.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            // Border (LineRenderer)
            var border = new GameObject("Border");
            border.transform.SetParent(sceneRoot.transform, false);
            var lr = border.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 4;
            lr.startWidth = borderWidth;
            lr.endWidth = borderWidth;
            lr.material = new Material(Shader.Find("Sprites/Default"));

            float halfW = width / 2f;
            float halfH = height / 2f;
            lr.SetPosition(0, new Vector3(-halfW, 0.02f, -halfH));
            lr.SetPosition(1, new Vector3(halfW, 0.02f, -halfH));
            lr.SetPosition(2, new Vector3(halfW, 0.02f, halfH));
            lr.SetPosition(3, new Vector3(-halfW, 0.02f, halfH));
        }

        private void BuildCircleArena(MapConfigData config)
        {
            float radius = Mathf.Max(0.1f, config.circleRadius);

            // Floor disc
            var floor = new GameObject("Floor");
            floor.transform.SetParent(sceneRoot.transform, false);
            floor.transform.localPosition = Vector3.zero;
            var mf = floor.AddComponent<MeshFilter>();
            var mr = floor.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mf.mesh = CreateDiscMesh(radius, Mathf.Max(32, circleBorderSegments));

            // Border circle (LineRenderer)
            var border = new GameObject("Border");
            border.transform.SetParent(sceneRoot.transform, false);
            var lr = border.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            int segments = Mathf.Max(32, circleBorderSegments);
            lr.positionCount = segments;
            lr.startWidth = borderWidth;
            lr.endWidth = borderWidth;
            lr.material = new Material(Shader.Find("Sprites/Default"));

            for (int i = 0; i < segments; i++)
            {
                float t = (i / (float)segments) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(t) * radius, 0.02f, Mathf.Sin(t) * radius));
            }
        }

        private void BuildGridCells(MapConfigData config, GridMapData gridData)
        {
            if (config == null || gridData == null)
                return;

            var gridRoot = new GameObject("GridCells");
            gridRoot.transform.SetParent(sceneRoot.transform, false);

            int width = Mathf.Max(1, gridData.config.width);
            int height = Mathf.Max(1, gridData.config.height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cellGo = new GameObject($"Cell_{x}_{y}");
                    cellGo.transform.SetParent(gridRoot.transform, false);

                    var worldPos = GridMapUtils.CellToWorld(config, x, y);
                    cellGo.transform.localPosition = worldPos - config.worldOffset;

                    var cell = cellGo.AddComponent<GridCell>();
                    cell.Initialize(x, y, gridData.GetCellData(x, y));
                }
            }
        }

        private void BuildStaticGameElements(MapConfigData config, GridMapData gridData)
        {
            if (config == null || gridData == null || sceneRoot == null)
                return;

            var elements = gridData.gameElements;
            if (elements == null || elements.Length == 0 || gameElementLibrary == null)
                return;

            var root = new GameObject("StaticElements");
            root.transform.SetParent(sceneRoot.transform, false);

            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                if (string.IsNullOrEmpty(element.id))
                    continue;

                var definition = gameElementLibrary.GetById(element.id);
                if (definition == null || definition.Prefab == null)
                    continue;

                var worldPos = GetElementWorldCenter(config, gridData, element);
                var instance = Instantiate(definition.Prefab, root.transform);
                instance.name = $"{element.id}_{i}";
                instance.transform.localPosition = worldPos - config.worldOffset + definition.LocalOffset;
                instance.transform.localRotation = Quaternion.Euler(definition.LocalEulerAngles);
                instance.transform.localScale = definition.LocalScale;
            }
        }

        private static Vector3 GetElementWorldCenter(MapConfigData config, GridMapData gridData, GameElementData element)
        {
            if (element.cells == null || element.cells.Length == 0)
            {
                return config != null ? config.worldOffset : Vector3.zero;
            }

            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int i = 0; i < element.cells.Length; i++)
            {
                var cell = element.cells[i];
                if (!gridData.InBounds(cell.x, cell.y))
                    continue;

                sum += GridMapUtils.CellToWorld(config, cell.x, cell.y);
                count++;
            }

            if (count == 0)
            {
                return config != null ? config.worldOffset : Vector3.zero;
            }

            return sum / count;
        }

        private static int ResolveGridWidth(MapConfigData config)
        {
            if (config.gridWidth > 0)
                return config.gridWidth;

            float cellSize = Mathf.Max(0.01f, config.cellSize);
            float width = Mathf.Abs(config.mapSize.x);
            return Mathf.Max(1, Mathf.RoundToInt(width / cellSize));
        }

        private static int ResolveGridHeight(MapConfigData config)
        {
            if (config.gridHeight > 0)
                return config.gridHeight;

            float cellSize = Mathf.Max(0.01f, config.cellSize);
            float height = Mathf.Abs(config.mapSize.z);
            if (Mathf.Approximately(height, 0f))
                height = Mathf.Abs(config.mapSize.y);
            return Mathf.Max(1, Mathf.RoundToInt(height / cellSize));
        }

        private static Mesh CreateDiscMesh(float radius, int segments)
        {
            var mesh = new Mesh();
            mesh.name = "Disc";

            var vertices = new Vector3[segments + 1];
            var triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float t = (i / (float)segments) * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
            }

            for (int i = 0; i < segments; i++)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = (i + 1) % segments + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void SetupCamera(MapConfigData config)
        {
            var camera = Camera.main;
            if (camera == null)
                return;

#if !UNITY_SERVER
            var legacyFollow = camera.GetComponent<CameraFollowPawn>();
            if (legacyFollow != null)
                legacyFollow.enabled = false;

            camera.orthographic = true;
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var smart = camera.GetComponent<SmartArenaCamera>();
            if (smart == null)
            {
                smart = camera.gameObject.AddComponent<SmartArenaCamera>();
            }

            smart.ApplyMapConfig(config);
#endif
        }

        public void CleanupScene()
        {
            if (sceneRoot != null)
            {
                Destroy(sceneRoot);
                sceneRoot = null;
            }

            currentGrid = null;
#if !UNITY_SERVER
            if (GridMapUIController.Instance != null)
            {
                GridMapUIController.Instance.Clear();
            }
#endif
        }

        public MapConfigData GetCurrentConfig() => currentConfig;
        public GridMapData GetCurrentGrid() => currentGrid;
    }
}
