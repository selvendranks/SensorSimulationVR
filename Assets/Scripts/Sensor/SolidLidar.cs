using System.Collections.Generic;
using UnityEngine;

public class SolidStateLidarQuadVisualizer : MonoBehaviour
{
    [Header("LiDAR Window")]
    [SerializeField] private float horizontalAngleDeg = 120f;
    [SerializeField] private float verticalAngleDeg = 40f;
    [SerializeField] private float raysPerDegree = 2f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Quad Visualization")]
    [SerializeField] private GameObject quadPrefab;
    [SerializeField] private float quadSize = 0.04f;
    [SerializeField] private int maxPoints = 5000;
    [SerializeField] private Transform pointCloudRoot;
    [SerializeField] private string pointCloudRootObjectName = "PointCloudRoot";

    [Header("Scan Settings")]
    [SerializeField] private float scanHz = 2f;
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Progressive Scan")]
    [SerializeField] private int rowsPerUpdate = 1;

    [Header("Optional Shared Settings")]
    [SerializeField] private SolidLidarGlobalSettings globalSettings;
    [SerializeField] private string globalSettingsObjectName = "SolidLidarGlobalSettings";

    private readonly List<GameObject> quadPool = new();
    private float scanTimer;
    private int currentRow;
    private int nextQuadIndex;

    // Change detection
    private float lastHorizontalAngleDeg;
    private float lastVerticalAngleDeg;
    private float lastRaysPerDegree;
    private int lastMaxPoints;

    private int HorizontalRayCount => Mathf.Max(1, Mathf.CeilToInt(horizontalAngleDeg * raysPerDegree));
    private int VerticalRayCount => Mathf.Max(1, Mathf.CeilToInt(verticalAngleDeg * raysPerDegree));
    private int TotalRayCount => HorizontalRayCount * VerticalRayCount;
    private int VisualPointCount => Mathf.Min(maxPoints, TotalRayCount);
    private float ScanInterval => scanHz > 0f ? 1f / scanHz : 0f;

    private bool PoolSizeChanged =>
        quadPool.Count != VisualPointCount;

    private bool ScanSettingsChanged =>
        !Mathf.Approximately(horizontalAngleDeg, lastHorizontalAngleDeg) ||
        !Mathf.Approximately(verticalAngleDeg, lastVerticalAngleDeg) ||
        !Mathf.Approximately(raysPerDegree, lastRaysPerDegree) ||
        maxPoints != lastMaxPoints;

