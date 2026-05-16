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

    private int HorizontalRayCount => Mathf.Max(1, Mathf.CeilToInt(horizontalAngleDeg * raysPerDegree));
    private int VerticalRayCount => Mathf.Max(1, Mathf.CeilToInt(verticalAngleDeg * raysPerDegree));
    private float ScanInterval => scanHz > 0f ? 1f / scanHz : 0f;

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
        BuildPool();
    }

    private void Update()
    {
        PullGlobalSettings();

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

    private void ScanNextRows()
    {
        if (VerticalRayCount <= 0 || HorizontalRayCount <= 0) return;

        Vector3 origin = transform.position;
        int rowsThisStep = Mathf.Max(1, rowsPerUpdate);

        for (int rowStep = 0; rowStep < rowsThisStep; rowStep++)
        {
            if (currentRow >= VerticalRayCount)
                currentRow = 0;

            float vT = VerticalRayCount <= 1 ? 0.5f : (float)currentRow / (VerticalRayCount - 1);
            float pitch = Mathf.Lerp(verticalAngleDeg * 0.5f, -verticalAngleDeg * 0.5f, vT);

            for (int h = 0; h < HorizontalRayCount; h++)
            {
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

                    nextQuadIndex = (nextQuadIndex + 1) % quadPool.Count;
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

    private void BuildPool()
    {
        if (pointCloudRoot == null)
        {
            Debug.LogError($"[{name}] Cannot build pool — PointCloudRoot is missing.", this);
            enabled = false;
            return;
        }

        quadPool.Clear();

        for (int i = 0; i < maxPoints; i++)
        {
            GameObject quad = Instantiate(quadPrefab, pointCloudRoot);
            quad.name = $"LidarQuad_{i}";
            quad.SetActive(false);
            quadPool.Add(quad);
        }

        nextQuadIndex = 0;
        Debug.Log($"[{name}] Built quad pool with {maxPoints} points.", this);
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