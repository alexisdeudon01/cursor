using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCanvasManager : MonoBehaviour
{
    public static GameCanvasManager Instance { get; private set; }

    [Tooltip("Optional explicit reference. If missing, the manager will auto-find a GameCanvasRoot in loaded scenes.")]
    [SerializeField] private GameObject gameCanvas;

    [Tooltip("Hide the game canvas when this manager starts.")]
    [SerializeField] private bool hideOnAwake = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        ResolveGameCanvas();
        if (hideOnAwake)
        {
            HideGameCanvas();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveGameCanvas();
    }

    private void ResolveGameCanvas()
    {
        if (gameCanvas == gameObject)
        {
            gameCanvas = null;
        }

        if (gameCanvas != null && gameCanvas.scene.IsValid())
        {
            return;
        }

        var roots = FindObjectsByType<GameCanvasRoot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (roots == null || roots.Length == 0)
        {
            return;
        }

        for (int i = 0; i < roots.Length; i++)
        {
            var root = roots[i];
            if (root != null && root.gameObject != null && root.gameObject.scene.IsValid())
            {
                gameCanvas = root.gameObject;
                return;
            }
        }
    }

    public void ShowGameCanvas()
    {
        ResolveGameCanvas();
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(true);
        }
    }

    public void HideGameCanvas()
    {
        ResolveGameCanvas();
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }
    }
}
