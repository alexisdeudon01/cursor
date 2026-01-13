using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic state machine for managing UI states and transitions.
/// Ensures only one state is active at a time with clear transitions.
/// </summary>
/// <typeparam name="TState">Enum type representing states</typeparam>
public class StateMachine<TState> where TState : Enum
{
    private TState _currentState;
    private readonly Dictionary<TState, StateConfig> _states = new Dictionary<TState, StateConfig>();
    private bool _isTransitioning;

    /// <summary>
    /// Current active state.
    /// </summary>
    public TState CurrentState => _currentState;

    /// <summary>
    /// Event fired when state changes.
    /// </summary>
    public event Action<TState, TState> OnStateChanged;

    /// <summary>
    /// Create a new state machine with an initial state.
    /// </summary>
    public StateMachine(TState initialState)
    {
        _currentState = initialState;
        _isTransitioning = false;
    }

    /// <summary>
    /// Configure a state with enter/exit/update callbacks.
    /// </summary>
    public StateConfig Configure(TState state)
    {
        if (!_states.ContainsKey(state))
            _states[state] = new StateConfig();
        return _states[state];
    }

    /// <summary>
    /// Transition to a new state.
    /// Executes exit callback of current state, then enter callback of new state.
    /// </summary>
    public void TransitionTo(TState newState)
    {
        if (_isTransitioning)
        {
            Debug.LogWarning($"[StateMachine] Already transitioning, ignoring transition to {newState}");
            return;
        }

        if (_currentState.Equals(newState))
        {
            Debug.Log($"[StateMachine] Already in state {newState}, ignoring transition");
            return;
        }

        _isTransitioning = true;
        TState oldState = _currentState;

        try
        {
            // Execute exit callback of current state
            if (_states.TryGetValue(_currentState, out var currentConfig))
            {
                currentConfig.OnExitAction?.Invoke();
            }

            _currentState = newState;

            // Execute enter callback of new state
            if (_states.TryGetValue(_currentState, out var newConfig))
            {
                newConfig.OnEnterAction?.Invoke();
            }

            Debug.Log($"[StateMachine] {typeof(TState).Name}: {oldState} → {newState}");

            // Fire event
            OnStateChanged?.Invoke(oldState, newState);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StateMachine] Error during transition {oldState} → {newState}: {ex.Message}");
            Debug.LogException(ex);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    /// <summary>
    /// Call this in Update() to execute state update callbacks.
    /// </summary>
    public void Update()
    {
        if (_states.TryGetValue(_currentState, out var config))
        {
            config.OnUpdateAction?.Invoke();
        }
    }

    /// <summary>
    /// Check if currently in a specific state.
    /// </summary>
    public bool IsInState(TState state)
    {
        return _currentState.Equals(state);
    }

    /// <summary>
    /// Check if currently in any of the specified states.
    /// </summary>
    public bool IsInAnyState(params TState[] states)
    {
        foreach (var state in states)
        {
            if (_currentState.Equals(state))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Configuration for a single state.
    /// </summary>
    public class StateConfig
    {
        internal Action OnEnterAction;
        internal Action OnExitAction;
        internal Action OnUpdateAction;

        /// <summary>
        /// Set callback to execute when entering this state.
        /// </summary>
        public StateConfig OnEnter(Action action)
        {
            OnEnterAction = action;
            return this;
        }

        /// <summary>
        /// Set callback to execute when exiting this state.
        /// </summary>
        public StateConfig OnExit(Action action)
        {
            OnExitAction = action;
            return this;
        }

        /// <summary>
        /// Set callback to execute every frame while in this state.
        /// </summary>
        public StateConfig OnUpdate(Action action)
        {
            OnUpdateAction = action;
            return this;
        }
    }
}
