using Core.Patterns;
using Core.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Centralized UI Manager that ensures only one popup/panel is visible at a time.
/// Manages all UI states and transitions using Singleton pattern.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    /// <summary>
    /// All possible UI states
    /// </summary>
    public enum UIState
    {
        None,
        NameEntry,      // Enter player name
        Lobby,          // Main session list
        SessionDetail,  // Session popup/detail view
        InGame          // Playing the game
    }

    private UIState currentState = UIState.None;
    public UIState CurrentState => currentState;

    /// <summary>
    /// Event fired when UI state changes
    /// </summary>
    public static System.Action<UIState, UIState> OnStateChanged;

    // Track registered panels
    private readonly Dictionary<UIState, VisualElement> registeredPanels = new Dictionary<UIState, VisualElement>();

    protected override void OnInitialize()
    {
        NetworkLogger.Info("UIManager", "UI Manager initialized");
    }

    protected override void OnCleanup()
    {
        registeredPanels.Clear();
        NetworkLogger.Info("UIManager", "UI Manager cleaned up");
    }

    /// <summary>
    /// Register a panel for a specific UI state
    /// </summary>
    public void RegisterPanel(UIState state, VisualElement panel)
    {
        if (panel == null) return;

        registeredPanels[state] = panel;
        NetworkLogger.DebugLog("UIManager", $"Registered panel for state: {state}");
    }

    /// <summary>
    /// Unregister a panel
    /// </summary>
    public void UnregisterPanel(UIState state)
    {
        if (registeredPanels.ContainsKey(state))
        {
            registeredPanels.Remove(state);
            NetworkLogger.DebugLog("UIManager", $"Unregistered panel for state: {state}");
        }
    }

    /// <summary>
    /// Change to a new UI state, hiding all other panels
    /// </summary>
    public void SetState(UIState newState)
    {
        if (newState == currentState) return;

        UIState previousState = currentState;
        currentState = newState;

        NetworkLogger.Info("UIManager", $"State change: {previousState} -> {newState}");

        // Hide all panels except the new one
        foreach (var kvp in registeredPanels)
        {
            if (kvp.Value != null)
            {
                bool shouldShow = kvp.Key == newState;
                kvp.Value.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        // Also hide any PopupBase popups
        PopupBase.HideAll();

        // Fire event
        OnStateChanged?.Invoke(previousState, newState);
    }

    /// <summary>
    /// Show a specific panel while hiding others
    /// </summary>
    public void ShowPanel(VisualElement panel)
    {
        // Hide all registered panels
        foreach (var kvp in registeredPanels)
        {
            if (kvp.Value != null)
            {
                kvp.Value.style.display = DisplayStyle.None;
            }
        }

        // Show the requested panel
        if (panel != null)
        {
            panel.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Check if currently in game
    /// </summary>
    public bool IsInGame => currentState == UIState.InGame;

    /// <summary>
    /// Quick transition to lobby
    /// </summary>
    public void GoToLobby()
    {
        SetState(UIState.Lobby);
    }

    /// <summary>
    /// Quick transition to in-game
    /// </summary>
    public void GoToGame()
    {
        SetState(UIState.InGame);
    }

    /// <summary>
    /// Get panel for a specific state
    /// </summary>
    public VisualElement GetPanel(UIState state)
    {
        return registeredPanels.TryGetValue(state, out var panel) ? panel : null;
    }
}
