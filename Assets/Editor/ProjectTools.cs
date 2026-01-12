using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Menu Unity Editor avec outils de développement :
    /// - Export de diagrammes (Class, DTO, Package)
    /// - Scripts Git (push vers branché-1, dev)
    /// - Vérification et correction d'erreurs
    /// </summary>
    public static class ProjectTools
    {
        private const string MENU_BASE = "Tools/Project Tools/";
        private const string DIAGRAMS_PATH = "Assets/Scripts/Documentation/Diagrams/";
        private const string DOCUMENTATION_PATH = "documentation/diagrams/";

        #region Export Diagrams

        [MenuItem(MENU_BASE + "Export/Class Diagram", false, 1)]
        public static void ExportClassDiagram()
        {
            ExportDiagram("class_diagram.md", "ClassDiagram.md", "Class Diagram");
        }

        [MenuItem(MENU_BASE + "Export/DTO Diagram", false, 2)]
        public static void ExportDTODiagram()
        {
            ExportDiagram("schema_conceptuel.md", "DTODiagram.md", "DTO Diagram");
        }

        [MenuItem(MENU_BASE + "Export/Package Diagram", false, 3)]
        public static void ExportPackageDiagram()
        {
            ExportDiagram("package_diagram.md", "PackageDiagram.md", "Package Diagram");
        }

        private static void ExportDiagram(string sourceFile, string outputFile, string diagramName)
        {
            string sourcePath = Path.Combine(DOCUMENTATION_PATH, sourceFile);
            string outputPath = Path.Combine(DIAGRAMS_PATH, outputFile);

            // Chercher aussi dans Assets/Scripts/Documentation/Diagrams/
            if (!File.Exists(sourcePath))
            {
                string altPath = Path.Combine("Assets/Scripts/Documentation/Diagrams", sourceFile);
                if (File.Exists(altPath))
                {
                    sourcePath = altPath;
                }
                else
                {
                    Debug.LogError($"[ProjectTools] Fichier source non trouvé: {sourceFile}");
                    EditorUtility.DisplayDialog("Erreur", $"Fichier source non trouvé: {sourceFile}", "OK");
                    return;
                }
            }

            try
            {
                // Créer le dossier de destination si nécessaire
                string outputDir = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Copier le fichier
                File.Copy(sourcePath, outputPath, true);
                AssetDatabase.Refresh();

                Debug.Log($"[ProjectTools] {diagramName} exporté vers: {outputPath}");
                EditorUtility.DisplayDialog("Succès", $"{diagramName} exporté avec succès!", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectTools] Erreur lors de l'export: {ex.Message}");
                EditorUtility.DisplayDialog("Erreur", $"Erreur lors de l'export: {ex.Message}", "OK");
            }
        }

        #endregion

        #region Git Scripts

        [MenuItem(MENU_BASE + "Git/Push to branché-1", false, 20)]
        public static void PushToBranche1()
        {
            ExecuteGitScript("push_to_branche1.sh", "Push vers branché-1");
        }

        [MenuItem(MENU_BASE + "Git/Push to dev", false, 21)]
        public static void PushToDev()
        {
            ExecuteGitScript("push_to_dev.sh", "Push vers dev");
        }

        private static void ExecuteGitScript(string scriptName, string actionName)
        {
            string scriptPath = Path.Combine(Application.dataPath, "..", scriptName);

            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"[ProjectTools] Script non trouvé: {scriptPath}");
                EditorUtility.DisplayDialog("Erreur", $"Script non trouvé: {scriptName}", "OK");
                return;
            }

            // Sur Linux/Mac, exécuter directement
            #if UNITY_EDITOR_LINUX || UNITY_EDITOR_OSX
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath)
            };

            try
            {
                var process = System.Diagnostics.Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Debug.Log($"[ProjectTools] {actionName} réussi:\n{output}");
                    EditorUtility.DisplayDialog("Succès", $"{actionName} réussi!", "OK");
                }
                else
                {
                    Debug.LogError($"[ProjectTools] {actionName} échoué:\n{error}");
                    EditorUtility.DisplayDialog("Erreur", $"{actionName} échoué. Voir la console pour les détails.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectTools] Erreur lors de l'exécution: {ex.Message}");
                EditorUtility.DisplayDialog("Erreur", $"Erreur: {ex.Message}", "OK");
            }
            #else
            // Sur Windows, ouvrir dans un terminal
            EditorUtility.DisplayDialog("Info", $"Sur Windows, exécutez manuellement: {scriptName}", "OK");
            #endif
        }

        #endregion

        #region Error Checker & Fixer

        [MenuItem(MENU_BASE + "Errors/Check & Fix All", false, 40)]
        public static void CheckAndFixAllErrors()
        {
            bool hasErrors = false;

            // 1. Fix Assembly Definitions
            if (FixAssemblyDefinitions())
            {
                hasErrors = true;
            }

            // 2. Check Input Manager (avertissement seulement)
            CheckInputManager();

            // 3. Check GridMapExporter (avertissement seulement)
            CheckGridMapExporter();

            if (hasErrors)
            {
                Debug.Log("[ProjectTools] ✅ Toutes les erreurs ont été corrigées!");
                EditorUtility.DisplayDialog("Succès", "Erreurs corrigées! Voir la console pour les détails.", "OK");
            }
            else
            {
                Debug.Log("[ProjectTools] ✅ Aucune erreur critique détectée.");
                EditorUtility.DisplayDialog("Info", "Aucune erreur critique. Voir la console pour les avertissements.", "OK");
            }

            AssetDatabase.Refresh();
        }

        [MenuItem(MENU_BASE + "Errors/Fix Assembly Definitions", false, 41)]
        public static bool FixAssemblyDefinitions()
        {
            bool fixed = false;

            // Fix Server.asmdef
            string serverAsmdef = "Assets/Scripts/Networking/Server/Server.asmdef";
            if (File.Exists(serverAsmdef))
            {
                string content = File.ReadAllText(serverAsmdef);
                if (content.Contains("\"Server\""))
                {
                    content = content.Replace("\"Server\"", "\"LinuxStandalone64Server\", \"WindowsStandalone64Server\"");
                    File.WriteAllText(serverAsmdef, content);
                    Debug.Log("[ProjectTools] ✅ Server.asmdef corrigé");
                    fixed = true;
                }
            }

            // Fix Client.asmdef
            string clientAsmdef = "Assets/Scripts/Networking/Client/Client.asmdef";
            if (File.Exists(clientAsmdef))
            {
                string content = File.ReadAllText(clientAsmdef);
                if (content.Contains("\"Server\""))
                {
                    // Client doit exclure les plateformes serveur
                    content = content.Replace("\"Server\"", "\"LinuxStandalone64Server\", \"WindowsStandalone64Server\"");
                    File.WriteAllText(clientAsmdef, content);
                    Debug.Log("[ProjectTools] ✅ Client.asmdef corrigé");
                    fixed = true;
                }
            }

            if (fixed)
            {
                AssetDatabase.Refresh();
            }

            return fixed;
        }

        [MenuItem(MENU_BASE + "Errors/Check Input Manager", false, 42)]
        public static void CheckInputManager()
        {
            // Vérifier si Input Manager est activé
            #if UNITY_6000_OR_NEWER
            Debug.LogWarning("[ProjectTools] ⚠️ Input Manager est déprécié. Utilisez Input System package à la place.");
            Debug.Log("[ProjectTools] Pour migrer: Edit > Project Settings > Player > Active Input Handling > Input System Package (New)");
            #else
            Debug.Log("[ProjectTools] Input Manager: OK (pas de migration nécessaire pour cette version)");
            #endif
        }

        [MenuItem(MENU_BASE + "Errors/Check GridMapExporter", false, 43)]
        public static void CheckGridMapExporter()
        {
            // Les warnings GridMapExporter sont normaux si aucun asset n'est sélectionné
            Debug.Log("[ProjectTools] GridMapExporter: Les warnings sont normaux si aucun GridMapAsset n'est sélectionné.");
        }

        #endregion
    }
}
