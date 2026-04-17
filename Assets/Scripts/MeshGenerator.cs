using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointCloudMesher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sonar sonar;
    [SerializeField] private Material meshMaterial;

    [Header("Mesh Settings")]
    [SerializeField] private Transform meshParent;
    [SerializeField] private int minimumPointsRequired = 4;
    [SerializeField] private Vector3 meshOffset;
    [SerializeField] private float maxEdgeLength = 2.0f;

    [Header("Input")]
    [SerializeField] private Key buildMeshKey = Key.M;

    [Header("Debug")]
    [SerializeField] private bool verboseDebug = true;
    [SerializeField] private bool buildMeshNow;

    private GameObject generatedMeshObject;

    private void Update()
    {
        if (buildMeshNow)
        {
            buildMeshNow = false;
            Log("Inspector build trigger activated.");
            BuildMesh();
        }

        if (Keyboard.current != null && Keyboard.current[buildMeshKey].wasPressedThisFrame)
        {
            Log($"Build key pressed: {buildMeshKey}");
            BuildMesh();
        }
    }

    public void BuildMesh()
    {
        Log("===== BuildMesh started =====");

        if (sonar == null)
        {
            Debug.LogWarning("PointCloudMesher: Sonar reference is missing.");
            return;
        }

        List<List<Vector3>> scanRows = sonar.GetScanRows();

        if (scanRows == null)
        {
            Debug.LogWarning("PointCloudMesher: Scan rows are null.");
            return;
        }

        Log($"Retrieved scan row count = {scanRows.Count}");

        int totalPointCount = 0;
        for (int r = 0; r < scanRows.Count; r++)
        {
            int rowCount = scanRows[r] != null ? scanRows[r].Count : 0;
            totalPointCount += rowCount;
            Log($"Scan row {r}: point count = {rowCount}");
        }

        Log($"Total structured point count = {totalPointCount}");

        if (totalPointCount < minimumPointsRequired)
        {
            Debug.LogWarning($"PointCloudMesher: Not enough total points to build a mesh. Minimum required = {minimumPointsRequired}, current = {totalPointCount}");
            return;
        }

        if (scanRows.Count < 2)
        {
            Debug.LogWarning("PointCloudMesher: Need at least 2 scan rows to build a mesh.");
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int[]> rowVertexIndices = new List<int[]>();

        for (int r = 0; r < scanRows.Count; r++)
        {
            List<Vector3> row = scanRows[r];

            if (row == null || row.Count == 0)
            {
                rowVertexIndices.Add(new int[0]);
                LogWarning($"Row {r} is empty.");
                continue;
            }

            int[] indices = new int[row.Count];

            for (int i = 0; i < row.Count; i++)
            {
                indices[i] = vertices.Count;
                vertices.Add(row[i]);
            }

            rowVertexIndices.Add(indices);
            Log($"Processed row {r}: vertices added = {row.Count}, total vertices = {vertices.Count}");
        }

        int triangleFaceCount = 0;
        int skippedQuads = 0;

        for (int r = 0; r < rowVertexIndices.Count - 1; r++)
        {
            int[] rowA = rowVertexIndices[r];
            int[] rowB = rowVertexIndices[r + 1];

            int quadCount = Mathf.Min(rowA.Length, rowB.Length) - 1;
            Log($"Connecting row {r} and row {r + 1}: rowA = {rowA.Length}, rowB = {rowB.Length}, quadCount = {quadCount}");

            if (quadCount <= 0)
            {
                LogWarning($"Skipped row pair {r}-{r + 1}: not enough matching points.");
                continue;
            }

            for (int i = 0; i < quadCount; i++)
            {
                Vector3 aPos = vertices[rowA[i]];
                Vector3 bPos = vertices[rowA[i + 1]];
                Vector3 cPos = vertices[rowB[i]];
                Vector3 dPos = vertices[rowB[i + 1]];

                bool firstTriangleValid =
                    Vector3.Distance(aPos, cPos) <= maxEdgeLength &&
                    Vector3.Distance(cPos, bPos) <= maxEdgeLength &&
                    Vector3.Distance(aPos, bPos) <= maxEdgeLength;

                bool secondTriangleValid =
                    Vector3.Distance(bPos, cPos) <= maxEdgeLength &&
                    Vector3.Distance(cPos, dPos) <= maxEdgeLength &&
                    Vector3.Distance(bPos, dPos) <= maxEdgeLength;

                if (firstTriangleValid)
                {
                    triangles.Add(rowA[i]);
                    triangles.Add(rowB[i]);
                    triangles.Add(rowA[i + 1]);
                    triangleFaceCount++;
                }
                else
                {
                    skippedQuads++;
                }

                if (secondTriangleValid)
                {
                    triangles.Add(rowA[i + 1]);
                    triangles.Add(rowB[i]);
                    triangles.Add(rowB[i + 1]);
                    triangleFaceCount++;
                }
                else
                {
                    skippedQuads++;
                }
            }
        }

        Log($"Final vertex count = {vertices.Count}");
        Log($"Final triangle index count = {triangles.Count}");
        Log($"Final triangle face count = {triangleFaceCount}");
        Log($"Skipped triangle candidates due to maxEdgeLength = {skippedQuads}");

        if (vertices.Count < 3 || triangles.Count < 3)
        {
            Debug.LogWarning("PointCloudMesher: Mesh data is incomplete. Not enough valid triangles generated.");
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedPointCloudMesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Log($"Mesh created: bounds center = {mesh.bounds.center}, bounds size = {mesh.bounds.size}");

        if (generatedMeshObject != null)
        {
            Log("Destroying previous generated mesh object.");
            Destroy(generatedMeshObject);
        }

        generatedMeshObject = new GameObject("GeneratedMesh");

        if (meshParent != null)
        {
            generatedMeshObject.transform.SetParent(meshParent);
            generatedMeshObject.transform.localPosition = meshOffset;
            generatedMeshObject.transform.localRotation = Quaternion.identity;
            generatedMeshObject.transform.localScale = Vector3.one;
            Log($"Generated mesh parented to {meshParent.name} with local offset {meshOffset}");
        }
        else
        {
            generatedMeshObject.transform.position = meshOffset;
            Log($"Generated mesh created in world space at offset {meshOffset}");
        }

        MeshFilter meshFilter = generatedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = generatedMeshObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = meshMaterial;

        Log($"Generated mesh world position = {generatedMeshObject.transform.position}");

        if (meshMaterial == null)
        {
            LogWarning("Mesh material is NULL. Mesh may exist but be invisible or pink.");
        }

        Log("===== BuildMesh completed successfully =====");
    }

    private void Log(string message)
    {
        if (verboseDebug)
            Debug.Log($"[PointCloudMesher] {message}", this);
    }

    private void LogWarning(string message)
    {
        if (verboseDebug)
            Debug.LogWarning($"[PointCloudMesher] {message}", this);
    }
}