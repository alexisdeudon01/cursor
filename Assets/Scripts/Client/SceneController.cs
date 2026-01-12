using UnityEngine;
using UnityEngine.SceneManagement;

namespace CursorClient
{
    /// <summary>
    /// Manages scene transitions for the client
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        private static SceneController instance;
        public static SceneController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneController>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SceneController");
                        instance = go.AddComponent<SceneController>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMainMenu()
        {
            Debug.Log("Loading Main Menu scene");
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadGameplay()
        {
            Debug.Log("Loading Gameplay scene");
            SceneManager.LoadScene("Gameplay");
        }

        public void LoadSettings()
        {
            Debug.Log("Loading Settings scene");
            SceneManager.LoadScene("Settings");
        }

        public void LoadScene(string sceneName)
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneAsync(string sceneName)
        {
            Debug.Log($"Loading scene asynchronously: {sceneName}");
            SceneManager.LoadSceneAsync(sceneName);
        }

        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
