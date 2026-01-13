namespace Core.Networking
{
    /// <summary>
    /// Interface for scene loading services used by client and server.
    /// Implemented by Networking layer to avoid circular dependency.
    /// </summary>
    public interface ISceneServiceSync
    {
        string ActiveSceneName { get; }
        bool IsSceneLoaded(string sceneName);
        void LoadSceneIfNeeded(string sceneName);
    }
}
