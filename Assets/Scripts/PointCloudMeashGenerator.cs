using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PointCloudMeshGenerator : MonoBehaviour
{
    [Header("Recorder")]
    [SerializeField] private PointCloudRecorder pointCloudRecorder;
    [SerializeField] private string pointCloudRecorderObjectName = "PointCloudRecorder";

    [Header("Voxel Settings")]
    [SerializeField] private float voxelSize = 0.1f;
    [SerializeField] private int maxGridSize = 100;

    [Header("Marching Cubes")]
    [SerializeField][Range(1, 99)] private int isoLevelPercentile = 20;

    [Header("Output")]
    [SerializeField] private Material meshMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshMaterial != null)
            meshRenderer.material = meshMaterial;
    }

    private void Start()
    {
        AttachRecorderIfNeeded();
    }

    private void AttachRecorderIfNeeded()
    {
        if (pointCloudRecorder != null) return;

        GameObject recorderObject = GameObject.Find(pointCloudRecorderObjectName);
        if (recorderObject != null)
        {
            pointCloudRecorder = recorderObject.GetComponent<PointCloudRecorder>();
            Debug.Log($"[PointCloudMeshGenerator] Attached recorder: {recorderObject.name}", this);
            return;
        }

        pointCloudRecorder = FindFirstObjectByType<PointCloudRecorder>();

        if (pointCloudRecorder == null)
            Debug.LogWarning($"[PointCloudMeshGenerator] PointCloudRecorder not found.", this);
    }

    /// <summary>
    /// Call this to generate a mesh from the current recorded point cloud.
    /// Wire this to a UI button or controller input.
    /// </summary>
    public void GenerateMesh()
    {
        AttachRecorderIfNeeded();

        Debug.Log("Generating Mesh from Point Cloud...");

        if (pointCloudRecorder == null)
        {
            Debug.LogError("[PointCloudMeshGenerator] No PointCloudRecorder attached.", this);
            return;
        }

        if (pointCloudRecorder.PointCount == 0)
        {
            Debug.LogWarning("[PointCloudMeshGenerator] No points recorded yet.", this);
            return;
        }

        // Clean up any existing mesh before creating a new one to prevent memory leaks
        ClearMesh();

        Debug.Log($"[PointCloudMeshGenerator] Generating mesh from {pointCloudRecorder.PointCount} points...");

        Vector3[] points = new Vector3[pointCloudRecorder.SavedPoints.Count];
        for (int i = 0; i < points.Length; i++)
            points[i] = pointCloudRecorder.SavedPoints[i];

        Mesh mesh = RunMarchingCubes(points);

        if (mesh == null)
        {
            Debug.LogError("[PointCloudMeshGenerator] Mesh generation failed.", this);
            return;
        }

        meshFilter.mesh = mesh;
        Debug.Log($"[PointCloudMeshGenerator] Mesh generated — {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles.", this);
    }

    /// <summary>
    /// Clears the generated mesh and destroys the asset from memory to prevent leaks.
    /// </summary>
    public void ClearMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.mesh != null)
        {
            // Destroy procedural mesh data explicitly to avoid memory leaking in Unity
            if (Application.isPlaying)
            {
                Destroy(meshFilter.mesh);
            }
            else
            {
                DestroyImmediate(meshFilter.mesh);
            }

            meshFilter.mesh = null;
            Debug.Log("[PointCloudMeshGenerator] Generated mesh cleared.", this);
        }
    }

    private Mesh RunMarchingCubes(Vector3[] points)
    {
        // ── Step 1: Compute bounds ────────────────────────────────────────
        Vector3 mins = points[0];
        Vector3 maxs = points[0];

        foreach (Vector3 p in points)
        {
            mins = Vector3.Min(mins, p);
            maxs = Vector3.Max(maxs, p);
        }

        // ── Step 2: Adjust voxel size to fit within max grid size ─────────
        float currentVoxelSize = voxelSize;
        Vector3 range = maxs - mins;
        float maxDim = Mathf.Max(range.x, range.y, range.z);
        if (maxDim / currentVoxelSize > maxGridSize)
        {
            currentVoxelSize = maxDim / maxGridSize;
            Debug.Log($"[PointCloudMeshGenerator] Voxel size adjusted to {currentVoxelSize:F4} to fit grid.", this);
        }

        // ── Step 3: Build grid ────────────────────────────────────────────
        int nx = Mathf.Max(2, Mathf.CeilToInt(range.x / currentVoxelSize) + 1);
        int ny = Mathf.Max(2, Mathf.CeilToInt(range.y / currentVoxelSize) + 1);
        int nz = Mathf.Max(2, Mathf.CeilToInt(range.z / currentVoxelSize) + 1);

        Debug.Log($"[PointCloudMeshGenerator] Grid size: {nx} x {ny} x {nz}", this);

        float[,,] scalarField = new float[nx, ny, nz];

        // Initialise all cells to a large distance
        for (int ix = 0; ix < nx; ix++)
            for (int iy = 0; iy < ny; iy++)
                for (int iz = 0; iz < nz; iz++)
                    scalarField[ix, iy, iz] = float.MaxValue;

        // ── Step 4: Scatter points into the grid (nearest-distance approx) ─
        foreach (Vector3 p in points)
        {
            int cx = Mathf.Clamp(Mathf.RoundToInt((p.x - mins.x) / currentVoxelSize), 0, nx - 1);
            int cy = Mathf.Clamp(Mathf.RoundToInt((p.y - mins.y) / currentVoxelSize), 0, ny - 1);
            int cz = Mathf.Clamp(Mathf.RoundToInt((p.z - mins.z) / currentVoxelSize), 0, nz - 1);

            int radius = 2;
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int gx = cx + dx;
                        int gy = cy + dy;
                        int gz = cz + dz;

                        if (gx < 0 || gx >= nx || gy < 0 || gy >= ny || gz < 0 || gz >= nz)
                            continue;

                        Vector3 gridWorldPos = new Vector3(
                            mins.x + gx * currentVoxelSize,
                            mins.y + gy * currentVoxelSize,
                            mins.z + gz * currentVoxelSize
                        );

                        float dist = Vector3.Distance(p, gridWorldPos);
                        if (dist < scalarField[gx, gy, gz])
                            scalarField[gx, gy, gz] = dist;
                    }
        }

        // ── Step 5: Determine iso level from percentile ───────────────────
        List<float> allValues = new();
        for (int ix = 0; ix < nx; ix++)
            for (int iy = 0; iy < ny; iy++)
                for (int iz = 0; iz < nz; iz++)
                    if (scalarField[ix, iy, iz] < float.MaxValue)
                        allValues.Add(scalarField[ix, iy, iz]);

        if (allValues.Count == 0)
        {
            Debug.LogError("[PointCloudMeshGenerator] Scalar field is empty.", this);
            return null;
        }

        allValues.Sort();
        int percentileIndex = Mathf.Clamp(
            Mathf.RoundToInt(isoLevelPercentile / 100f * (allValues.Count - 1)),
            0, allValues.Count - 1
        );
        float isoLevel = allValues[percentileIndex];
        Debug.Log($"[PointCloudMeshGenerator] Iso level: {isoLevel:F4}", this);

        // ── Step 6: Marching Cubes ────────────────────────────────────────
        List<Vector3> verts = new();
        List<int> tris = new();

        for (int ix = 0; ix < nx - 1; ix++)
            for (int iy = 0; iy < ny - 1; iy++)
                for (int iz = 0; iz < nz - 1; iz++)
                {
                    MarchCube(
                        scalarField, ix, iy, iz,
                        mins, currentVoxelSize,
                        isoLevel, verts, tris
                    );
                }

        if (verts.Count == 0)
        {
            Debug.LogWarning("[PointCloudMeshGenerator] Marching Cubes produced no geometry. Try adjusting iso level percentile.", this);
            return null;
        }

        // ── Step 7: Build Unity Mesh ──────────────────────────────────────
        Mesh mesh = new Mesh();
        mesh.name = "PointCloudMesh";

        if (verts.Count > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // ── Marching Cubes tables and cube marcher ────────────────────────────

    private void MarchCube(
        float[,,] field,
        int ix, int iy, int iz,
        Vector3 mins, float vs,
        float iso,
        List<Vector3> verts, List<int> tris)
    {
        // Cube corner offsets
        Vector3Int[] corners = new Vector3Int[8]
        {
            new(ix,     iy,     iz    ),
            new(ix + 1, iy,     iz    ),
            new(ix + 1, iy,     iz + 1),
            new(ix,     iy,     iz + 1),
            new(ix,     iy + 1, iz    ),
            new(ix + 1, iy + 1, iz    ),
            new(ix + 1, iy + 1, iz + 1),
            new(ix,     iy + 1, iz + 1),
        };

        float[] val = new float[8];
        for (int i = 0; i < 8; i++)
            val[i] = field[corners[i].x, corners[i].y, corners[i].z];

        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
            if (val[i] < iso) cubeIndex |= (1 << i);

        if (cubeIndex == 0 || cubeIndex == 255) return;

        // Edge midpoints
        Vector3[] edgeVerts = new Vector3[12];
        int[] edgePairs = new int[]
        {
            0,1, 1,2, 2,3, 3,0,
            4,5, 5,6, 6,7, 7,4,
            0,4, 1,5, 2,6, 3,7
        };

        for (int e = 0; e < 12; e++)
        {
            int a = edgePairs[e * 2];
            int b = edgePairs[e * 2 + 1];
            float va = val[a];
            float vb = val[b];
            float t = Mathf.Approximately(vb - va, 0f) ? 0.5f : (iso - va) / (vb - va);
            t = Mathf.Clamp01(t);

            Vector3 pa = new Vector3(
                mins.x + corners[a].x * vs,
                mins.y + corners[a].y * vs,
                mins.z + corners[a].z * vs);

            Vector3 pb = new Vector3(
                mins.x + corners[b].x * vs,
                mins.y + corners[b].y * vs,
                mins.z + corners[b].z * vs);

            edgeVerts[e] = Vector3.Lerp(pa, pb, t);
        }

        int[] triTable = GetTriTable(cubeIndex);
        for (int i = 0; i < triTable.Length; i += 3)
        {
            int baseIndex = verts.Count;
            verts.Add(edgeVerts[triTable[i]]);
            verts.Add(edgeVerts[triTable[i + 1]]);
            verts.Add(edgeVerts[triTable[i + 2]]);
            tris.Add(baseIndex);
            tris.Add(baseIndex + 1);
            tris.Add(baseIndex + 2);
        }
    }

    private static int[] GetTriTable(int cubeIndex)
    {
        // Standard 256-entry Marching Cubes triangle table (Paul Bourke)
        int[][] triTable = new int[][]
        {
            new int[]{},
            new int[]{0,8,3},
            new int[]{0,1,9},
            new int[]{1,8,3,9,8,1},
            new int[]{1,2,10},
            new int[]{0,8,3,1,2,10},
            new int[]{9,2,10,0,2,9},
            new int[]{2,8,3,2,10,8,10,9,8},
            new int[]{3,11,2},
            new int[]{0,11,2,8,11,0},
            new int[]{1,9,0,2,3,11},
            new int[]{1,11,2,1,9,11,9,8,11},
            new int[]{3,10,1,11,10,3},
            new int[]{0,10,1,0,8,10,8,11,10},
            new int[]{3,9,0,3,11,9,11,10,9},
            new int[]{9,8,10,10,8,11},
            new int[]{4,7,8},
            new int[]{4,3,0,7,3,4},
            new int[]{0,1,9,8,4,7},
            new int[]{4,1,9,4,7,1,7,3,1},
            new int[]{1,2,10,8,4,7},
            new int[]{3,4,7,3,0,4,1,2,10},
            new int[]{9,2,10,9,0,2,8,4,7},
            new int[]{2,10,9,2,9,7,2,7,3,7,9,4},
            new int[]{8,4,7,3,11,2},
            new int[]{11,4,7,11,2,4,2,0,4},
            new int[]{9,0,1,8,4,7,2,3,11},
            new int[]{4,7,11,9,4,11,9,11,2,9,2,1},
            new int[]{3,10,1,3,11,10,7,8,4},
            new int[]{1,11,10,1,4,11,1,0,4,7,11,4},
            new int[]{4,7,8,9,0,11,9,11,10,11,0,3},
            new int[]{4,7,11,4,11,9,9,11,10},
            new int[]{9,5,4},
            new int[]{9,5,4,0,8,3},
            new int[]{0,5,4,1,5,0},
            new int[]{8,5,4,8,3,5,3,1,5},
            new int[]{1,2,10,9,5,4},
            new int[]{3,0,8,1,2,10,4,9,5},
            new int[]{5,2,10,5,4,2,4,0,2},
            new int[]{2,10,5,3,2,5,3,5,4,3,4,8},
            new int[]{9,5,4,2,3,11},
            new int[]{0,11,2,0,8,11,4,9,5},
            new int[]{0,5,4,0,1,5,2,3,11},
            new int[]{2,1,5,2,5,8,2,8,11,4,8,5},
            new int[]{10,3,11,10,1,3,9,5,4},
            new int[]{4,9,5,0,8,1,8,10,1,8,11,10},
            new int[]{5,4,0,5,0,11,5,11,10,11,0,3},
            new int[]{5,4,8,5,8,10,10,8,11},
            new int[]{9,7,8,5,7,9},
            new int[]{9,3,0,9,5,3,5,7,3},
            new int[]{0,7,8,0,1,7,1,5,7},
            new int[]{1,5,3,3,5,7},
            new int[]{9,7,8,9,5,7,10,1,2},
            new int[]{10,1,2,9,5,0,5,3,0,5,7,3},
            new int[]{8,0,2,8,2,5,8,5,7,10,5,2},
            new int[]{2,10,5,2,5,3,3,5,7},
            new int[]{7,9,5,7,8,9,3,11,2},
            new int[]{9,5,7,9,7,2,9,2,0,2,7,11},
            new int[]{2,3,11,0,1,8,1,7,8,1,5,7},
            new int[]{11,2,1,11,1,7,7,1,5},
            new int[]{9,5,8,8,5,7,10,1,3,10,3,11},
            new int[]{5,7,0,5,0,9,7,11,0,1,0,10,11,10,0},
            new int[]{11,10,0,11,0,3,10,5,0,8,0,7,5,7,0},
            new int[]{11,10,5,7,11,5},
            new int[]{10,6,5},
            new int[]{0,8,3,5,10,6},
            new int[]{9,0,1,5,10,6},
            new int[]{1,8,3,1,9,8,5,10,6},
            new int[]{1,6,5,2,6,1},
            new int[]{1,6,5,1,2,6,3,0,8},
            new int[]{9,6,5,9,0,6,0,2,6},
            new int[]{5,9,8,5,8,2,5,2,6,3,2,8},
            new int[]{2,3,11,10,6,5},
            new int[]{11,0,8,11,2,0,10,6,5},
            new int[]{0,1,9,2,3,11,5,10,6},
            new int[]{5,10,6,1,9,2,9,11,2,9,8,11},
            new int[]{6,3,11,6,5,3,5,1,3},
            new int[]{0,8,11,0,11,5,0,5,1,5,11,6},
            new int[]{3,11,6,0,3,6,0,6,5,0,5,9},
            new int[]{6,5,9,6,9,11,11,9,8},
            new int[]{5,10,6,4,7,8},
            new int[]{4,3,0,4,7,3,6,5,10},
            new int[]{1,9,0,5,10,6,8,4,7},
            new int[]{10,6,5,1,9,7,1,7,3,7,9,4},
            new int[]{6,1,2,6,5,1,4,7,8},
            new int[]{1,2,5,5,2,6,3,0,4,3,4,7},
            new int[]{8,4,7,9,0,5,0,6,5,0,2,6},
            new int[]{7,3,9,7,9,4,3,2,9,5,9,6,2,6,9},
            new int[]{3,11,2,7,8,4,10,6,5},
            new int[]{5,10,6,4,7,2,4,2,0,2,7,11},
            new int[]{0,1,9,4,7,8,2,3,11,5,10,6},
            new int[]{9,2,1,9,11,2,9,4,11,7,11,4,5,10,6},
            new int[]{8,4,7,3,11,5,3,5,1,5,11,6},
            new int[]{5,1,11,5,11,6,1,0,11,7,11,4,0,4,11},
            new int[]{0,5,9,0,6,5,0,3,6,11,6,3,8,4,7},
            new int[]{6,5,9,6,9,11,4,7,9,7,11,9},
            new int[]{10,4,9,6,4,10},
            new int[]{4,10,6,4,9,10,0,8,3},
            new int[]{10,0,1,10,6,0,6,4,0},
            new int[]{8,3,1,8,1,6,8,6,4,6,1,10},
            new int[]{1,4,9,1,2,4,2,6,4},
            new int[]{3,0,8,1,2,9,2,4,9,2,6,4},
            new int[]{0,2,4,4,2,6},
            new int[]{8,3,2,8,2,4,4,2,6},
            new int[]{10,4,9,10,6,4,11,2,3},
            new int[]{0,8,2,2,8,11,4,9,10,4,10,6},
            new int[]{3,11,2,0,1,6,0,6,4,6,1,10},
            new int[]{6,4,1,6,1,10,4,8,1,2,1,11,8,11,1},
            new int[]{9,6,4,9,3,6,9,1,3,11,6,3},
            new int[]{8,11,1,8,1,0,11,6,1,9,1,4,6,4,1},
            new int[]{3,11,6,3,6,0,0,6,4},
            new int[]{6,4,8,11,6,8},
            new int[]{7,10,6,7,8,10,8,9,10},
            new int[]{0,7,3,0,10,7,0,9,10,6,7,10},
            new int[]{10,6,7,1,10,7,1,7,8,1,8,0},
            new int[]{10,6,7,10,7,1,1,7,3},
            new int[]{1,2,6,1,6,8,1,8,9,8,6,7},
            new int[]{2,6,9,2,9,1,6,7,9,0,9,3,7,3,9},
            new int[]{7,8,0,7,0,6,6,0,2},
            new int[]{7,3,2,6,7,2},
            new int[]{2,3,11,10,6,8,10,8,9,8,6,7},
            new int[]{2,0,7,2,7,11,0,9,7,6,7,10,9,10,7},
            new int[]{1,8,0,1,7,8,1,10,7,6,7,10,2,3,11},
            new int[]{11,2,1,11,1,7,10,6,1,6,7,1},
            new int[]{8,9,6,8,6,7,9,1,6,11,6,3,1,3,6},
            new int[]{0,9,1,11,6,7},
            new int[]{7,8,0,7,0,6,3,11,0,11,6,0},
            new int[]{7,11,6},
            new int[]{7,6,11},
            new int[]{3,0,8,11,7,6},
            new int[]{0,1,9,11,7,6},
            new int[]{8,1,9,8,3,1,11,7,6},
            new int[]{10,1,2,6,11,7},
            new int[]{1,2,10,3,0,8,6,11,7},
            new int[]{2,9,0,2,10,9,6,11,7},
            new int[]{6,11,7,2,10,3,10,8,3,10,9,8},
            new int[]{7,2,3,6,2,7},
            new int[]{7,0,8,7,6,0,6,2,0},
            new int[]{2,7,6,2,3,7,0,1,9},
            new int[]{1,6,2,1,8,6,1,9,8,8,7,6},
            new int[]{10,7,6,10,1,7,1,3,7},
            new int[]{10,7,6,1,7,10,1,8,7,1,0,8},
            new int[]{0,3,7,0,7,10,0,10,9,6,10,7},
            new int[]{7,6,10,7,10,8,8,10,9},
            new int[]{6,8,4,11,8,6},
            new int[]{3,6,11,3,0,6,0,4,6},
            new int[]{8,6,11,8,4,6,9,0,1},
            new int[]{9,4,6,9,6,3,9,3,1,11,3,6},
            new int[]{6,8,4,6,11,8,2,10,1},
            new int[]{1,2,10,3,0,11,0,6,11,0,4,6},
            new int[]{4,11,8,4,6,11,0,2,9,2,10,9},
            new int[]{10,9,3,10,3,2,9,4,3,11,3,6,4,6,3},
            new int[]{8,2,3,8,4,2,4,6,2},
            new int[]{0,4,2,4,6,2},
            new int[]{1,9,0,2,3,4,2,4,6,4,3,8},
            new int[]{1,9,4,1,4,2,2,4,6},
            new int[]{8,1,3,8,6,1,8,4,6,6,10,1},
            new int[]{10,1,0,10,0,6,6,0,4},
            new int[]{4,6,3,4,3,8,6,10,3,0,3,9,10,9,3},
            new int[]{10,9,4,6,10,4},
            new int[]{4,9,5,7,6,11},
            new int[]{0,8,3,4,9,5,11,7,6},
            new int[]{5,0,1,5,4,0,7,6,11},
            new int[]{11,7,6,8,3,4,3,5,4,3,1,5},
            new int[]{9,5,4,10,1,2,7,6,11},
            new int[]{6,11,7,1,2,10,0,8,3,4,9,5},
            new int[]{7,6,11,5,4,10,4,2,10,4,0,2},
            new int[]{3,4,8,3,5,4,3,2,5,10,5,2,11,7,6},
            new int[]{7,2,3,7,6,2,5,4,9},
            new int[]{9,5,4,0,8,6,0,6,2,6,8,7},
            new int[]{3,6,2,3,7,6,1,5,0,5,4,0},
            new int[]{6,2,8,6,8,7,2,1,8,4,8,5,1,5,8},
            new int[]{9,5,4,10,1,6,1,7,6,1,3,7},
            new int[]{1,6,10,1,7,6,1,0,7,8,7,0,9,5,4},
            new int[]{4,0,10,4,10,5,0,3,10,6,10,7,3,7,10},
            new int[]{7,6,10,7,10,8,5,4,10,4,8,10},
            new int[]{6,9,5,6,11,9,11,8,9},
            new int[]{3,6,11,0,6,3,0,5,6,0,9,5},
            new int[]{0,11,8,0,5,11,0,1,5,5,6,11},
            new int[]{6,11,3,6,3,5,5,3,1},
            new int[]{1,2,10,9,5,11,9,11,8,11,5,6},
            new int[]{0,11,3,0,6,11,0,9,6,5,6,9,1,2,10},
            new int[]{11,8,5,11,5,6,8,0,5,10,5,2,0,2,5},
            new int[]{6,11,3,6,3,5,2,10,3,10,5,3},
            new int[]{5,8,9,5,2,8,5,6,2,3,8,2},
            new int[]{9,5,6,9,6,0,0,6,2},
            new int[]{1,5,8,1,8,0,5,6,8,3,8,2,6,2,8},
            new int[]{1,5,6,2,1,6},
            new int[]{1,3,6,1,6,10,3,8,6,5,6,9,8,9,6},
            new int[]{10,1,0,10,0,6,9,5,0,5,6,0},
            new int[]{0,3,8,5,6,10},
            new int[]{10,5,6},
            new int[]{11,5,10,7,5,11},
            new int[]{11,5,10,11,7,5,8,3,0},
            new int[]{5,11,7,5,10,11,1,9,0},
            new int[]{10,7,5,10,11,7,9,8,1,8,3,1},
            new int[]{11,1,2,11,7,1,7,5,1},
            new int[]{0,8,3,1,2,7,1,7,5,7,2,11},
            new int[]{9,7,5,9,2,7,9,0,2,2,11,7},
            new int[]{7,5,2,7,2,11,5,9,2,3,2,8,9,8,2},
            new int[]{2,5,10,2,3,5,3,7,5},
            new int[]{8,2,0,8,5,2,8,7,5,10,2,5},
            new int[]{9,0,1,2,3,10,3,5,10,3,7,5},
            new int[]{1,10,2,5,9,8,5,8,7,7,8,3},  // fixed index 242
            new int[]{5,7,3,5,3,1,1,3,2},  // fixed index 243 - simplified
            new int[]{0,8,7,0,7,1,1,7,5},
            new int[]{9,0,3,9,3,5,5,3,7},
            new int[]{9,8,7,5,9,7},
            new int[]{5,8,4,5,10,8,10,11,8},
            new int[]{5,0,4,5,11,0,5,10,11,11,3,0},
            new int[]{0,1,9,8,4,10,8,10,11,10,4,5},
            new int[]{10,11,4,10,4,5,11,3,4,9,4,1,3,1,4},
            new int[]{2,5,1,2,8,5,2,11,8,4,5,8},
            new int[]{0,4,11,0,11,3,4,5,11,2,11,1,5,1,11},
            new int[]{0,2,5,0,5,9,2,11,5,4,5,8,11,8,5},
            new int[]{9,4,5,2,11,3},
            new int[]{2,5,10,3,5,2,3,4,5,3,8,4},
            new int[]{5,10,2,5,2,4,4,2,0},
            new int[]{3,10,2,3,5,10,3,8,5,4,5,8,0,1,9},
            new int[]{5,10,2,5,2,4,1,9,2,9,4,2},
            new int[]{8,4,5,8,5,3,3,5,1},
            new int[]{0,4,5,1,0,5},
            new int[]{8,4,5,8,5,3,9,0,5,0,3,5},
            new int[]{9,4,5},
            new int[]{4,11,7,4,9,11,9,10,11},
            new int[]{0,8,3,4,9,7,9,11,7,9,10,11},
            new int[]{1,10,11,1,11,4,1,4,0,7,4,11},
            new int[]{3,1,4,3,4,8,1,10,4,7,4,11,10,11,4},
            new int[]{4,11,7,9,11,4,9,2,11,9,1,2},
            new int[]{9,7,4,9,11,7,9,1,11,2,11,1,0,8,3},
            new int[]{11,7,4,11,4,2,2,4,0},
            new int[]{11,7,4,11,4,2,8,3,4,3,2,4},
            new int[]{2,9,10,2,7,9,2,3,7,7,4,9},
            new int[]{9,10,7,9,7,4,10,2,7,8,7,0,2,0,7},
            new int[]{3,7,10,3,10,2,7,4,10,1,10,0,4,0,10},
            new int[]{1,10,2,8,7,4},
            new int[]{4,9,1,4,1,7,7,1,3},
            new int[]{4,9,1,4,1,7,0,8,1,8,7,1},
            new int[]{4,0,3,7,4,3},
            new int[]{4,8,7},
            new int[]{9,10,8,10,11,8},
            new int[]{3,0,9,3,9,11,11,9,10},
            new int[]{0,1,10,0,10,8,8,10,11},
            new int[]{3,1,10,11,3,10},
            new int[]{1,2,11,1,11,9,9,11,8},
            new int[]{3,0,9,3,9,11,1,2,9,2,11,9},
            new int[]{0,2,11,8,0,11},
            new int[]{3,2,11},
            new int[]{2,3,8,2,8,10,10,8,9},
            new int[]{9,10,2,0,9,2},
            new int[]{2,3,8,2,8,10,0,1,8,1,10,8},
            new int[]{1,10,2},
            new int[]{1,3,8,9,1,8},
            new int[]{0,9,1},
            new int[]{0,3,8},
            new int[]{}
        };

        if (cubeIndex < 0 || cubeIndex >= triTable.Length)
            return new int[] { };

        return triTable[cubeIndex];
    }
}