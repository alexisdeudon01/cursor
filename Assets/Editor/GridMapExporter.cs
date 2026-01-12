using System.IO;
using Core.Maps;
using UnityEditor;
using UnityEngine;

public static class GridMapExporter
{
    [MenuItem("Tools/Maps/Export Selected GridMapAsset to JSON")]
    public static void ExportSelected()
    {
        var asset = Selection.activeObject as GridMapAsset;
        if (asset == null)
        {
            Debug.LogWarning("[GridMapExporter] Select a GridMapAsset to export.");
            return;
        }

        var json = asset.ToJson(true);
        var mapId = string.IsNullOrEmpty(asset.MapId) ? asset.name : asset.MapId;

        var dir = Path.Combine("Assets", "Resources", "Maps");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var path = Path.Combine(dir, $"{mapId}.json");
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log($"[GridMapExporter] Exported {mapId} to {path}");
    }
}
