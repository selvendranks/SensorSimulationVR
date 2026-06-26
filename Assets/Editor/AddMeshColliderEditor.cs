using UnityEngine;
using UnityEditor;

public class AddMeshCollidersEditor : EditorWindow
{
    private GameObject rootObject;
    private bool convex = false;
    private bool skipInvalidMeshes = true;

    [MenuItem("Tools/Add MeshColliders To Children")]
    public static void ShowWindow()
    {
        GetWindow<AddMeshCollidersEditor>("Add MeshColliders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add MeshColliders to all MeshFilters under a root object", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        rootObject = (GameObject)EditorGUILayout.ObjectField("Root Object", rootObject, typeof(GameObject), true);
        convex = EditorGUILayout.Toggle("Convex", convex);
        skipInvalidMeshes = EditorGUILayout.Toggle("Skip Invalid Meshes", skipInvalidMeshes);

        EditorGUILayout.HelpBox(
            "Skip Invalid Meshes: ignores MeshFilters whose mesh has no triangles (avoids the 'non-degenerate triangle' error).",
            MessageType.Info);

        EditorGUILayout.Space();

        GUI.enabled = rootObject != null;

        if (GUILayout.Button("Add MeshColliders"))
            AddMeshColliders();

        if (GUILayout.Button("Remove All MeshColliders"))
            RemoveMeshColliders();

        GUI.enabled = true;
    }

    private void AddMeshColliders()
    {
        if (rootObject == null)
        {
            Debug.LogError("[AddMeshColliders] No root object assigned.");
            return;
        }

        MeshFilter[] meshFilters = rootObject.GetComponentsInChildren<MeshFilter>(true);
        int added = 0;
        int skipped = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;

            // Skip if no mesh or mesh has no valid triangles
            if (skipInvalidMeshes)
            {
                if (mesh == null)
                {
                    Debug.LogWarning($"[AddMeshColliders] Skipped '{mf.gameObject.name}' — no mesh assigned.", mf.gameObject);
                    skipped++;
                    continue;
                }

                if (mesh.triangles.Length < 3)
                {
                    Debug.LogWarning($"[AddMeshColliders] Skipped '{mf.gameObject.name}' — mesh has no valid triangles.", mf.gameObject);
                    skipped++;
                    continue;
                }
            }

            if (mf.GetComponent<MeshCollider>() != null)
                continue;

            MeshCollider col = Undo.AddComponent<MeshCollider>(mf.gameObject);
            col.sharedMesh = mesh;
            col.convex = convex;
            added++;
        }

        Debug.Log($"[AddMeshColliders] Added {added} MeshColliders, skipped {skipped} invalid meshes under '{rootObject.name}'.");
    }

    private void RemoveMeshColliders()
    {
        if (rootObject == null)
        {
            Debug.LogError("[AddMeshColliders] No root object assigned.");
            return;
        }

        MeshCollider[] colliders = rootObject.GetComponentsInChildren<MeshCollider>(true);
        foreach (MeshCollider col in colliders)
            Undo.DestroyObjectImmediate(col);

        Debug.Log($"[AddMeshColliders] Removed {colliders.Length} MeshColliders from '{rootObject.name}'.");
    }
}