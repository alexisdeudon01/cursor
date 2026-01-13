using System.Linq;
using UnityEditor;
using UnityEngine;

public class PawnPrefabChecker : EditorWindow
{
    [MenuItem("Tools/Check Pawn Prefabs")] 
    public static void CheckPawnPrefabs()
    {
        var assets = AssetDatabase.FindAssets("t:GameDefinitionAsset", new[] {"Assets/Resources/Games"});
        int missing = 0;
        foreach (var guid in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            var so = new SerializedObject(asset);
            var prop = so.FindProperty("pawnPrefab");
            if (prop == null || prop.objectReferenceValue == null)
            {
                Debug.LogWarning($"[PawnPrefabChecker] MISSING Pawn Prefab: {path}");
                missing++;
            }
            else
            {
                Debug.Log($"[PawnPrefabChecker] OK: {path} -> {prop.objectReferenceValue.name}");
            }
        }
        if (missing == 0)
            EditorUtility.DisplayDialog("Pawn Prefab Checker", "Tous les GameDefinitionAsset ont un Pawn Prefab assigné.", "OK");
        else
            EditorUtility.DisplayDialog("Pawn Prefab Checker", $"{missing} asset(s) sans Pawn Prefab ! Voir la console.", "OK");
    }
}
