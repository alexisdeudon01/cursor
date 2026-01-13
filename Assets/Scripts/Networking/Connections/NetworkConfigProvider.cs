using UnityEngine;

public static class NetworkConfigProvider
{
    private static AppNetworkConfig _config;

    public static AppNetworkConfig Config
    {
        get
        {
            try
            {
                if (_config == null)
                {
                    Debug.Log("[NetworkConfigProvider] Loading NetworkConfig from Resources...");
                    _config = Resources.Load<AppNetworkConfig>("NetworkConfig");

                    if (_config == null)
                    {
                        Debug.LogError("[NetworkConfigProvider] ❌ NetworkConfig.asset not found. Put it at: Assets/Resources/NetworkConfig.asset");
                    }
                    else
                    {
                        Debug.Log($"[NetworkConfigProvider] ✅ Loaded config: ip={_config.ipAddress}, port={_config.port}, maxPlayers={_config.maxPlayers}, verbose={_config.verboseLogs}");
                    }
                }

                return _config;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[NetworkConfigProvider] ❌ Exception while loading config");
                Debug.LogException(ex);
                return null;
            }
        }
    }
}
