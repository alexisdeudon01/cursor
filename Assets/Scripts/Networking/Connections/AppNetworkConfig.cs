using UnityEngine;

[CreateAssetMenu(menuName = "Network/App Network Config")]
public class AppNetworkConfig : ScriptableObject
{
    [Header("Connection")]
    public string ipAddress = "127.0.0.1";
    public ushort port = 7777;

    [Header("Lobby / Limits")]
    public int maxPlayers = 8;

    [Header("Debug")]
    public bool verboseLogs = true;  // active/désactive les logs très verbeux
}
