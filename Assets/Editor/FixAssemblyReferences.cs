#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Outil pour corriger automatiquement les références d'Assembly Definitions
/// </summary>
public static class FixAssemblyReferences
{
    [MenuItem("Tools/Project Tools/Errors/Fix Assembly References", false, 50)]
    public static void FixAllAssemblyReferences()
    {
        bool fixed = false;

        // Fix Core.asmdef - s'assurer qu'il existe et référence Unity.Collections
        string coreAsmdef = "Assets/Scripts/Core/Core.asmdef";
        if (!File.Exists(coreAsmdef))
        {
            CreateCoreAssemblyDefinition(coreAsmdef);
            fixed = true;
        }
        else
        {
            if (FixAssemblyDefinitionReferences(coreAsmdef, new[] { "Unity.Collections" }))
                fixed = true;
        }

        // Fix Networking.Shared.asmdef - doit référencer Core
        string sharedAsmdef = "Assets/Scripts/Networking/Shared.asmdef";
        if (File.Exists(sharedAsmdef))
        {
            if (FixAssemblyDefinitionReferences(sharedAsmdef, new[] { "Core", "Unity.Collections" }))
                fixed = true;
        }

        // Fix Networking.Server.asmdef - doit référencer Core
        string serverAsmdef = "Assets/Scripts/Networking/Server/Server.asmdef";
        if (File.Exists(serverAsmdef))
        {
            if (FixAssemblyDefinitionReferences(serverAsmdef, new[] { "Core", "Unity.Collections" }))
                fixed = true;
        }

        // Fix Networking.Client.asmdef - doit référencer Core
        string clientAsmdef = "Assets/Scripts/Networking/Client/Client.asmdef";
        if (File.Exists(clientAsmdef))
        {
            if (FixAssemblyDefinitionReferences(clientAsmdef, new[] { "Core", "Unity.Collections" }))
                fixed = true;
        }

        if (fixed)
        {
            AssetDatabase.Refresh();
            Debug.Log("[FixAssemblyReferences] ✅ Références d'assemblies corrigées!");
            EditorUtility.DisplayDialog("Succès", "Références d'assemblies corrigées! Unity va recompiler.", "OK");
        }
        else
        {
            Debug.Log("[FixAssemblyReferences] ✅ Toutes les références sont déjà correctes.");
            EditorUtility.DisplayDialog("Info", "Toutes les références sont déjà correctes.", "OK");
        }
    }

    private static void CreateCoreAssemblyDefinition(string path)
    {
        string content = @"{
    ""name"": ""Core"",
    ""references"": [
        ""GUID:8c3a8e0b3c3b4c4a8e0b3c3b4c4a8e0b3"",
        ""Unity.Netcode.Runtime"",
        ""Unity.Collections""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
        File.WriteAllText(path, content);
        Debug.Log($"[FixAssemblyReferences] Créé: {path}");
    }

    private static bool FixAssemblyDefinitionReferences(string asmdefPath, string[] requiredReferences)
    {
        if (!File.Exists(asmdefPath))
            return false;

        string content = File.ReadAllText(asmdefPath);
        bool modified = false;

        foreach (string refName in requiredReferences)
        {
            // Vérifier si la référence existe déjà
            if (!content.Contains($"\"{refName}\""))
            {
                // Trouver la section "references" et ajouter la référence
                int refIndex = content.IndexOf("\"references\"");
                if (refIndex >= 0)
                {
                    int arrayStart = content.IndexOf("[", refIndex);
                    int arrayEnd = content.IndexOf("]", arrayStart);
                    
                    string refsSection = content.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
                    string newRef = $"        \"{refName}\"";
                    
                    // Ajouter la référence avant la fermeture du tableau
                    if (refsSection.Trim().Length > 0 && !refsSection.Trim().EndsWith(","))
                    {
                        refsSection += ",";
                    }
                    refsSection += "\n" + newRef;
                    
                    content = content.Substring(0, arrayStart + 1) + refsSection + "\n    " + content.Substring(arrayEnd);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            File.WriteAllText(asmdefPath, content);
            Debug.Log($"[FixAssemblyReferences] Corrigé: {asmdefPath}");
        }

        return modified;
    }
}
#endif
