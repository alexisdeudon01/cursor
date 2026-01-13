using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Generic Factory interface for creating objects of type T.
    /// </summary>
    /// <typeparam name="T">Type of objects to create</typeparam>
    public interface IFactory<out T>
    {
        T Create();
    }

    /// <summary>
    /// Generic Factory interface with parameter.
    /// </summary>
    /// <typeparam name="TParam">Type of creation parameter</typeparam>
    /// <typeparam name="TProduct">Type of product to create</typeparam>
    public interface IFactory<in TParam, out TProduct>
    {
        TProduct Create(TParam param);
    }

    /// <summary>
    /// Base abstract factory class.
    /// </summary>
    /// <typeparam name="T">Type of objects to create</typeparam>
    public abstract class Factory<T> : IFactory<T>
    {
        public abstract T Create();

        /// <summary>
        /// Optional post-creation setup hook.
        /// </summary>
        protected virtual void OnCreated(T instance) { }
    }

    /// <summary>
    /// Base abstract factory class with parameter.
    /// </summary>
    public abstract class Factory<TParam, TProduct> : IFactory<TParam, TProduct>
    {
        public abstract TProduct Create(TParam param);
        protected virtual void OnCreated(TProduct instance) { }
    }

    /// <summary>
    /// Registry-based factory for registering and creating instances by ID.
    /// Useful for game objects, UI elements, network prefabs, etc.
    /// </summary>
    /// <typeparam name="TKey">Type of registry key (string, int, enum, etc.)</typeparam>
    /// <typeparam name="TProduct">Type of product to create</typeparam>
    public class RegistryFactory<TKey, TProduct>
    {
        private readonly Dictionary<TKey, Func<TProduct>> _creators = new Dictionary<TKey, Func<TProduct>>();
        private readonly bool _allowOverwrite;

        public int Count => _creators.Count;
        public IEnumerable<TKey> RegisteredKeys => _creators.Keys;

        public RegistryFactory(bool allowOverwrite = false)
        {
            _allowOverwrite = allowOverwrite;
        }

        /// <summary>
        /// Register a creation function for a key.
        /// </summary>
        public bool Register(TKey key, Func<TProduct> creator)
        {
            if (creator == null)
            {
                Debug.LogError($"[RegistryFactory] Cannot register null creator for key '{key}'");
                return false;
            }

            if (_creators.ContainsKey(key) && !_allowOverwrite)
            {
                Debug.LogWarning($"[RegistryFactory] Key '{key}' already registered");
                return false;
            }

            _creators[key] = creator;
            Debug.Log($"[RegistryFactory] Registered creator for key: {key}");
            return true;
        }

        /// <summary>
        /// Unregister a key.
        /// </summary>
        public bool Unregister(TKey key)
        {
            return _creators.Remove(key);
        }

        /// <summary>
        /// Create an instance for the given key.
        /// </summary>
        public TProduct Create(TKey key)
        {
            if (!_creators.TryGetValue(key, out var creator))
            {
                Debug.LogError($"[RegistryFactory] No creator registered for key '{key}'");
                return default;
            }

            try
            {
                return creator();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RegistryFactory] Error creating instance for key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Check if key is registered.
        /// </summary>
        public bool IsRegistered(TKey key)
        {
            return _creators.ContainsKey(key);
        }

        /// <summary>
        /// Clear all registrations.
        /// </summary>
        public void Clear()
        {
            _creators.Clear();
        }
    }

    /// <summary>
    /// Object pool factory for reusing objects.
    /// Reduces garbage collection by pooling instances.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public class PoolFactory<T> where T : class
    {
        private readonly Func<T> _creator;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly int _maxSize;

        public int AvailableCount => _pool.Count;
        public int MaxSize => _maxSize;

        /// <summary>
        /// Create a pool factory.
        /// </summary>
        /// <param name="creator">Function to create new instances</param>
        /// <param name="onGet">Called when getting from pool (for reset/activation)</param>
        /// <param name="onRelease">Called when returning to pool (for cleanup/deactivation)</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        public PoolFactory(Func<T> creator, Action<T> onGet = null, Action<T> onRelease = null, int maxSize = 0)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
        }

        /// <summary>
        /// Get an instance from the pool or create a new one.
        /// </summary>
        public T Get()
        {
            T instance = _pool.Count > 0 ? _pool.Pop() : _creator();
            _onGet?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Return an instance to the pool.
        /// </summary>
        public void Release(T instance)
        {
            if (instance == null) return;

            _onRelease?.Invoke(instance);

            if (_maxSize <= 0 || _pool.Count < _maxSize)
            {
                _pool.Push(instance);
            }
        }

        /// <summary>
        /// Clear the pool.
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// Prewarm the pool with instances.
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Release(_creator());
            }
        }
    }

    /// <summary>
    /// Lazy factory - creates instance only when first accessed.
    /// </summary>
    /// <typeparam name="T">Type of object to create</typeparam>
    public class LazyFactory<T> where T : class
    {
        private readonly Func<T> _creator;
        private T _instance;

        public bool IsCreated => _instance != null;

        public LazyFactory(Func<T> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public T Get()
        {
            return _instance ?? (_instance = _creator());
        }

        public void Reset()
        {
            _instance = null;
        }
    }
}
