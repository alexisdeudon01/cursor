using Core.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [server] Network scene loader using NetworkManager to sync clients.
/// Use on server or host for authoritative scene changes.
/// </summary>
public class SceneServiceNetworkServer : NetworkBehaviour, ISceneServiceSync
{
    [SerializeField] private string defaultSceneName = "Game";

    public string ActiveSceneName
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    public bool IsSceneLoaded(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        return SceneManager.GetActiveScene().name == sceneName;
    }

    public void LoadSceneIfNeeded(string sceneName)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[SceneServiceNetworkServer] Not running on server");
            return;
        }

        string target = sceneName;

        if (string.IsNullOrEmpty(target))
        {
            target = defaultSceneName;
        }

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("[SceneServiceNetworkServer] Missing scene name");
            return;
        }

        if (IsSceneLoaded(target))
        {
            return;
        }

        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
        {
            Debug.LogWarning("[SceneServiceNetworkServer] NetworkManager not found");
            return;
        }

        NetworkSceneManager sceneManager = manager.SceneManager;
        if (sceneManager == null)
        {
            Debug.LogWarning("[SceneServiceNetworkServer] NetworkSceneManager not found");
            return;
        }

        sceneManager.LoadScene(target, LoadSceneMode.Single);
    }
}
