using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Progress indicator for game loading and startup operations.
/// Shows a progress bar with percentage and status message.
/// </summary>
public class ProgressIndicator : MonoBehaviour
{
    private static ProgressIndicator _instance;
    private UIDocument _uiDocument;
    private VisualElement _overlay;
    private VisualElement _progressContainer;
    private VisualElement _progressBar;
    private VisualElement _progressFill;
    private Label _progressLabel;
    private Label _progressDetails;
    private bool _isVisible;
    
    public static ProgressIndicator Instance => _instance;
    
    public static void Show(string title = "Chargement", float initialProgress = 0f)
    {
        var instance = FindOrCreateInstance();
        instance?.ShowInternal(title, initialProgress);
    }
    
    public static void UpdateProgress(float progress, string message = "")
    {
        if (_instance != null)
        {
            _instance.UpdateProgressInternal(progress, message);
        }
    }
    
    public static void Hide()
    {
        if (_instance != null)
        {
            _instance.HideInternal();
        }
    }
    
    private static ProgressIndicator FindOrCreateInstance()
    {
        if (_instance == null)
        {
            var go = new GameObject("ProgressIndicator");
            _instance = go.AddComponent<ProgressIndicator>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        _uiDocument = gameObject.AddComponent<UIDocument>();
        
        // Overlay (fullscreen dark background)
        _overlay = new VisualElement();
        _overlay.name = "progress-overlay";
        _overlay.style.position = Position.Absolute;
        _overlay.style.top = 0;
        _overlay.style.left = 0;
        _overlay.style.right = 0;
        _overlay.style.bottom = 0;
        _overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        _overlay.style.justifyContent = Justify.Center;
        _overlay.style.alignItems = Align.Center;
        _overlay.style.display = DisplayStyle.None;
        
        // Progress container
        _progressContainer = new VisualElement();
        _progressContainer.name = "progress-container";
        _progressContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        _progressContainer.style.borderTopLeftRadius = 12;
        _progressContainer.style.borderTopRightRadius = 12;
        _progressContainer.style.borderBottomLeftRadius = 12;
        _progressContainer.style.borderBottomRightRadius = 12;
        _progressContainer.style.paddingTop = 24;
        _progressContainer.style.paddingBottom = 24;
        _progressContainer.style.paddingLeft = 32;
        _progressContainer.style.paddingRight = 32;
        _progressContainer.style.minWidth = 400;
        _progressContainer.style.maxWidth = 600;
        
        // Title label
        _progressLabel = new Label();
        _progressLabel.name = "progress-label";
        _progressLabel.style.fontSize = 18;
        _progressLabel.style.color = Color.white;
        _progressLabel.style.marginBottom = 16;
        _progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _progressLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        // Progress bar background
        _progressBar = new VisualElement();
        _progressBar.name = "progress-bar";
        _progressBar.style.height = 8;
        _progressBar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        _progressBar.style.borderTopLeftRadius = 4;
        _progressBar.style.borderTopRightRadius = 4;
        _progressBar.style.borderBottomLeftRadius = 4;
        _progressBar.style.borderBottomRightRadius = 4;
        _progressBar.style.overflow = Overflow.Hidden;
        _progressBar.style.marginBottom = 12;
        
        // Progress bar fill
        _progressFill = new VisualElement();
        _progressFill.name = "progress-fill";
        _progressFill.style.height = Length.Percent(100);
        _progressFill.style.width = Length.Percent(0);
        _progressFill.style.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
        _progressFill.style.borderTopLeftRadius = 4;
        _progressFill.style.borderTopRightRadius = 4;
        _progressFill.style.borderBottomLeftRadius = 4;
        _progressFill.style.borderBottomRightRadius = 4;
        
        // Details label
        _progressDetails = new Label();
        _progressDetails.name = "progress-details";
        _progressDetails.style.fontSize = 14;
        _progressDetails.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        _progressDetails.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // Assemble hierarchy
        _progressBar.Add(_progressFill);
        _progressContainer.Add(_progressLabel);
        _progressContainer.Add(_progressBar);
        _progressContainer.Add(_progressDetails);
        _overlay.Add(_progressContainer);
        _uiDocument.rootVisualElement.Add(_overlay);
    }
    
    private void ShowInternal(string title, float progress)
    {
        if (_overlay == null) return;
        
        _isVisible = true;
        _overlay.style.display = DisplayStyle.Flex;
        
        if (_progressLabel != null)
        {
            _progressLabel.text = title;
        }
        
        UpdateProgressInternal(progress, "");
        
        // Fade in animation
        StartCoroutine(FadeIn());
    }
    
    private void UpdateProgressInternal(float progress, string message)
    {
        progress = Mathf.Clamp01(progress);
        
        if (_progressFill != null)
        {
            _progressFill.style.width = Length.Percent(progress * 100);
        }
        
        if (_progressDetails != null)
        {
            if (string.IsNullOrEmpty(message))
            {
                _progressDetails.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            else
            {
                _progressDetails.text = $"{Mathf.RoundToInt(progress * 100)}% - {message}";
            }
        }
        
        // Change color based on progress
        if (_progressFill != null)
        {
            if (progress >= 1f)
            {
                _progressFill.style.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 1f); // Green
            }
            else if (progress >= 0.5f)
            {
                _progressFill.style.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
            }
            else
            {
                _progressFill.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
            }
        }
    }
    
    private void HideInternal()
    {
        if (!_isVisible) return;
        
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        if (_overlay == null) yield break;
        
        _overlay.style.opacity = 0;
        float duration = 0.3f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _overlay.style.opacity = elapsed / duration;
            yield return null;
        }
        
        _overlay.style.opacity = 1;
    }
    
    private IEnumerator FadeOut()
    {
        if (_overlay == null) yield break;
        
        float duration = 0.3f;
        float elapsed = 0;
        float startOpacity = _overlay.style.opacity.value;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _overlay.style.opacity = startOpacity * (1 - elapsed / duration);
            yield return null;
        }
        
        _overlay.style.opacity = 0;
        _overlay.style.display = DisplayStyle.None;
        _isVisible = false;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
