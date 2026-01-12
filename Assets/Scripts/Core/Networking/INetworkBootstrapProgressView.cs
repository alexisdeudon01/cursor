namespace Core.Networking
{
    /// <summary>
    /// Interface for network bootstrap progress view (client-side UI).
    /// Implemented by Networking layer to avoid circular dependency.
    /// </summary>
    public interface INetworkBootstrapProgressView
    {
        void AddEntry(string message, bool isError);
        void SetError(string errorMessage);
        void ClearError();
        void SetProgress(float progressPercent, string step, string subStep);
        void Show();
        void Hide();
        bool Initialize();
    }
}
