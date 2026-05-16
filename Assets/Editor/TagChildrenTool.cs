using UnityEngine;
using UnityEditor;

public class TagChildrenTool : EditorWindow
{
    [SerializeField] private GameObject parentObject;
    [SerializeField] private string snapSurfaceTag = "SnapSurface";

    [MenuItem("Tools/Surface Snapping/Tag Children")]
    public static void ShowWindow()
    {
        GetWindow<TagChildrenTool>("Tag Children Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tag All Children", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        snapSurfaceTag = EditorGUILayout.TextField("Snap Surface Tag", snapSurfaceTag);

        if (GUILayout.Button("Apply Tag to All Children"))
        {
            ApplyTag();
        }
    }

    private void ApplyTag()
    {
        if (parentObject == null)
        {
            Debug.LogWarning("[TagChildrenTool] No parent object assigned.");
            return;
        }

        Transform[] children = parentObject.GetComponentsInChildren<Transform>(true);
        int count = 0;

        foreach (Transform child in children)
        {
            if (child == parentObject.transform)
                continue;

            child.gameObject.tag = snapSurfaceTag;
            count++;
        }

        Debug.Log($"[TagChildrenTool] Applied tag '{snapSurfaceTag}' to {count} children of '{parentObject.name}'.");
    }
}
