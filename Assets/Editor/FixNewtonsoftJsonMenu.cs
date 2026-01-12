using System.IO;
using UnityEditor;
using UnityEngine;

public static class FixNewtonsoftJsonMenu
{
    [MenuItem("Tools/Fix Newtonsoft.Json for UWP")] 
    public static void FixNewtonsoftJson()
    {
        // Remove all Newtonsoft.Json.dll files from Assets
        string[] dlls = Directory.GetFiles(Application.dataPath, "Newtonsoft.Json.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls)
        {
            Debug.Log($"[Fix] Removing: {dll}");
            File.Delete(dll);
            string meta = dll + ".meta";
            if (File.Exists(meta)) File.Delete(meta);
        }

        AssetDatabase.Refresh();
        
        // Check if Unity's official Newtonsoft.Json package is installed
        var listRequest = UnityEditor.PackageManager.Client.List(true);
        while (!listRequest.IsCompleted) { }
        bool found = false;
        foreach (var pkg in listRequest.Result)
        {
            if (pkg.name == "com.unity.nuget.newtonsoft-json")
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.Log("[Fix] Installing Unity Newtonsoft.Json package...");
            UnityEditor.PackageManager.Client.Add("com.unity.nuget.newtonsoft-json");
        }
        else
        {
            Debug.Log("[Fix] Unity Newtonsoft.Json package already installed.");
        }
        EditorUtility.DisplayDialog("Fix Newtonsoft.Json", "Nettoyage terminé. Vérifiez la console pour les détails.", "OK");
    }
}
