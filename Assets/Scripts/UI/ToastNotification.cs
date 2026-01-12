using Core.Patterns;
using Core.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Toast notification system for user feedback.
/// Displays temporary messages at the top of the screen using Singleton pattern.
/// </summary>
public class ToastNotification : Singleton<ToastNotification>
{
    public enum ToastType { Info, Success, Warning, Error }

    private VisualElement _toastContainer;
    private UIDocument _uiDocument;

    public static void Show(string message, ToastType type = ToastType.Info, float duration = 3f)
    {
        if (!HasInstance)
        {
            NetworkLogger.Warning("ToastNotification", "Instance not available, cannot show toast");
            return;
        }

        Instance.ShowToastInternal(message, type, duration);
    }

    protected override void OnInitialize()
    {
        InitializeUI();
        NetworkLogger.Info("ToastNotification", "Toast system initialized");
    }

    protected override void OnCleanup()
    {
        if (_toastContainer != null)
        {
            _toastContainer.Clear();
            _toastContainer = null;
        }

        if (_uiDocument != null)
        {
            Destroy(_uiDocument);
            _uiDocument = null;
        }

        NetworkLogger.Info("ToastNotification", "Toast system cleaned up");
    }

    private void InitializeUI()
    {
        // Create UIDocument
        _uiDocument = gameObject.AddComponent<UIDocument>();

        // Create root container
        var root = new VisualElement();
        root.style.position = Position.Absolute;
        root.style.top = 0;
        root.style.left = 0;
        root.style.right = 0;
        root.style.bottom = 0;
        root.pickingMode = PickingMode.Ignore; // Don't block clicks

        // Create toast container at top
        _toastContainer = new VisualElement();
        _toastContainer.name = "toast-container";
        _toastContainer.style.position = Position.Absolute;
        _toastContainer.style.top = 20;
        _toastContainer.style.left = Length.Percent(50);
        _toastContainer.style.translate = new Translate(Length.Percent(-50), 0);
        _toastContainer.style.minWidth = 300;
        _toastContainer.style.maxWidth = 600;
        _toastContainer.style.flexDirection = FlexDirection.Column;
        _toastContainer.style.alignItems = Align.Center;
        _toastContainer.pickingMode = PickingMode.Ignore;

        root.Add(_toastContainer);
        _uiDocument.rootVisualElement.Add(root);
    }

    private void ShowToastInternal(string message, ToastType type, float duration)
    {
        if (_toastContainer == null)
        {
            NetworkLogger.Warning("ToastNotification", "Container not initialized");
            return;
        }

        var toast = CreateToastElement(message, type);
        _toastContainer.Add(toast);

        StartCoroutine(AnimateToast(toast, duration));

        NetworkLogger.DebugLog("ToastNotification", $"Showing {type} toast: {message}");
    }

    private VisualElement CreateToastElement(string message, ToastType type)
    {
        var toast = new VisualElement();
        toast.name = "toast";

        // Base styling
        toast.style.backgroundColor = GetBackgroundColor(type);
        toast.style.borderTopLeftRadius = 8;
        toast.style.borderTopRightRadius = 8;
        toast.style.borderBottomLeftRadius = 8;
        toast.style.borderBottomRightRadius = 8;
        toast.style.paddingTop = 12;
        toast.style.paddingBottom = 12;
        toast.style.paddingLeft = 16;
        toast.style.paddingRight = 16;
        toast.style.marginBottom = 8;
        toast.style.minWidth = 300;
        toast.style.maxWidth = 600;

        // Icon + message container
        var content = new VisualElement();
        content.style.flexDirection = FlexDirection.Row;
        content.style.alignItems = Align.Center;

        // Icon
        var icon = new Label(GetIcon(type));
        icon.style.fontSize = 20;
        icon.style.marginRight = 8;
        icon.style.color = Color.white;

        // Message
        var label = new Label(message);
        label.style.color = Color.white;
        label.style.fontSize = 14;
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.flexGrow = 1;

        content.Add(icon);
        content.Add(label);
        toast.Add(content);

        // Shadow effect
        toast.style.unityBackgroundImageTintColor = new Color(0, 0, 0, 0.5f);

        return toast;
    }

    private Color GetBackgroundColor(ToastType type)
    {
        switch (type)
        {
            case ToastType.Success:
                return new Color(0.2f, 0.7f, 0.3f, 0.95f); // Green
            case ToastType.Warning:
                return new Color(1f, 0.7f, 0.2f, 0.95f); // Orange
            case ToastType.Error:
                return new Color(0.8f, 0.2f, 0.2f, 0.95f); // Red
            case ToastType.Info:
            default:
                return new Color(0.2f, 0.5f, 0.8f, 0.95f); // Blue
        }
    }

    private string GetIcon(ToastType type)
    {
        switch (type)
        {
            case ToastType.Success:
                return "✓";
            case ToastType.Warning:
                return "⚠";
            case ToastType.Error:
                return "✗";
            case ToastType.Info:
            default:
                return "ℹ";
        }
    }

    private IEnumerator AnimateToast(VisualElement toast, float duration)
    {
        // Initial state - above screen
        toast.style.translate = new Translate(0, -100);
        toast.style.opacity = 0;

        // Slide in
        float slideTime = 0.3f;
        float elapsed = 0;

        while (elapsed < slideTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideTime;
            float eased = EaseOutBack(t);

            toast.style.translate = new Translate(0, Mathf.Lerp(-100, 0, eased));
            toast.style.opacity = t;

            yield return null;
        }

        toast.style.translate = new Translate(0, 0);
        toast.style.opacity = 1;

        // Wait
        yield return new WaitForSeconds(duration);

        // Fade out
        float fadeTime = 0.3f;
        elapsed = 0;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            toast.style.opacity = 1 - t;
            toast.style.translate = new Translate(0, Mathf.Lerp(0, -50, t));

            yield return null;
        }

        // Remove
        toast.RemoveFromHierarchy();
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

}