    private void Start()
    {
        if (quadPrefab == null)
        {
            Debug.LogError($"{name}: Quad Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        AttachGlobalSettingsIfNeeded();
        AttachPointCloudRootIfNeeded();
        PullGlobalSettings();
        RebuildPool();
        SnapshotSettings();
        BeginNewSweep();
    }

    private void Update()
    {
        PullGlobalSettings();

        if (ScanSettingsChanged)
        {
            bool needsRebuild = PoolSizeChanged;

            SnapshotSettings();

            if (needsRebuild)
                RebuildPool();

            HideAllQuads();
            BeginNewSweep();
            return;
        }

        if (scanHz <= 0f)
            return;

        scanTimer += Time.deltaTime;

        if (scanTimer >= ScanInterval)
        {
            scanTimer = 0f;
            ScanNextRows();
        }
    }

    private void PullGlobalSettings()
    {
        if (globalSettings == null) return;

        horizontalAngleDeg = globalSettings.HorizontalAngleDeg;
        verticalAngleDeg = globalSettings.VerticalAngleDeg;
        raysPerDegree = globalSettings.RaysPerDegree;
        maxDistance = globalSettings.MaxDistance;
    }

    private void SnapshotSettings()
    {
        lastHorizontalAngleDeg = horizontalAngleDeg;
        lastVerticalAngleDeg = verticalAngleDeg;
        lastRaysPerDegree = raysPerDegree;
        lastMaxPoints = maxPoints;
    }

    private void AttachGlobalSettingsIfNeeded()
    {
        if (globalSettings != null) return;

        GameObject settingsObject = GameObject.Find(globalSettingsObjectName);
        if (settingsObject != null)
            globalSettings = settingsObject.GetComponent<SolidLidarGlobalSettings>();

        if (globalSettings == null)
            globalSettings = FindFirstObjectByType<SolidLidarGlobalSettings>();

        Debug.Log(
            globalSettings != null
                ? $"[{name}] Attached SolidLidarGlobalSettings: {globalSettings.name}"
                : $"[{name}] SolidLidarGlobalSettings not found.",
            this
        );
    }

    private void AttachPointCloudRootIfNeeded()
    {
        if (pointCloudRoot != null) return;

        GameObject rootObject = GameObject.Find(pointCloudRootObjectName);
        if (rootObject != null)
        {
            pointCloudRoot = rootObject.transform;
            Debug.Log($"[{name}] Attached PointCloudRoot: {pointCloudRoot.name}", this);
            return;
        }

        Debug.LogWarning($"[{name}] PointCloudRoot object '{pointCloudRootObjectName}' not found in scene.", this);
    }

    private void BeginNewSweep()
    {
        currentRow = 0;
        nextQuadIndex = 0;
    }

    private void ScanNextRows()
    {
        if (VerticalRayCount <= 0 || HorizontalRayCount <= 0) return;

        Vector3 origin = transform.position;
        int rowsThisStep = Mathf.Max(1, rowsPerUpdate);

        for (int rowStep = 0; rowStep < rowsThisStep; rowStep++)
        {
            if (currentRow >= VerticalRayCount)
            {
                HideQuadsFrom(nextQuadIndex);
                BeginNewSweep();
            }

            float vT = VerticalRayCount <= 1 ? 0.5f : (float)currentRow / (VerticalRayCount - 1);
            float pitch = Mathf.Lerp(verticalAngleDeg * 0.5f, -verticalAngleDeg * 0.5f, vT);

            for (int h = 0; h < HorizontalRayCount; h++)
            {
                if (nextQuadIndex >= quadPool.Count)
                {
                    currentRow = VerticalRayCount;
                    return;
                }

                float hT = HorizontalRayCount <= 1 ? 0.5f : (float)h / (HorizontalRayCount - 1);
                float yaw = Mathf.Lerp(-horizontalAngleDeg * 0.5f, horizontalAngleDeg * 0.5f, hT);

                Vector3 localDirection = DirectionFromAngles(yaw, pitch);
                Vector3 worldDirection = transform.TransformDirection(localDirection);

                if (Physics.Raycast(origin, worldDirection, out RaycastHit hit, maxDistance, collisionMask))
                {
                    GameObject quad = quadPool[nextQuadIndex];
                    quad.transform.position = hit.point;

                    Vector3 toSensor = origin - hit.point;
                    if (toSensor.sqrMagnitude > 0.0001f)
                        quad.transform.rotation = Quaternion.LookRotation(toSensor.normalized);

                    quad.transform.localScale = Vector3.one * quadSize;
                    quad.SetActive(true);
                    nextQuadIndex++;
                }
            }

            currentRow++;
        }
    }

    private Vector3 DirectionFromAngles(float yawDeg, float pitchDeg)
    {
        float yawRad = yawDeg * Mathf.Deg2Rad;
        float pitchRad = pitchDeg * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
        ).normalized;
    }

    private void RebuildPool()
    {
        if (pointCloudRoot == null)
        {
            Debug.LogError($"[{name}] Cannot rebuild pool — PointCloudRoot is missing.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                Destroy(quadPool[i]);
        }

        quadPool.Clear();

        for (int i = 0; i < VisualPointCount; i++)
        {
            GameObject quad = Instantiate(quadPrefab, pointCloudRoot);
            quad.name = $"LidarQuad_{i}";
            quad.SetActive(false);
            quadPool.Add(quad);
        }

        Debug.Log($"[{name}] Rebuilt quad pool with {VisualPointCount} points.", this);
    }

    private void HideAllQuads()
    {
        for (int i = 0; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                quadPool[i].SetActive(false);
        }
    }

    private void HideQuadsFrom(int startIndex)
    {
        for (int i = startIndex; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                quadPool[i].SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;

        const float previewLength = 3f;

        Vector3 bottomLeft = DirectionFromAngles(-horizontalAngleDeg * 0.5f, -verticalAngleDeg * 0.5f) * previewLength;
        Vector3 bottomRight = DirectionFromAngles(horizontalAngleDeg * 0.5f, -verticalAngleDeg * 0.5f) * previewLength;
        Vector3 topRight = DirectionFromAngles(horizontalAngleDeg * 0.5f, verticalAngleDeg * 0.5f) * previewLength;
        Vector3 topLeft = DirectionFromAngles(-horizontalAngleDeg * 0.5f, verticalAngleDeg * 0.5f) * previewLength;

        Gizmos.DrawLine(Vector3.zero, bottomLeft);
        Gizmos.DrawLine(Vector3.zero, bottomRight);
        Gizmos.DrawLine(Vector3.zero, topRight);
        Gizmos.DrawLine(Vector3.zero, topLeft);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}