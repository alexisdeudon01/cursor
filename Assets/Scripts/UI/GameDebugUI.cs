using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// In-game debug UI showing player position and controls info.
/// Uses IMGUI for simplicity - toggled with F1.
/// </summary>
public class GameDebugUI : MonoBehaviour
{
    public static GameDebugUI Instance { get; private set; }

    private bool showUI = false;
    private Vector3 lastKnownPosition = Vector3.zero;
    private string sessionName = "";
    private ulong clientId = 0;

    private bool guiInitialized;
    private Texture2D bgTexture;
    private Texture2D borderTexture;
    private Texture2D buttonNormalTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D buttonActiveTexture;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle valueStyle;
    private GUIStyle controlsStyle;
    private GUIStyle leaveButtonStyle;

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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        DestroyGuiResources();
    }

    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("GameDebugUI");
            go.AddComponent<GameDebugUI>();
        }
    }

    public void SetSession(string name)
    {
        sessionName = name;
    }

    public void SetClientId(ulong id)
    {
        clientId = id;
    }

    public void UpdatePosition(Vector3 position)
    {
        lastKnownPosition = position;
    }

    public void Show()
    {
        showUI = true;
    }

    public void Hide()
    {
        showUI = false;
    }

    private void Update()
    {
        // Toggle debug UI with F1 (disabled by default so it doesn't cover gameplay).
        if (IsTogglePressed())
        {
            showUI = !showUI;
        }
    }

    private static bool IsTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.f1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F1);
#endif
    }

    private void OnGUI()
    {
        if (!showUI) return;

        // Only show when connected as client
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            return;

        EnsureGuiResources();

        // Modern dark theme colors
        // Draw the debug panel at bottom-left
        float width = 300f;
        float height = 200f;
        float margin = 15f;
        Rect panelRect = new Rect(margin, Screen.height - height - margin, width, height);

        // Draw border
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, panelRect.height + 4), borderTexture);
        
        GUI.Box(panelRect, "", boxStyle);

        GUILayout.BeginArea(new Rect(panelRect.x + 12, panelRect.y + 12, panelRect.width - 24, panelRect.height - 24));

        GUILayout.Label("IN-GAME", headerStyle);
        GUILayout.Space(8);

        if (!string.IsNullOrEmpty(sessionName))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Session:", subHeaderStyle, GUILayout.Width(70));
            GUILayout.Label(sessionName, valueStyle);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Client ID:", subHeaderStyle, GUILayout.Width(70));
        GUILayout.Label(clientId.ToString(), valueStyle);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Position:", subHeaderStyle, GUILayout.Width(70));
        GUILayout.Label($"({lastKnownPosition.x:F1}, {lastKnownPosition.y:F1}, {lastKnownPosition.z:F1})", valueStyle);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Arrow Keys / WASD / ZQSD", controlsStyle);

        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Leave Game", leaveButtonStyle, GUILayout.Height(36)))
        {
            OnLeaveGameClicked();
        }

        GUILayout.EndArea();
    }

    private void OnLeaveGameClicked()
    {
        Debug.Log("[GameDebugUI] Leave Game button clicked");
        
        if (SessionLobbyUI.Instance != null)
        {
            SessionLobbyUI.Instance.LeaveCurrentGame();
        }
        else
        {
            // Fallback when Client scene is unloaded while in-game.
            StartCoroutine(LeaveGameFallback());
        }
    }

    private IEnumerator LeaveGameFallback()
    {
        // Best-effort: leave session on server so pawns are despawned.
        if (!string.IsNullOrEmpty(sessionName) &&
            SessionRpcHub.Instance != null &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsClient)
        {
            SessionRpcHub.Instance.LeaveSessionServerRpc(sessionName);
        }

        yield return null;

        // Return to client lobby UI.
        SceneManager.LoadScene(SceneNames.Client);
    }

    private void EnsureGuiResources()
    {
        if (guiInitialized)
        {
            return;
        }

        // Colors
        var bgColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        var borderColor = new Color(0.4f, 0.5f, 0.7f, 1f);
        var accentBlue = new Color(0.3f, 0.55f, 0.9f, 1f);
        var accentGreen = new Color(0.3f, 0.75f, 0.45f, 1f);
        var textSecondary = new Color(0.7f, 0.7f, 0.75f, 1f);
        var textMuted = new Color(0.5f, 0.55f, 0.65f, 1f);

        bgTexture = CreateTexture(2, 2, bgColor);
        borderTexture = CreateTexture(2, 2, borderColor);
        buttonNormalTexture = CreateTexture(2, 2, new Color(0.7f, 0.28f, 0.28f, 1f));
        buttonHoverTexture = CreateTexture(2, 2, new Color(0.8f, 0.35f, 0.35f, 1f));
        buttonActiveTexture = CreateTexture(2, 2, new Color(0.6f, 0.22f, 0.22f, 1f));

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 12, 12)
        };
        boxStyle.normal.background = bgTexture;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 14;

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.normal.textColor = accentBlue;
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;

        subHeaderStyle = new GUIStyle(GUI.skin.label);
        subHeaderStyle.normal.textColor = textSecondary;
        subHeaderStyle.fontSize = 13;

        valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.normal.textColor = accentGreen;
        valueStyle.fontSize = 14;
        valueStyle.fontStyle = FontStyle.Bold;

        controlsStyle = new GUIStyle(GUI.skin.label);
        controlsStyle.normal.textColor = textMuted;
        controlsStyle.fontSize = 11;

        leaveButtonStyle = new GUIStyle(GUI.skin.button);
        leaveButtonStyle.fontSize = 14;
        leaveButtonStyle.fontStyle = FontStyle.Bold;
        leaveButtonStyle.normal.textColor = Color.white;
        leaveButtonStyle.normal.background = buttonNormalTexture;
        leaveButtonStyle.hover.background = buttonHoverTexture;
        leaveButtonStyle.active.background = buttonActiveTexture;

        guiInitialized = true;
    }

    private void DestroyGuiResources()
    {
        guiInitialized = false;
        DestroyTexture(ref bgTexture);
        DestroyTexture(ref borderTexture);
        DestroyTexture(ref buttonNormalTexture);
        DestroyTexture(ref buttonHoverTexture);
        DestroyTexture(ref buttonActiveTexture);
    }

    private static void DestroyTexture(ref Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }

        Destroy(texture);
        texture = null;
    }

    private static Texture2D CreateTexture(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
        texture.hideFlags = HideFlags.DontSave;
        texture.SetPixels(pixels);
        texture.Apply(updateMipmaps: false);
        return texture;
    }
}
