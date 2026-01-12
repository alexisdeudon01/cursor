using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [client] Local scene loader for offline or client only flow.
/// Uses Unity SceneManager and does not depend on Netcode.
/// </summary>
public class SceneServiceLocalClient : MonoBehaviour, ISceneServiceSync
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
        string target = sceneName;

        if (string.IsNullOrEmpty(target))
        {
            target = defaultSceneName;
        }

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("[SceneServiceLocalClient] Missing scene name");
            return;
        }

        if (IsSceneLoaded(target))
        {
            return;
        }

        SceneManager.LoadScene(target, LoadSceneMode.Single);
    }
}
