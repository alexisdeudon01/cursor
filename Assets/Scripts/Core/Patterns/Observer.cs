using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Generic Observer interface.
    /// </summary>
    /// <typeparam name="T">Type of data passed to observer</typeparam>
    public interface IObserver<in T>
    {
        void OnNotify(T data);
    }

    /// <summary>
    /// Generic Subject interface for Observer pattern.
    /// </summary>
    /// <typeparam name="T">Type of data to notify observers with</typeparam>
    public interface ISubject<T>
    {
        void Attach(IObserver<T> observer);
        void Detach(IObserver<T> observer);
        void Notify(T data);
    }

    /// <summary>
    /// Base Subject class implementing Observer pattern.
    /// Manages a list of observers and notifies them of changes.
    /// </summary>
    /// <typeparam name="T">Type of data to send to observers</typeparam>
    public class Subject<T> : ISubject<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public int ObserverCount => _observers.Count;

        public void Attach(IObserver<T> observer)
        {
            if (observer == null)
            {
                Debug.LogWarning("[Subject] Cannot attach null observer");
                return;
            }

            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IObserver<T> observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(T data)
        {
            // Iterate backwards in case observer detaches itself during notification
            for (int i = _observers.Count - 1; i >= 0; i--)
            {
                try
                {
                    _observers[i].OnNotify(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Subject] Error notifying observer: {ex.Message}");
                }
            }
        }

        public void Clear()
        {
            _observers.Clear();
        }
    }

    /// <summary>
    /// Event-based Observer using C# events.
    /// Simpler alternative to IObserver interface.
    /// </summary>
    /// <typeparam name="T">Type of event data</typeparam>
    public class EventSubject<T>
    {
        private event Action<T> _onNotify;

        public void Subscribe(Action<T> callback)
        {
            if (callback != null)
                _onNotify += callback;
        }

        public void Unsubscribe(Action<T> callback)
        {
            if (callback != null)
                _onNotify -= callback;
        }

        public void Notify(T data)
        {
            _onNotify?.Invoke(data);
        }

        public void Clear()
        {
            _onNotify = null;
        }
    }

    /// <summary>
    /// Property observer - notifies when a value changes.
    /// </summary>
    /// <typeparam name="T">Type of property value</typeparam>
    public class ObservableProperty<T>
    {
        private T _value;
        private event Action<T, T> _onValueChanged; // oldValue, newValue

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    T oldValue = _value;
                    _value = value;
                    _onValueChanged?.Invoke(oldValue, _value);
                }
            }
        }

        public ObservableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        public void Subscribe(Action<T, T> callback)
        {
            if (callback != null)
                _onValueChanged += callback;
        }

        public void Unsubscribe(Action<T, T> callback)
        {
            if (callback != null)
                _onValueChanged -= callback;
        }

        public void Clear()
        {
            _onValueChanged = null;
        }

        public static implicit operator T(ObservableProperty<T> property) => property.Value;
    }

    /// <summary>
    /// Observable collection - notifies when items are added/removed.
    /// </summary>
    /// <typeparam name="T">Type of collection items</typeparam>
    public class ObservableCollection<T>
    {
        private readonly List<T> _items = new List<T>();

        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;
        public event Action OnCleared;

        public int Count => _items.Count;
        public IReadOnlyList<T> Items => _items.AsReadOnly();

        public void Add(T item)
        {
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public bool Remove(T item)
        {
            if (_items.Remove(item))
            {
                OnItemRemoved?.Invoke(item);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _items.Clear();
            OnCleared?.Invoke();
        }

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
    }

    /// <summary>
    /// Message bus for decoupled communication between systems.
    /// Allows publishing and subscribing to messages by type.
    /// </summary>
    public class MessageBus
    {
        private readonly Dictionary<Type, Delegate> _subscribers = new Dictionary<Type, Delegate>();
        private static MessageBus _instance;

        public static MessageBus Instance => _instance ?? (_instance = new MessageBus());

        /// <summary>
        /// Subscribe to messages of type T.
        /// </summary>
        public void Subscribe<T>(Action<T> callback)
        {
            Type messageType = typeof(T);

            if (_subscribers.TryGetValue(messageType, out var existingDelegate))
            {
                _subscribers[messageType] = Delegate.Combine(existingDelegate, callback);
            }
            else
            {
                _subscribers[messageType] = callback;
            }
        }

        /// <summary>
        /// Unsubscribe from messages of type T.
        /// </summary>
        public void Unsubscribe<T>(Action<T> callback)
        {
            Type messageType = typeof(T);

            if (_subscribers.TryGetValue(messageType, out var existingDelegate))
            {
                var updatedDelegate = Delegate.Remove(existingDelegate, callback);
                if (updatedDelegate != null)
                    _subscribers[messageType] = updatedDelegate;
                else
                    _subscribers.Remove(messageType);
            }
        }

        /// <summary>
        /// Publish a message to all subscribers.
        /// </summary>
        public void Publish<T>(T message)
        {
            Type messageType = typeof(T);

            if (_subscribers.TryGetValue(messageType, out var callback))
            {
                try
                {
                    (callback as Action<T>)?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MessageBus] Error publishing message {messageType.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions.
        /// </summary>
        public void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// Clear subscriptions for a specific message type.
        /// </summary>
        public void Clear<T>()
        {
            _subscribers.Remove(typeof(T));
        }
    }
}
