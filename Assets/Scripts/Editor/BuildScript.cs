using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Script de build Unity pour Client et Serveur.
/// Utilisé par Docker et les builds automatiques.
/// </summary>
public class BuildScript
{
    private static string BuildClientPath => Path.Combine(Application.dataPath, "..", "Build", "Client");
    private static string BuildServerPath => Path.Combine(Application.dataPath, "..", "Build", "Server");

    /// <summary>
    /// Build Client (scènes Menu, Client, Game).
    /// </summary>
    public static void BuildClient()
    {
        Debug.Log("[BuildScript] Démarrage build Client...");
        
        // Scènes à inclure dans le build Client
        string[] scenes = {
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Client.unity",
            "Assets/Scenes/Game.unity"
        };
        
        // Vérifier que les scènes existent
        foreach (var scene in scenes)
        {
            if (!File.Exists(scene))
            {
                Debug.LogError($"[BuildScript] Scène non trouvée: {scene}");
                EditorApplication.Exit(1);
                return;
            }
        }
        
        // Créer le dossier de build
        Directory.CreateDirectory(BuildClientPath);
        
        // Options de build
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(BuildClientPath, "Client.x86_64"),
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        };
        
        // Lancer le build
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] ✅ Build Client réussi: {summary.outputPath}");
            Debug.Log($"[BuildScript] Taille: {summary.totalSize} bytes");
            EditorApplication.Exit(0);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BuildScript] ❌ Build Client échoué");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.LogWarning($"[BuildScript] ⚠️  Build Client annulé");
            EditorApplication.Exit(1);
        }
    }
    
    /// <summary>
    /// Build Serveur (scène Server).
    /// </summary>
    public static void BuildServer()
    {
        Debug.Log("[BuildScript] Démarrage build Serveur...");
        
        // Scène serveur
        string[] scenes = {
            "Assets/Scenes/Server.unity"
        };
        
        // Vérifier que la scène existe
        if (!File.Exists(scenes[0]))
        {
            Debug.LogError($"[BuildScript] Scène non trouvée: {scenes[0]}");
            EditorApplication.Exit(1);
            return;
        }
        
        // Créer le dossier de build
        Directory.CreateDirectory(BuildServerPath);
        
        // Options de build
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(BuildServerPath, "Server.x86_64"),
            target = BuildTarget.LinuxServer,
            options = BuildOptions.EnableHeadlessMode
        };
        
        // Lancer le build
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] ✅ Build Serveur réussi: {summary.outputPath}");
            Debug.Log($"[BuildScript] Taille: {summary.totalSize} bytes");
            EditorApplication.Exit(0);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BuildScript] ❌ Build Serveur échoué");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.LogWarning($"[BuildScript] ⚠️  Build Serveur annulé");
            EditorApplication.Exit(1);
        }
    }
}
