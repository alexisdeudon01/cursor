using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Confirmation popup with Yes/No or custom buttons
/// </summary>
public class ConfirmPopup : PopupBase
{
    private Label messageLabel;
    private Button confirmButton;
    private Button cancelButton;
    
    public System.Action OnConfirm;
    public System.Action OnCancel;
    
    public ConfirmPopup(VisualElement parent, string title, string message, string confirmText = "Confirm", string cancelText = "Cancel") 
        : base(parent, title)
    {
        CreateConfirmContent(message, confirmText, cancelText);
    }
    
    private void CreateConfirmContent(string message, string confirmText, string cancelText)
    {
        var content = GetContentArea();
        if (content == null) return;
        
        // Message label
        messageLabel = new Label(message);
        messageLabel.name = "popup-message";
        messageLabel.style.color = new Color(0.85f, 0.85f, 0.9f, 1f);
        messageLabel.style.marginBottom = 24;
        messageLabel.style.fontSize = 15;
        messageLabel.style.whiteSpace = WhiteSpace.Normal;
        content.Add(messageLabel);
        
        // Buttons container
        var buttonsContainer = new VisualElement();
        buttonsContainer.style.flexDirection = FlexDirection.Row;
        buttonsContainer.style.justifyContent = Justify.FlexEnd;
        
        // Cancel button
        cancelButton = new Button(OnCancelClicked);
        cancelButton.text = cancelText;
        cancelButton.style.paddingTop = 12;
        cancelButton.style.paddingBottom = 12;
        cancelButton.style.paddingLeft = 28;
        cancelButton.style.paddingRight = 28;
        cancelButton.style.marginRight = 12;
        cancelButton.style.backgroundColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        cancelButton.style.color = Color.white;
        cancelButton.style.borderTopLeftRadius = 6;
        cancelButton.style.borderTopRightRadius = 6;
        cancelButton.style.borderBottomLeftRadius = 6;
        cancelButton.style.borderBottomRightRadius = 6;
        cancelButton.style.borderTopWidth = 0;
        cancelButton.style.borderBottomWidth = 0;
        cancelButton.style.borderLeftWidth = 0;
        cancelButton.style.borderRightWidth = 0;
        cancelButton.style.fontSize = 14;
        buttonsContainer.Add(cancelButton);
        
        // Confirm button
        confirmButton = new Button(OnConfirmClicked);
        confirmButton.text = confirmText;
        confirmButton.style.paddingTop = 12;
        confirmButton.style.paddingBottom = 12;
        confirmButton.style.paddingLeft = 28;
        confirmButton.style.paddingRight = 28;
        confirmButton.style.backgroundColor = new Color(0.75f, 0.3f, 0.3f, 1f);
        confirmButton.style.color = Color.white;
        confirmButton.style.borderTopLeftRadius = 6;
        confirmButton.style.borderTopRightRadius = 6;
        confirmButton.style.borderBottomLeftRadius = 6;
        confirmButton.style.borderBottomRightRadius = 6;
        confirmButton.style.borderTopWidth = 0;
        confirmButton.style.borderBottomWidth = 0;
        confirmButton.style.borderLeftWidth = 0;
        confirmButton.style.borderRightWidth = 0;
        confirmButton.style.fontSize = 14;
        buttonsContainer.Add(confirmButton);
        
        content.Add(buttonsContainer);
    }
    
    private void OnConfirmClicked()
    {
        OnConfirm?.Invoke();
        Hide();
    }
    
    private void OnCancelClicked()
    {
        OnCancel?.Invoke();
        Hide();
    }
    
    public void SetMessage(string message)
    {
        if (messageLabel != null)
        {
            messageLabel.text = message;
        }
    }
    
    public void SetConfirmButtonColor(Color color)
    {
        if (confirmButton != null)
        {
            confirmButton.style.backgroundColor = color;
        }
    }
}
