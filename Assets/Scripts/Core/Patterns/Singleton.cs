using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Base class for MonoBehaviour Singleton pattern.
    /// Provides thread-safe singleton implementation with automatic DontDestroyOnLoad.
    /// </summary>
    /// <typeparam name="T">The type of the singleton</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Singleton instance. Thread-safe.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                            Debug.Log($"[Singleton] Created new instance of {typeof(T)}");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Check if instance exists without creating it.
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Override in derived classes for custom initialization.
        /// Called automatically on Awake.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Override in derived classes for custom cleanup.
        /// Called automatically on OnDestroy.
        /// </summary>
        protected virtual void OnCleanup() { }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnInitialize();
                Debug.Log($"[Singleton] Initialized {typeof(T).Name}");
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} destroyed");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                OnCleanup();
                _instance = null;
                Debug.Log($"[Singleton] Destroyed {typeof(T).Name}");
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }

    /// <summary>
    /// Base class for persistent singleton (survives scene loads).
    /// Identical to Singleton but enforces DontDestroyOnLoad.
    /// </summary>
    /// <typeparam name="T">The type of the persistent singleton</typeparam>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    /// <summary>
    /// Base class for scene-specific singleton (destroyed on scene load).
    /// Does NOT use DontDestroyOnLoad.
    /// </summary>
    /// <typeparam name="T">The type of the scene singleton</typeparam>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void OnInitialize() { }
        protected virtual void OnCleanup() { }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                OnInitialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                OnCleanup();
                _instance = null;
            }
        }
    }
}
