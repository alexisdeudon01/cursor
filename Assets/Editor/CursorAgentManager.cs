#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mo2.Editor
{
    /// <summary>
    /// Gestionnaire des agents Cursor pour Unity
    /// </summary>
    public class CursorAgentManager : EditorWindow
    {
        private Vector2 scrollPosition;
        private string agentsDirectory;
        private List<AgentInfo> agents = new List<AgentInfo>();
        private int selectedAgentIndex = -1;
        private string searchFilter = "";
        private bool showDetails = false;

        private class AgentInfo
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public string FullPath { get; set; }
            public long FileSize { get; set; }
            public int LineCount { get; set; }
            public string Description { get; set; }
            public DateTime LastModified { get; set; }
        }

        [MenuItem("Tools/Cursor/Manage Agents", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<CursorAgentManager>("Cursor Agents");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        [MenuItem("Tools/Cursor/List Agents", false, 2)]
        public static void ListAgents()
        {
            var agentsDir = Path.Combine(Application.dataPath, "..", ".cursor", "agents");
            if (!Directory.Exists(agentsDir))
            {
                EditorUtility.DisplayDialog("Cursor Agents", 
                    $"Agents directory not found:\n{agentsDir}\n\nPlease ensure you have agents in .cursor/agents/", 
                    "OK");
                return;
            }

            var agents = Directory.GetFiles(agentsDir, "*.md", SearchOption.TopDirectoryOnly);
            var message = $"Found {agents.Length} agent(s):\n\n";
            
            foreach (var agent in agents)
            {
                var name = Path.GetFileNameWithoutExtension(agent);
                var info = new FileInfo(agent);
                message += $"• {name}\n  Size: {info.Length} bytes\n  Modified: {info.LastWriteTime}\n\n";
            }

            EditorUtility.DisplayDialog("Cursor Agents", message, "OK");
        }

        [MenuItem("Tools/Cursor/Open Agents Directory", false, 3)]
        public static void OpenAgentsDirectory()
        {
            var agentsDir = Path.Combine(Application.dataPath, "..", ".cursor", "agents");
            if (!Directory.Exists(agentsDir))
            {
                Directory.CreateDirectory(agentsDir);
            }
            
            EditorUtility.RevealInFinder(agentsDir);
        }

        [MenuItem("Tools/Cursor/Verify Agent Configuration", false, 4)]
        public static void VerifyConfiguration()
        {
            var projectRoot = Application.dataPath + "/..";
            var agentsDir = Path.Combine(projectRoot, ".cursor", "agents");
            var issues = new List<string>();
            var successes = new List<string>();

            // Vérifier le répertoire
            if (Directory.Exists(agentsDir))
            {
                successes.Add($"✅ Agents directory exists: {agentsDir}");
            }
            else
            {
                issues.Add($"❌ Agents directory missing: {agentsDir}");
            }

            // Vérifier les agents
            if (Directory.Exists(agentsDir))
            {
                var agents = Directory.GetFiles(agentsDir, "*.md", SearchOption.TopDirectoryOnly);
                if (agents.Length > 0)
                {
                    successes.Add($"✅ Found {agents.Length} agent file(s)");
                    foreach (var agent in agents)
                    {
                        var info = new FileInfo(agent);
                        if (info.Length > 0)
                        {
                            successes.Add($"  • {Path.GetFileName(agent)} ({info.Length} bytes)");
                        }
                        else
                        {
                            issues.Add($"  ⚠️ {Path.GetFileName(agent)} is empty");
                        }
                    }
                }
                else
                {
                    issues.Add("⚠️ No agent files found in .cursor/agents/");
                }
            }

            // Afficher le résultat
            var message = "Cursor Agent Configuration Check\n\n";
            message += string.Join("\n", successes);
            if (issues.Count > 0)
            {
                message += "\n\nIssues:\n";
                message += string.Join("\n", issues);
            }
            else
            {
                message += "\n\n✅ All checks passed!";
            }

            EditorUtility.DisplayDialog("Agent Configuration", message, "OK");
        }

        [MenuItem("Tools/Cursor/Create Agent Template", false, 5)]
        public static void CreateAgentTemplate()
        {
            var agentsDir = Path.Combine(Application.dataPath, "..", ".cursor", "agents");
            if (!Directory.Exists(agentsDir))
            {
                Directory.CreateDirectory(agentsDir);
            }

            var templatePath = Path.Combine(agentsDir, "new-agent-template.md");
            var template = @"# Agent role
You are a Cursor agent specialized in [DESCRIBE YOUR AGENT'S PURPOSE].

# Mandatory context (must follow)
- [Add your mandatory context here]

# Target architecture
[Describe the target architecture]

# Implementation directives
1. [Directive 1]
2. [Directive 2]

# Coding conventions
- [Convention 1]
- [Convention 2]

# What the agent must produce
- [Output 1]
- [Output 2]
";

            File.WriteAllText(templatePath, template);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(templatePath);
            EditorUtility.DisplayDialog("Agent Template", 
                $"Template created at:\n{templatePath}\n\nPlease edit it with your agent's specific instructions.", 
                "OK");
        }

        private void OnEnable()
        {
            RefreshAgents();
        }

        private void RefreshAgents()
        {
            agents.Clear();
            var projectRoot = Application.dataPath + "/..";
            agentsDirectory = Path.Combine(projectRoot, ".cursor", "agents");

            if (!Directory.Exists(agentsDirectory))
            {
                return;
            }

            var agentFiles = Directory.GetFiles(agentsDirectory, "*.md", SearchOption.TopDirectoryOnly);
            
            foreach (var file in agentFiles)
            {
                try
                {
                    var info = new FileInfo(file);
                    var lines = File.ReadAllLines(file);
                    var description = ExtractDescription(lines);
                    
                    agents.Add(new AgentInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        FilePath = Path.GetRelativePath(projectRoot, file),
                        FullPath = file,
                        FileSize = info.Length,
                        LineCount = lines.Length,
                        Description = description,
                        LastModified = info.LastWriteTime
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading agent file {file}: {e.Message}");
                }
            }

            agents = agents.OrderBy(a => a.Name).ToList();
        }

        private string ExtractDescription(string[] lines)
        {
            // Chercher une description dans les premières lignes
            for (int i = 0; i < Math.Min(20, lines.Length); i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("# Agent role") && i + 1 < lines.Length)
                {
                    return lines[i + 1].Trim();
                }
                if (line.StartsWith("description:") || line.StartsWith("Description:"))
                {
                    var desc = line.Substring(line.IndexOf(':') + 1).Trim();
                    if (desc.StartsWith("'") || desc.StartsWith("\""))
                    {
                        desc = desc.Substring(1, desc.Length - 2);
                    }
                    return desc;
                }
            }
            return "No description available";
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // En-tête
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Cursor Agents Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Barre d'outils
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshAgents();
            }
            
            if (GUILayout.Button("Open Directory", GUILayout.Width(120)))
            {
                OpenAgentsDirectory();
            }
            
            if (GUILayout.Button("Verify Config", GUILayout.Width(120)))
            {
                VerifyConfiguration();
            }
            
            if (GUILayout.Button("Create Template", GUILayout.Width(120)))
            {
                CreateAgentTemplate();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Recherche
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Found {agents.Count} agent(s)", EditorStyles.miniLabel);
            EditorGUILayout.Space(5);
            
            // Liste des agents
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var filteredAgents = string.IsNullOrEmpty(searchFilter)
                ? agents
                : agents.Where(a => a.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                                   a.Description.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            
            for (int i = 0; i < filteredAgents.Count; i++)
            {
                var agent = filteredAgents[i];
                var isSelected = selectedAgentIndex == i;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // En-tête de l'agent
                EditorGUILayout.BeginHorizontal();
                
                var style = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                showDetails = EditorGUILayout.Foldout(showDetails && isSelected, agent.Name, true, style);
                
                if (showDetails && isSelected)
                {
                    selectedAgentIndex = i;
                }
                else if (!showDetails && isSelected)
                {
                    selectedAgentIndex = -1;
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    EditorUtility.OpenWithDefaultApp(agent.FullPath);
                }
                
                if (GUILayout.Button("Reveal", GUILayout.Width(60)))
                {
                    EditorUtility.RevealInFinder(agent.FullPath);
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Détails
                if (showDetails && isSelected)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Description:", EditorStyles.miniLabel);
                    EditorGUILayout.TextArea(agent.Description, GUILayout.Height(40));
                    
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"File: {agent.FilePath}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Size: {agent.FileSize} bytes", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Lines: {agent.LineCount}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Modified: {agent.LastModified:yyyy-MM-dd HH:mm}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    // Aperçu court
                    EditorGUILayout.LabelField(agent.Description, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
            
            if (filteredAgents.Count == 0)
            {
                EditorGUILayout.HelpBox("No agents found. Create one using 'Create Template' button.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
