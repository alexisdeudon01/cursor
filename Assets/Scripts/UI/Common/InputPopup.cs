using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Simple input popup for getting text input from user (e.g., player name, session name)
/// </summary>
public class InputPopup : PopupBase
{
    private TextField inputField;
    private Button confirmButton;
    private Label messageLabel;
    
    public System.Action<string> OnConfirm;
    
    public InputPopup(VisualElement parent, string title, string placeholder = "", string buttonText = "Confirm") 
        : base(parent, title)
    {
        CreateInputContent(placeholder, buttonText);
    }
    
    private void CreateInputContent(string placeholder, string buttonText)
    {
        var content = GetContentArea();
        if (content == null) return;
        
        // Message label
        messageLabel = new Label();
        messageLabel.name = "popup-message";
        messageLabel.style.color = new Color(0.75f, 0.75f, 0.8f, 1f);
        messageLabel.style.marginBottom = 16;
        messageLabel.style.fontSize = 14;
        messageLabel.style.display = DisplayStyle.None;
        content.Add(messageLabel);
        
        // Input field
        inputField = new TextField();
        inputField.name = "popup-input";
        inputField.style.marginBottom = 20;
        inputField.style.minWidth = 300;
        
        // Style the input field
        var textInput = inputField.Q<VisualElement>("unity-text-input");
        if (textInput != null)
        {
            textInput.style.backgroundColor = new Color(0.12f, 0.12f, 0.15f, 1f);
            textInput.style.borderTopColor = new Color(0.3f, 0.35f, 0.5f, 1f);
            textInput.style.borderBottomColor = new Color(0.3f, 0.35f, 0.5f, 1f);
            textInput.style.borderLeftColor = new Color(0.3f, 0.35f, 0.5f, 1f);
            textInput.style.borderRightColor = new Color(0.3f, 0.35f, 0.5f, 1f);
            textInput.style.borderTopWidth = 1;
            textInput.style.borderBottomWidth = 1;
            textInput.style.borderLeftWidth = 1;
            textInput.style.borderRightWidth = 1;
            textInput.style.borderTopLeftRadius = 6;
            textInput.style.borderTopRightRadius = 6;
            textInput.style.borderBottomLeftRadius = 6;
            textInput.style.borderBottomRightRadius = 6;
            textInput.style.paddingTop = 12;
            textInput.style.paddingBottom = 12;
            textInput.style.paddingLeft = 12;
            textInput.style.paddingRight = 12;
            textInput.style.color = Color.white;
        }
        
        if (!string.IsNullOrEmpty(placeholder))
        {
            inputField.value = placeholder;
        }
        
        content.Add(inputField);
        
        // Buttons container
        var buttonsContainer = new VisualElement();
        buttonsContainer.style.flexDirection = FlexDirection.Row;
        buttonsContainer.style.justifyContent = Justify.FlexEnd;
        
        // Cancel button
        var cancelButton = new Button(() => Hide());
        cancelButton.text = "Cancel";
        cancelButton.style.paddingTop = 10;
        cancelButton.style.paddingBottom = 10;
        cancelButton.style.paddingLeft = 24;
        cancelButton.style.paddingRight = 24;
        cancelButton.style.marginRight = 10;
        cancelButton.style.backgroundColor = new Color(0.35f, 0.35f, 0.4f, 1f);
        cancelButton.style.color = Color.white;
        cancelButton.style.borderTopLeftRadius = 6;
        cancelButton.style.borderTopRightRadius = 6;
        cancelButton.style.borderBottomLeftRadius = 6;
        cancelButton.style.borderBottomRightRadius = 6;
        cancelButton.style.borderTopWidth = 0;
        cancelButton.style.borderBottomWidth = 0;
        cancelButton.style.borderLeftWidth = 0;
        cancelButton.style.borderRightWidth = 0;
        buttonsContainer.Add(cancelButton);
        
        // Confirm button
        confirmButton = new Button(OnConfirmClicked);
        confirmButton.text = buttonText;
        confirmButton.style.paddingTop = 10;
        confirmButton.style.paddingBottom = 10;
        confirmButton.style.paddingLeft = 24;
        confirmButton.style.paddingRight = 24;
        confirmButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.4f, 1f);
        confirmButton.style.color = Color.white;
        confirmButton.style.borderTopLeftRadius = 6;
        confirmButton.style.borderTopRightRadius = 6;
        confirmButton.style.borderBottomLeftRadius = 6;
        confirmButton.style.borderBottomRightRadius = 6;
        confirmButton.style.borderTopWidth = 0;
        confirmButton.style.borderBottomWidth = 0;
        confirmButton.style.borderLeftWidth = 0;
        confirmButton.style.borderRightWidth = 0;
        buttonsContainer.Add(confirmButton);
        
        content.Add(buttonsContainer);
        
        // Enter key to confirm
        inputField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnConfirmClicked();
            }
        });
    }
    
    private void OnConfirmClicked()
    {
        string value = inputField?.value?.Trim() ?? "";
        if (!string.IsNullOrEmpty(value))
        {
            OnConfirm?.Invoke(value);
            Hide();
        }
    }
    
    public void SetMessage(string message)
    {
        if (messageLabel != null)
        {
            messageLabel.text = message;
            messageLabel.style.display = string.IsNullOrEmpty(message) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
    
    public void SetValue(string value)
    {
        if (inputField != null)
        {
            inputField.value = value;
        }
    }
    
    public string GetValue()
    {
        return inputField?.value ?? "";
    }
    
    public void Focus()
    {
        inputField?.Focus();
    }
    
    public override void Show()
    {
        base.Show();
        // Focus input after showing
        inputField?.schedule.Execute(() => inputField.Focus()).ExecuteLater(100);
    }
}
