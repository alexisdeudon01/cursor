using UnityEngine;
using Unity.Netcode;
using System;

public class CommandLineHandler : MonoBehaviour
{
    [SerializeField] private string defaultServerIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;

    private void Start()
    {
        #if !UNITY_EDITOR
        ParseCommandLineArgs();
        #endif
    }

    private void ParseCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        bool isServer = false, isClient = false;
        string ip = defaultServerIP;
        ushort port = defaultPort;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--server": case "-s": isServer = true; break;
                case "--client": case "-c": isClient = true; break;
                case "--ip": case "-ip": if (i + 1 < args.Length) ip = args[++i]; break;
                case "--port": case "-p": if (i + 1 < args.Length) ushort.TryParse(args[++i], out port); break;
            }
        }

        var transport = NetworkManager.Singleton?.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        
        if (isServer)
        {
            Debug.Log($"[Server] Starting on port {port}");
            transport?.SetConnectionData("0.0.0.0", port);
            NetworkManager.Singleton.StartServer();
        }
        else if (isClient)
        {
            Debug.Log($"[Client] Connecting to {ip}:{port}");
            transport?.SetConnectionData(ip, port);
            NetworkManager.Singleton.StartClient();
        }
    }
}
