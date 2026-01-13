using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Contrôleur du menu principal - UI Toolkit
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private TextField playerNameField;
    private TextField serverIpField;
    private TextField serverPortField;
    private Button connectButton;
    private Button quitButton;
    
    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        
        playerNameField = root.Q<TextField>("player-name");
        serverIpField = root.Q<TextField>("server-ip");
        serverPortField = root.Q<TextField>("server-port");
        connectButton = root.Q<Button>("btn-connect");
        quitButton = root.Q<Button>("btn-quit");
        
        connectButton.clicked += OnConnectClicked;
        quitButton.clicked += OnQuitClicked;
    }
    
    private void OnDisable()
    {
        connectButton.clicked -= OnConnectClicked;
        quitButton.clicked -= OnQuitClicked;
    }
    
    private void OnConnectClicked()
    {
        string playerName = playerNameField.value;
        string ip = serverIpField.value;
        
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Veuillez entrer un nom");
            return;
        }
        
        if (ushort.TryParse(serverPortField.value, out ushort port))
        {
            // Configurer le transport
            var transport = NetworkManager.Singleton?.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport?.SetConnectionData(ip, port);
            
            // Sauvegarder le nom du joueur
            PlayerPrefs.SetString("PlayerName", playerName);
            
            // Démarrer le client
            NetworkManager.Singleton.StartClient();
            
            // Charger la scène lobby
            SceneManager.LoadScene("Lobby");
        }
    }
    
    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
