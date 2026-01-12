using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Base interface for commands.
    /// Encapsulates an action with undo capability.
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    /// <summary>
    /// Base interface for commands with client/player ownership.
    /// Used for multiplayer actions.
    /// </summary>
    public interface IPlayerCommand : ICommand
    {
        ulong ClientId { get; }
    }

    /// <summary>
    /// Base abstract command class.
    /// Provides logging and error handling.
    /// </summary>
    public abstract class Command : ICommand
    {
        protected bool _isExecuted;

        public bool IsExecuted => _isExecuted;

        public void Execute()
        {
            if (_isExecuted)
            {
                Debug.LogWarning($"[Command] {GetType().Name} already executed");
                return;
            }

            try
            {
                OnExecute();
                _isExecuted = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Command] Error executing {GetType().Name}: {ex.Message}");
            }
        }

        public void Undo()
        {
            if (!_isExecuted)
            {
                Debug.LogWarning($"[Command] Cannot undo {GetType().Name} - not executed");
                return;
            }

            try
            {
                OnUndo();
                _isExecuted = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Command] Error undoing {GetType().Name}: {ex.Message}");
            }
        }

        protected abstract void OnExecute();
        protected abstract void OnUndo();
    }

    /// <summary>
    /// Command invoker - manages command execution and history.
    /// Supports undo/redo, batch execution, and command queuing.
    /// </summary>
    public class CommandInvoker
    {
        private readonly Stack<ICommand> _history = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private readonly Queue<ICommand> _commandQueue = new Queue<ICommand>();
        private readonly int _maxHistorySize;

        public int HistoryCount => _history.Count;
        public int RedoCount => _redoStack.Count;
        public int QueueCount => _commandQueue.Count;
        public bool CanUndo => _history.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public CommandInvoker(int maxHistorySize = 100)
        {
            _maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// Execute a command immediately and add to history.
        /// </summary>
        public void Execute(ICommand command)
        {
            if (command == null)
            {
                Debug.LogWarning("[CommandInvoker] Cannot execute null command");
                return;
            }

            command.Execute();
            _history.Push(command);

            // Clear redo stack when new command is executed
            _redoStack.Clear();

            // Trim history if needed
            while (_history.Count > _maxHistorySize)
            {
                _history.Pop();
            }
        }

        /// <summary>
        /// Queue a command for later execution.
        /// </summary>
        public void Queue(ICommand command)
        {
            if (command == null)
            {
                Debug.LogWarning("[CommandInvoker] Cannot queue null command");
                return;
            }

            _commandQueue.Enqueue(command);
        }

        /// <summary>
        /// Process all queued commands.
        /// </summary>
        public void ProcessQueue()
        {
            while (_commandQueue.Count > 0)
            {
                var command = _commandQueue.Dequeue();
                Execute(command);
            }
        }

        /// <summary>
        /// Undo last executed command.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[CommandInvoker] No commands to undo");
                return;
            }

            var command = _history.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        /// <summary>
        /// Redo last undone command.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
            {
                Debug.LogWarning("[CommandInvoker] No commands to redo");
                return;
            }

            var command = _redoStack.Pop();
            command.Execute();
            _history.Push(command);
        }

        /// <summary>
        /// Clear all history and redo stacks.
        /// </summary>
        public void ClearHistory()
        {
            _history.Clear();
            _redoStack.Clear();
        }

        /// <summary>
        /// Clear command queue without executing.
        /// </summary>
        public void ClearQueue()
        {
            _commandQueue.Clear();
        }
    }

    /// <summary>
    /// Composite command - executes multiple commands as one.
    /// </summary>
    public class CompositeCommand : Command
    {
        private readonly List<ICommand> _commands = new List<ICommand>();

        public void Add(ICommand command)
        {
            if (command != null)
                _commands.Add(command);
        }

        protected override void OnExecute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        protected override void OnUndo()
        {
            // Undo in reverse order
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }

    /// <summary>
    /// Macro command - record and replay a sequence of commands.
    /// </summary>
    public class MacroCommand : Command
    {
        private readonly List<ICommand> _recordedCommands = new List<ICommand>();
        private bool _isRecording;

        public bool IsRecording => _isRecording;
        public int RecordedCount => _recordedCommands.Count;

        public void StartRecording()
        {
            _isRecording = true;
            _recordedCommands.Clear();
        }

        public void StopRecording()
        {
            _isRecording = false;
        }

        public void Record(ICommand command)
        {
            if (_isRecording && command != null)
            {
                _recordedCommands.Add(command);
            }
        }

        protected override void OnExecute()
        {
            foreach (var command in _recordedCommands)
            {
                command.Execute();
            }
        }

        protected override void OnUndo()
        {
            for (int i = _recordedCommands.Count - 1; i >= 0; i--)
            {
                _recordedCommands[i].Undo();
            }
        }

        public void Clear()
        {
            _recordedCommands.Clear();
        }
    }
}
