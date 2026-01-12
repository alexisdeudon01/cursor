#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CirclePawnPrefabFixer : EditorWindow
{
    [MenuItem("Tools/Fix CirclePawn Prefab")]
    public static void FixCirclePawnPrefab()
    {
        // Charger le prefab CirclePawn
        string prefabPath = "Assets/Prefabs/Pawns/CirclePawn.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "CirclePawn prefab not found at " + prefabPath, "OK");
            return;
        }

        // Ouvrir en mode édition
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);
        
        try
        {
            var meshRenderer = instance.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Assigner un matériau par défaut
                Material defaultMat = Resources.GetBuiltinResource<Material>("Default-Material.mat");
                if (defaultMat == null)
                {
                    // Créer un matériau simple
                    defaultMat = new Material(Shader.Find("Sprites/Default"));
                    defaultMat.color = Color.red;
                }
                
                meshRenderer.material = defaultMat;
                Debug.Log("[CirclePawnPrefabFixer] Material assigned to CirclePawn prefab");
            }

            var meshFilter = instance.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh == null)
            {
                // Créer un mesh de cercle simple
                Mesh circleMesh = CreateSimpleCircleMesh();
                meshFilter.mesh = circleMesh;
                Debug.Log("[CirclePawnPrefabFixer] Circle mesh assigned to CirclePawn prefab");
            }

            // Sauvegarder les modifications
            PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
            Debug.Log("[CirclePawnPrefabFixer] CirclePawn prefab fixed successfully");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(instance);
        }

        EditorUtility.DisplayDialog("CirclePawn Prefab Fixer", "CirclePawn prefab has been fixed with default material and circle mesh.", "OK");
    }

    private static Mesh CreateSimpleCircleMesh()
    {
        Mesh mesh = new Mesh();
        int segments = 32;
        float radius = 0.5f;

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Center vertex
        vertices[0] = Vector3.zero;

        // Circle vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
        }

        // Triangles (fan from center)
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.name = "CircleMesh";

        return mesh;
    }
}
#endif