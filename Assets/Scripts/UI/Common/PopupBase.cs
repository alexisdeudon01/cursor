using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Base class for all popup windows in the UI system.
/// Ensures only one popup is visible at a time and provides consistent styling.
/// </summary>
public abstract class PopupBase
{
    // Static tracking of active popup
    private static PopupBase activePopup;
    
    // UI Elements
    protected VisualElement overlay;
    protected VisualElement container;
    protected Label titleLabel;
    protected Button closeButton;
    
    // State
    public bool IsVisible => overlay?.style.display == DisplayStyle.Flex;
    public string Title { get; protected set; }
    
    /// <summary>
    /// Event fired when popup is closed
    /// </summary>
    public System.Action OnClosed;
    
    /// <summary>
    /// Creates the popup structure programmatically
    /// </summary>
    protected PopupBase(VisualElement parent, string title)
    {
        Title = title;
        CreateStructure(parent);
        ApplyStyles();
        Hide();
    }
    
    /// <summary>
    /// Attaches to existing UXML elements
    /// </summary>
    protected PopupBase(VisualElement existingOverlay, VisualElement existingContainer)
    {
        overlay = existingOverlay;
        container = existingContainer;
        Hide();
    }
    
    protected virtual void CreateStructure(VisualElement parent)
    {
        // Create overlay (darkened background)
        overlay = new VisualElement();
        overlay.name = "popup-overlay";
        overlay.AddToClassList("popup-overlay-base");
        
        // Create popup container
        container = new VisualElement();
        container.name = "popup-container";
        container.AddToClassList("popup-container-base");
        
        // Create header
        var header = new VisualElement();
        header.name = "popup-header";
        header.AddToClassList("popup-header-base");
        
        titleLabel = new Label(Title);
        titleLabel.name = "popup-title";
        titleLabel.AddToClassList("popup-title-base");
        
        closeButton = new Button(() => Hide());
        closeButton.name = "popup-close";
        closeButton.text = "✕";
        closeButton.AddToClassList("popup-close-base");
        
        header.Add(titleLabel);
        header.Add(closeButton);
        container.Add(header);
        
        // Create content area (to be filled by subclasses)
        var content = new VisualElement();
        content.name = "popup-content";
        content.AddToClassList("popup-content-base");
        container.Add(content);
        
        overlay.Add(container);
        parent.Add(overlay);
        
        // Click outside to close (optional)
        overlay.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == overlay)
                Hide();
        });
    }
    
    protected virtual void ApplyStyles()
    {
        // Overlay styles
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.75f);
        overlay.style.justifyContent = Justify.Center;
        overlay.style.alignItems = Align.Center;
        
        // Container styles
        container.style.backgroundColor = new Color(0.18f, 0.18f, 0.22f, 1f);
        container.style.borderTopLeftRadius = 12;
        container.style.borderTopRightRadius = 12;
        container.style.borderBottomLeftRadius = 12;
        container.style.borderBottomRightRadius = 12;
        container.style.paddingTop = 20;
        container.style.paddingBottom = 20;
        container.style.paddingLeft = 24;
        container.style.paddingRight = 24;
        container.style.minWidth = 400;
        container.style.maxWidth = 600;
        container.style.borderTopWidth = 2;
        container.style.borderBottomWidth = 2;
        container.style.borderLeftWidth = 2;
        container.style.borderRightWidth = 2;
        container.style.borderTopColor = new Color(0.4f, 0.5f, 0.7f, 1f);
        container.style.borderBottomColor = new Color(0.4f, 0.5f, 0.7f, 1f);
        container.style.borderLeftColor = new Color(0.4f, 0.5f, 0.7f, 1f);
        container.style.borderRightColor = new Color(0.4f, 0.5f, 0.7f, 1f);
    }
    
    /// <summary>
    /// Shows this popup, hiding any other active popup
    /// </summary>
    public virtual void Show()
    {
        // Hide any other active popup first
        if (activePopup != null && activePopup != this)
        {
            activePopup.Hide(false); // Don't trigger callback
        }
        
        activePopup = this;
        
        if (overlay != null)
        {
            overlay.style.display = DisplayStyle.Flex;
            overlay.BringToFront();
        }
        
        Debug.Log($"[PopupBase] Showing popup: {Title}");
    }
    
    /// <summary>
    /// Hides this popup
    /// </summary>
    public virtual void Hide(bool triggerCallback = true)
    {
        if (overlay != null)
        {
            overlay.style.display = DisplayStyle.None;
        }
        
        if (activePopup == this)
        {
            activePopup = null;
        }
        
        if (triggerCallback)
        {
            OnClosed?.Invoke();
        }
        
        Debug.Log($"[PopupBase] Hiding popup: {Title}");
    }
    
    /// <summary>
    /// Updates the title text
    /// </summary>
    public void SetTitle(string newTitle)
    {
        Title = newTitle;
        if (titleLabel != null)
        {
            titleLabel.text = newTitle;
        }
    }
    
    /// <summary>
    /// Gets the content area to add custom elements
    /// </summary>
    protected VisualElement GetContentArea()
    {
        return container?.Q<VisualElement>("popup-content");
    }
    
    /// <summary>
    /// Static method to hide all popups
    /// </summary>
    public static void HideAll()
    {
        if (activePopup != null)
        {
            activePopup.Hide();
        }
    }
    
    /// <summary>
    /// Static method to check if any popup is visible
    /// </summary>
    public static bool IsAnyPopupVisible => activePopup != null && activePopup.IsVisible;
}
