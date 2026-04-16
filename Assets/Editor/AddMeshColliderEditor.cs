using UnityEngine;
using UnityEditor;

public class AddMeshCollidersEditor : EditorWindow
{
    private GameObject rootObject;

    [MenuItem("Tools/Add MeshColliders To Children")]
    public static void ShowWindow()
    {
        GetWindow<AddMeshCollidersEditor>("Add MeshColliders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add MeshColliders to all MeshFilters under a root object", EditorStyles.boldLabel);

        rootObject = (GameObject)EditorGUILayout.ObjectField("Root Object", rootObject, typeof(GameObject), true);

        if (GUILayout.Button("Add MeshColliders"))
        {
            if (rootObject == null)
            {
                Debug.LogError("No root object assigned.");
                return;
            }

            AddMeshColliders();
        }
    }

    private void AddMeshColliders()
    {
        MeshFilter[] meshFilters = rootObject.GetComponentsInChildren<MeshFilter>(true);
        int count = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.GetComponent<MeshCollider>() == null)
            {
                Undo.AddComponent<MeshCollider>(mf.gameObject);
                count++;
            }
        }

        Debug.Log($"Added MeshColliders to {count} objects under {rootObject.name}.");
    }
}
