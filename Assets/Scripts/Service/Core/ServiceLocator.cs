using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Simple service locator for dependency injection.
/// Provides centralized access to services without tight coupling.
/// </summary>
/// <remarks>
/// <para><b>Pattern:</b> Service Locator (lightweight alternative to full DI container).</para>
/// <para><b>Thread Safety:</b> Not thread-safe, use only from main thread.</para>
/// <para><b>Lifecycle:</b> Services are registered at startup, typically in a bootstrap/initializer.</para>
/// </remarks>
/// <example>
/// <code>
/// // Registration (at startup)
/// ServiceLocator.Register&lt;ISessionService&gt;(sessionServiceClient);
/// 
/// // Usage (anywhere)
/// var sessionService = ServiceLocator.Get&lt;ISessionService&gt;();
/// sessionService.CreateSession("My Room");
/// </code>
/// </example>
public static class ServiceLocator
{
    #region Storage

    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

    #endregion

    #region Registration

    /// <summary>
    /// Register a service instance.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <param name="service">Service implementation instance.</param>
    /// <exception cref="ArgumentNullException">If service is null.</exception>
    public static void Register<T>(T service) where T : class
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var type = typeof(T);
        
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}");
        }

        _services[type] = service;
        Debug.Log($"[ServiceLocator] Registered: {type.Name} → {service.GetType().Name}");
    }

    /// <summary>
    /// Register a factory for lazy service creation.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <param name="factory">Factory function to create the service.</param>
    public static void RegisterFactory<T>(Func<T> factory) where T : class
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var type = typeof(T);
        _factories[type] = () => factory();
        Debug.Log($"[ServiceLocator] Registered factory: {type.Name}");
    }

    /// <summary>
    /// Unregister a service.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <returns>True if service was unregistered.</returns>
    public static bool Unregister<T>() where T : class
    {
        var type = typeof(T);
        bool removed = _services.Remove(type);
        _factories.Remove(type);

        if (removed)
        {
            Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
        }

        return removed;
    }

    #endregion

    #region Resolution

    /// <summary>
    /// Get a registered service.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <returns>Service instance.</returns>
    /// <exception cref="InvalidOperationException">If service is not registered.</exception>
    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        // Try direct registration
        if (_services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

        // Try factory
        if (_factories.TryGetValue(type, out var factory))
        {
            var instance = (T)factory();
            _services[type] = instance; // Cache for future calls
            return instance;
        }

        throw new InvalidOperationException(
            $"[ServiceLocator] Service not registered: {type.Name}. " +
            $"Call ServiceLocator.Register<{type.Name}>() first.");
    }

    /// <summary>
    /// Try to get a registered service.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <param name="service">Service instance if found.</param>
    /// <returns>True if service was found.</returns>
    public static bool TryGet<T>(out T service) where T : class
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var obj))
        {
            service = (T)obj;
            return true;
        }

        if (_factories.TryGetValue(type, out var factory))
        {
            service = (T)factory();
            _services[type] = service;
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Check if a service is registered.
    /// </summary>
    /// <typeparam name="T">Service interface type.</typeparam>
    /// <returns>True if service is registered.</returns>
    public static bool IsRegistered<T>() where T : class
    {
        var type = typeof(T);
        return _services.ContainsKey(type) || _factories.ContainsKey(type);
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Clear all registered services.
    /// Call this when unloading/resetting the game.
    /// </summary>
    public static void Clear()
    {
        Debug.Log($"[ServiceLocator] Clearing {_services.Count} services and {_factories.Count} factories");
        _services.Clear();
        _factories.Clear();
    }

    /// <summary>
    /// Get count of registered services.
    /// </summary>
    public static int ServiceCount => _services.Count;

    /// <summary>
    /// Log all registered services (for debugging).
    /// </summary>
    public static void DebugPrint()
    {
        Debug.Log("[ServiceLocator] Registered services:");
        foreach (var kvp in _services)
        {
            Debug.Log($"  - {kvp.Key.Name} → {kvp.Value.GetType().Name}");
        }
        foreach (var kvp in _factories)
        {
            Debug.Log($"  - {kvp.Key.Name} → (factory)");
        }
    }

    #endregion
}

/// <summary>
/// MonoBehaviour helper for bootstrapping services at scene load.
/// Attach to a GameObject in your bootstrap scene.
/// </summary>
/// <remarks>
/// <para><b>Execution Order:</b> Set to -100 to run before other scripts.</para>
/// <para><b>Usage:</b> Configure service implementations in inspector or override <see cref="RegisterServices"/>.</para>
/// </remarks>
[DefaultExecutionOrder(-100)]
public class ServiceBootstrap : MonoBehaviour
{
    [Header("Service Configuration")]
    [Tooltip("Use server-side services (for host/dedicated server)")]
    [SerializeField] private bool isServer = false;

    [Header("Service References (Optional)")]
    [SerializeField] private SessionServiceClient clientSessionService;
    [SerializeField] private SessionServiceServer serverSessionService;
    [SerializeField] private SessionPresenter sessionPresenter;

    private void Awake()
    {
        RegisterServices();
    }

    private void OnDestroy()
    {
        // Optionally clear services when bootstrap is destroyed
        // ServiceLocator.Clear();
    }

    /// <summary>
    /// Register all services. Override in subclass for custom registration.
    /// </summary>
    protected virtual void RegisterServices()
    {
        Debug.Log($"[ServiceBootstrap] Registering services (isServer: {isServer})");

        // Determine which session service to use based on network role
        if (NetworkManager.Singleton != null)
        {
            isServer = NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost;
        }

        // Register session service
        if (isServer)
        {
            var server = serverSessionService != null ? serverSessionService : FindAnyObjectByType<SessionServiceServer>();
            if (server == null)
            {
                server = gameObject.AddComponent<SessionServiceServer>();
            }
            ServiceLocator.Register<ISessionService>(server);
        }
        else
        {
            var client = clientSessionService != null ? clientSessionService : FindAnyObjectByType<SessionServiceClient>();
            if (client == null)
            {
                client = gameObject.AddComponent<SessionServiceClient>();
            }
            ServiceLocator.Register<ISessionService>(client);
        }

        // Register presenter
        var presenter = sessionPresenter != null ? sessionPresenter : FindAnyObjectByType<SessionPresenter>();
        if (presenter != null)
        {
            // Wire up presenter to session service
            if (ServiceLocator.TryGet<ISessionService>(out var sessionService))
            {
                presenter.SetService(sessionService);
            }
        }

        ServiceLocator.DebugPrint();
    }
}
