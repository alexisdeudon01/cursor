using Core.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkBootstrapProgressViewClient : MonoBehaviour, INetworkBootstrapProgressView
{
    [Header("UI Toolkit (Mode A: Clone UXML)")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset overlayUxml;
    [SerializeField] private StyleSheet overlayUss;

    [Header("Menu UI to hide/show")]
    [SerializeField] private string menuTag = "Menu";

    [Header("Behavior")]
    [SerializeField] private int maxEntries = 12;
    [SerializeField] private float fadeSeconds = 0.20f;

    private VisualElement overlayRoot;
    private Label titleLabel;
    private ProgressBar progressBar;
    private Label stepLabel;
    private Label subStepLabel;
    private Label errorLabel;
    private ScrollView logScroll;

    private UIDocument menuDoc;
    private VisualElement menuRoot;

    private bool initialized;
    private Coroutine hideRoutine;

    public bool Initialize()
    {
        if (initialized) return true;

        // Always bind to the UIDocument on THIS GameObject (prevents wrong references)
        if (uiDocument == null || uiDocument.gameObject != gameObject)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[BootstrapUI] UIDocument missing on this GameObject");
            return false;
        }

        var docRoot = uiDocument.rootVisualElement;
        if (docRoot == null)
        {
            Debug.LogError("[BootstrapUI] rootVisualElement is null (Panel Settings missing?)");
            return false;
        }

        // If already exists, reuse
        var existing = docRoot.Q<VisualElement>("network-bootstrap-root");
        if (existing != null)
        {
            overlayRoot = existing;
        }
        else
        {
            if (overlayUxml == null)
            {
                Debug.LogError("[BootstrapUI] overlayUxml missing (assign NetworkBootstrapOverlay.uxml)");
                return false;
            }

            overlayRoot = overlayUxml.CloneTree();
            overlayRoot.name = "network-bootstrap-root";

            docRoot.Add(overlayRoot);
        }

        if (overlayUss != null)
            overlayRoot.styleSheets.Add(overlayUss);

        // Bind elements (must match UXML names)
        titleLabel = overlayRoot.Q<Label>("bootstrap-title");
        progressBar = overlayRoot.Q<ProgressBar>("bootstrap-progress");
        stepLabel = overlayRoot.Q<Label>("bootstrap-step");
        subStepLabel = overlayRoot.Q<Label>("bootstrap-substep");
        errorLabel = overlayRoot.Q<Label>("bootstrap-error");
        logScroll = overlayRoot.Q<ScrollView>("bootstrap-log");

        // Start hidden and non-blocking
        overlayRoot.style.display = DisplayStyle.None;
        overlayRoot.pickingMode = PickingMode.Ignore;

        initialized = true;
        return true;
    }

    private void TryResolveMenuUi()
    {
        if (menuRoot != null) return;

        GameObject go = null;
        try { go = GameObject.FindWithTag(menuTag); }
        catch { /* tag not defined */ }

        if (go == null) return;

        menuDoc = go.GetComponent<UIDocument>();
        if (menuDoc == null) return;

        menuRoot = menuDoc.rootVisualElement;
    }

    private void HideMenuUi()
    {
        TryResolveMenuUi();
        if (menuRoot == null) return;

        menuRoot.style.display = DisplayStyle.None;
        menuRoot.pickingMode = PickingMode.Ignore;
    }

    private void ShowMenuUi()
    {
        TryResolveMenuUi();
        if (menuRoot == null) return;

        menuRoot.style.display = DisplayStyle.Flex;
        menuRoot.pickingMode = PickingMode.Position;
    }

    public void Show()
    {
        enabled = true;
        if (!Initialize()) return;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        HideMenuUi();

        // Show overlay + BLOCK clicks behind
        overlayRoot.style.display = DisplayStyle.Flex;
        overlayRoot.pickingMode = PickingMode.Position;

        // Make sure visible even if USS fade is wrong
        overlayRoot.style.opacity = 1f;

        Debug.Log("[BootstrapUI] SHOW");
    }

    public void Hide()
    {
        if (!Initialize()) return;

        // Menu back immediately
        ShowMenuUi();

        // Stop blocking clicks immediately
        overlayRoot.pickingMode = PickingMode.Ignore;

        // Fade out (or immediate if no USS transition)
        overlayRoot.style.opacity = 0f;

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterFade());

        Debug.Log("[BootstrapUI] HIDE");
    }

    private IEnumerator HideAfterFade()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, fadeSeconds));

        if (overlayRoot != null)
            overlayRoot.style.display = DisplayStyle.None;

        hideRoutine = null;

        // Requirement: disable this script
        enabled = false;
    }

    private void OnDisable()
    {
        // Safety: never leave menu hidden
        ShowMenuUi();

        if (overlayRoot != null)
        {
            overlayRoot.pickingMode = PickingMode.Ignore;
            overlayRoot.style.display = DisplayStyle.None;
            overlayRoot.style.opacity = 0f;
        }
    }

    // --------- Called by NetworkBootstrap ----------

    public void SetProgress(float progressPercent, string step, string subStep)
    {
        if (!Initialize()) return;

        if (progressBar != null)
        {
            progressBar.value = progressPercent;
            progressBar.title = ((int)progressPercent) + "%";
        }

        if (stepLabel != null) stepLabel.text = step;
        if (subStepLabel != null) subStepLabel.text = subStep;
    }

    public void AddEntry(string message, bool isError)
    {
        if (!Initialize() || logScroll == null) return;

        var entry = new Label(message);
        if (isError) entry.style.color = new Color(1f, 0.5f, 0.5f, 1f);

        logScroll.Add(entry);

        if (maxEntries > 0)
        {
            while (logScroll.childCount > maxEntries)
                logScroll.RemoveAt(0);
        }
    }

    public void SetError(string message)
    {
        if (!Initialize() || errorLabel == null) return;

        errorLabel.text = message;
        errorLabel.style.display = DisplayStyle.Flex;
    }

    public void ClearError()
    {
        if (!Initialize() || errorLabel == null) return;

        errorLabel.text = "";
        errorLabel.style.display = DisplayStyle.None;
    }
}
