using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

/// <summary>
/// Point d'entrée du jeu - Gère le démarrage serveur/client
/// UN SEUL BUILD - Distinction par arguments CLI
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string serverSceneName = "ServerScene";
    [SerializeField] private string clientSceneName = "MainMenu";
    [SerializeField] private ushort defaultPort = 7777;
    
    private static GameBootstrap instance;
    
    public static bool IsServerMode { get; private set; }
    public static string ServerIP { get; private set; } = "127.0.0.1";
    public static ushort Port { get; private set; } = 7777;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        ParseCommandLineArgs();
        StartGame();
    }
    
    private void ParseCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--server":
                case "-s":
                    IsServerMode = true;
                    break;
                    
                case "--client":
                case "-c":
                    IsServerMode = false;
                    break;
                    
                case "--ip":
                    if (i + 1 < args.Length)
                        ServerIP = args[++i];
                    break;
                    
                case "--port":
                case "-p":
                    if (i + 1 < args.Length && ushort.TryParse(args[++i], out ushort port))
                        Port = port;
                    break;
            }
        }
        
        Debug.Log($"[Bootstrap] Mode: {(IsServerMode ? "SERVER" : "CLIENT")}");
        Debug.Log($"[Bootstrap] IP: {ServerIP}, Port: {Port}");
    }
    
    private void StartGame()
    {
        if (IsServerMode)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }
    
    private void StartServer()
    {
        Debug.Log($"[SERVER] Démarrage sur port {Port}...");
        
        // Configurer le transport
        var transport = NetworkManager.Singleton?.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport?.SetConnectionData("0.0.0.0", Port);
        
        // Charger la scène serveur
        SceneManager.LoadScene(serverSceneName);
        
        // Démarrer le serveur après chargement de la scène
        SceneManager.sceneLoaded += OnServerSceneLoaded;
    }
    
    private void OnServerSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == serverSceneName)
        {
            SceneManager.sceneLoaded -= OnServerSceneLoaded;
            NetworkManager.Singleton.StartServer();
            Debug.Log("[SERVER] Serveur démarré et en écoute");
        }
    }
    
    private void StartClient()
    {
        Debug.Log("[CLIENT] Chargement du menu...");
        
        // Charger le menu principal (UI Toolkit)
        SceneManager.LoadScene(clientSceneName);
        
        // Le client se connectera via le MainMenuController
    }
}
