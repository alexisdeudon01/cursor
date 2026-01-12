using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Generic State Machine interface.
    /// </summary>
    /// <typeparam name="TState">Enum type representing states</typeparam>
    public interface IStateMachine<TState> where TState : Enum
    {
        TState CurrentState { get; }
        void TransitionTo(TState newState);
        bool IsInState(TState state);
    }

    /// <summary>
    /// Base interface for states in a state machine.
    /// </summary>
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
    }

    /// <summary>
    /// Enhanced State Machine with validation and history.
    /// Extends the existing StateMachine.cs with additional features.
    /// </summary>
    /// <typeparam name="TState">Enum type representing states</typeparam>
    public class EnhancedStateMachine<TState> : IStateMachine<TState> where TState : Enum
    {
        private TState _currentState;
        private readonly Dictionary<TState, StateConfig> _states = new Dictionary<TState, StateConfig>();
        private readonly Stack<TState> _stateHistory = new Stack<TState>();
        private bool _isTransitioning;
        private readonly int _maxHistorySize;

        public TState CurrentState => _currentState;
        public TState PreviousState => _stateHistory.Count > 0 ? _stateHistory.Peek() : _currentState;
        public int HistoryCount => _stateHistory.Count;
        public bool IsTransitioning => _isTransitioning;

        public event Action<TState, TState> OnStateChanged;
        public event Action<TState> OnStateEntered;
        public event Action<TState> OnStateExited;

        public EnhancedStateMachine(TState initialState, int maxHistorySize = 50)
        {
            _currentState = initialState;
            _isTransitioning = false;
            _maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// Configure a state with callbacks.
        /// </summary>
        public StateConfig Configure(TState state)
        {
            if (!_states.ContainsKey(state))
                _states[state] = new StateConfig();
            return _states[state];
        }

        /// <summary>
        /// Transition to a new state with validation.
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
                return;
            }

            // Validate transition
            if (!CanTransitionTo(newState))
            {
                Debug.LogWarning($"[StateMachine] Invalid transition from {_currentState} to {newState}");
                return;
            }

            _isTransitioning = true;
            TState oldState = _currentState;

            try
            {
                // Exit current state
                if (_states.TryGetValue(_currentState, out var currentConfig))
                {
                    currentConfig.OnExitAction?.Invoke();
                }

                OnStateExited?.Invoke(_currentState);

                // Update history
                _stateHistory.Push(_currentState);
                while (_stateHistory.Count > _maxHistorySize)
                {
                    _stateHistory.Pop();
                }

                // Change state
                _currentState = newState;

                // Enter new state
                if (_states.TryGetValue(_currentState, out var newConfig))
                {
                    newConfig.OnEnterAction?.Invoke();
                }

                OnStateEntered?.Invoke(_currentState);
                OnStateChanged?.Invoke(oldState, newState);

                Debug.Log($"[StateMachine] {typeof(TState).Name}: {oldState} → {newState}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StateMachine] Error during transition: {ex.Message}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Check if can transition to a state.
        /// Override in derived classes for custom validation.
        /// </summary>
        protected virtual bool CanTransitionTo(TState newState)
        {
            if (_states.TryGetValue(_currentState, out var config))
            {
                return config.AllowedTransitions == null || config.AllowedTransitions.Contains(newState);
            }
            return true;
        }

        /// <summary>
        /// Update current state (call in Update()).
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
        /// Check if in any of the specified states.
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
        /// Force state without validation or callbacks.
        /// Use with caution.
        /// </summary>
        public void ForceState(TState state)
        {
            _currentState = state;
        }

        /// <summary>
        /// Return to previous state.
        /// </summary>
        public void ReturnToPreviousState()
        {
            if (_stateHistory.Count > 0)
            {
                TState previousState = _stateHistory.Pop();
                TransitionTo(previousState);
            }
        }

        /// <summary>
        /// Clear state history.
        /// </summary>
        public void ClearHistory()
        {
            _stateHistory.Clear();
        }

        /// <summary>
        /// Configuration for a state.
        /// </summary>
        public class StateConfig
        {
            public Action OnEnterAction;
            public Action OnExitAction;
            public Action OnUpdateAction;
            public HashSet<TState> AllowedTransitions;

            public StateConfig OnEnter(Action callback)
            {
                OnEnterAction = callback;
                return this;
            }

            public StateConfig OnExit(Action callback)
            {
                OnExitAction = callback;
                return this;
            }

            public StateConfig OnUpdate(Action callback)
            {
                OnUpdateAction = callback;
                return this;
            }

            /// <summary>
            /// Restrict transitions to only specified states.
            /// </summary>
            public StateConfig AllowTransitionsTo(params TState[] states)
            {
                AllowedTransitions = new HashSet<TState>(states);
                return this;
            }
        }
    }

    /// <summary>
    /// Hierarchical State Machine - supports nested states.
    /// </summary>
    /// <typeparam name="TState">State enum type</typeparam>
    public class HierarchicalStateMachine<TState> : IStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, IStateMachine<TState>> _substates = new Dictionary<TState, IStateMachine<TState>>();
        private readonly EnhancedStateMachine<TState> _rootMachine;

        public TState CurrentState => _rootMachine.CurrentState;

        public HierarchicalStateMachine(TState initialState)
        {
            _rootMachine = new EnhancedStateMachine<TState>(initialState);
        }

        /// <summary>
        /// Add a sub-state machine for a state.
        /// </summary>
        public void AddSubStateMachine(TState parentState, IStateMachine<TState> subMachine)
        {
            _substates[parentState] = subMachine;
        }

        public void TransitionTo(TState newState)
        {
            _rootMachine.TransitionTo(newState);
        }

        public bool IsInState(TState state)
        {
            if (_rootMachine.IsInState(state))
                return true;

            if (_substates.TryGetValue(_rootMachine.CurrentState, out var subMachine))
            {
                return subMachine.IsInState(state);
            }

            return false;
        }

        public void Update()
        {
            _rootMachine.Update();

            if (_substates.TryGetValue(_rootMachine.CurrentState, out var subMachine))
            {
                if (subMachine is EnhancedStateMachine<TState> enhanced)
                {
                    enhanced.Update();
                }
            }
        }
    }

    /// <summary>
    /// Finite State Machine with explicit state classes.
    /// More object-oriented approach using IState interface.
    /// </summary>
    /// <typeparam name="TState">State enum type</typeparam>
    public class FiniteStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, IState> _states = new Dictionary<TState, IState>();
        private TState _currentState;
        private IState _currentStateObject;

        public TState CurrentState => _currentState;

        public FiniteStateMachine(TState initialState)
        {
            _currentState = initialState;
        }

        /// <summary>
        /// Register a state implementation.
        /// </summary>
        public void RegisterState(TState state, IState stateObject)
        {
            _states[state] = stateObject;
        }

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        public void TransitionTo(TState newState)
        {
            if (_currentState.Equals(newState))
                return;

            _currentStateObject?.OnExit();

            _currentState = newState;

            if (_states.TryGetValue(newState, out var stateObject))
            {
                _currentStateObject = stateObject;
                _currentStateObject.OnEnter();
            }
        }

        /// <summary>
        /// Update current state.
        /// </summary>
        public void Update()
        {
            _currentStateObject?.OnUpdate();
        }
    }
}
