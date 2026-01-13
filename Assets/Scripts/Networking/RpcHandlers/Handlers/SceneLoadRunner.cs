using System.Collections;
using UnityEngine;

/// <summary>
/// Utility to run coroutines from non-MonoBehaviour handlers.
/// Creates a hidden runner GameObject if none exists.
/// </summary>
public class SceneLoadRunner : MonoBehaviour
{
    private static SceneLoadRunner _instance;

    public static void Run(IEnumerator routine)
    {
        EnsureInstance();
        if (_instance != null)
        {
            _instance.StartCoroutine(routine);
        }
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;

        var go = new GameObject("SceneLoadRunner");
        go.hideFlags = HideFlags.HideAndDontSave;
        _instance = go.AddComponent<SceneLoadRunner>();
        DontDestroyOnLoad(go);
    }
}
