#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvasAutoSetup : EditorWindow
{
    [MenuItem("Tools/Auto-Setup Game Canvas")]
    public static void AutoSetupGameCanvas()
    {
        // 1. Crée ou trouve le GameCanvas
        GameObject gameCanvas = GameObject.Find("GameCanvas");
        if (gameCanvas == null)
        {
            gameCanvas = new GameObject("GameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = gameCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.AddComponent<GameCanvasRoot>();
        }

        // 2. Crée ou trouve le GameCanvasManager
        GameObject managerGO = GameObject.Find("GameCanvasManager");
        if (managerGO == null)
        {
            managerGO = new GameObject("GameCanvasManager");
            managerGO.AddComponent<GameCanvasManager>();
        }
        var manager = managerGO.GetComponent<GameCanvasManager>();
        // Assign private serialized field via SerializedObject (keeps GameCanvasManager encapsulated)
        {
            var soManager = new SerializedObject(manager);
            var propCanvas = soManager.FindProperty("gameCanvas");
            if (propCanvas != null)
            {
                propCanvas.objectReferenceValue = gameCanvas;
                soManager.ApplyModifiedProperties();
            }
        }

        // 3. Trouve l'objet avec SessionLobbyUI et référence le GameCanvasManager
        var lobbyUI = Object.FindFirstObjectByType<SessionLobbyUI>();
        if (lobbyUI != null)
        {
            var so = new SerializedObject(lobbyUI);
            var prop = so.FindProperty("gameCanvasManager");
            if (prop != null)
            {
                prop.objectReferenceValue = manager;
                so.ApplyModifiedProperties();
            }
        }

        EditorUtility.DisplayDialog("Game Canvas Auto-Setup", "GameCanvas, GameCanvasManager et la liaison avec SessionLobbyUI ont été configurés automatiquement.", "OK");
    }
}
#endif
