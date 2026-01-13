using Core.Networking;

/// <summary>
/// [sync] Interface for scene loading services used by client and server.
/// Implementations can be local only or driven by NetworkManager.
/// </summary>
public interface ISceneServiceSync
{
    string ActiveSceneName { get; }
    bool IsSceneLoaded(string sceneName);
    void LoadSceneIfNeeded(string sceneName);
}
