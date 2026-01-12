using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CursorEditor
{
    /// <summary>
    /// Automated build script for generating Client and Server builds
    /// Implements dual-build system with full authorization for server
    /// </summary>
    public class BuildScript
    {
        private const string CLIENT_BUILD_PATH = "Builds/Client/";
        private const string SERVER_BUILD_PATH = "Builds/Server/";
        
        [MenuItem("Cursor/Build Client")]
        public static void BuildClient()
        {
            Debug.Log("Starting Client Build...");
            
            // Get all scenes for client build
            string[] clientScenes = GetClientScenes();
            
            // Build options for client
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = clientScenes,
                locationPathName = GetClientBuildPath(),
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            // Remove server define symbols for client build
            SetBuildDefineSymbols(false);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Client build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build location: {buildPlayerOptions.locationPathName}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Client build failed");
            }
        }

        [MenuItem("Cursor/Build Server")]
        public static void BuildServer()
        {
            Debug.Log("Starting Server Build...");
            
            // Get server scene (dedicated server doesn't need multiple scenes)
            string[] serverScenes = GetServerScenes();
            
            // Build options for server
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = serverScenes,
                locationPathName = GetServerBuildPath(),
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server
            };

            // Set server define symbols
            SetBuildDefineSymbols(true);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Server build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build location: {buildPlayerOptions.locationPathName}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Server build failed");
            }

            // Restore client symbols
            SetBuildDefineSymbols(false);
        }

        [MenuItem("Cursor/Build Both (Client + Server)")]
        public static void BuildBoth()
        {
            Debug.Log("Starting dual build process...");
            Debug.Log("======================================");
            
            // Build client first
            BuildClient();
            
            Debug.Log("======================================");
            
            // Build server second
            BuildServer();
            
            Debug.Log("======================================");
            Debug.Log("Dual build process completed!");
            
            // Show builds folder
            if (Directory.Exists("Builds"))
            {
                EditorUtility.RevealInFinder("Builds");
            }
        }

        [MenuItem("Cursor/Clean Builds")]
        public static void CleanBuilds()
        {
            if (Directory.Exists("Builds"))
            {
                Directory.Delete("Builds", true);
                Debug.Log("Builds folder cleaned");
            }
        }

        private static string[] GetClientScenes()
        {
            return new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Gameplay.unity",
                "Assets/Scenes/Settings.unity"
            };
        }

        private static string[] GetServerScenes()
        {
            // Server only needs a minimal scene
            return new string[]
            {
                "Assets/Scenes/ServerScene.unity"
            };
        }

        private static string GetClientBuildPath()
        {
            string buildName = "CursorClient";
            
#if UNITY_STANDALONE_WIN
            buildName += ".exe";
#elif UNITY_STANDALONE_OSX
            buildName += ".app";
#endif
            
            return Path.Combine(CLIENT_BUILD_PATH, buildName);
        }

        private static string GetServerBuildPath()
        {
            string buildName = "CursorServer";
            
#if UNITY_STANDALONE_WIN
            buildName += ".exe";
#elif UNITY_STANDALONE_OSX
            buildName += ".app";
#endif
            
            return Path.Combine(SERVER_BUILD_PATH, buildName);
        }

        private static void SetBuildDefineSymbols(bool isServerBuild)
        {
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            
            // Remove existing symbols
            currentSymbols = currentSymbols.Replace("SERVER_BUILD", "").Replace("CLIENT_BUILD", "");
            currentSymbols = currentSymbols.Replace(";;", ";").Trim(';');
            
            // Add appropriate symbol
            if (isServerBuild)
            {
                currentSymbols += ";SERVER_BUILD";
            }
            else
            {
                currentSymbols += ";CLIENT_BUILD";
            }
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, currentSymbols);
            Debug.Log($"Build symbols set: {currentSymbols}");
        }
    }
}
