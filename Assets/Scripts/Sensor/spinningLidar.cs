using System.Collections.Generic;
using UnityEngine;

public class SpinningLidarQuadVisualizer : MonoBehaviour
{
    [Header("LiDAR Parameters")]
    [SerializeField] private int raysPerVerticalLine = 32;
    [SerializeField] private float spinningSpeedDegPerSec = 3600f;
    [SerializeField] private float verticalAngleDeg = 50f;
    [SerializeField] private float maxDistance = 100f;

    [Header("Quad Visualization")]
    [SerializeField] private GameObject quadPrefab;
    [SerializeField] private float quadSize = 0.04f;
    [SerializeField] private int maxPoints = 50000;
    [SerializeField] private Transform pointCloudRoot;
    [SerializeField] private string pointCloudRootObjectName = "PointCloudRoot";

    [Header("Scan Settings")]
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Optional Shared Settings")]
    [SerializeField] private SpinningLidarGlobalSettings globalSettings;
    [SerializeField] private string globalSettingsObjectName = "SpinningLidarGlobalSettings";

    private readonly List<GameObject> quadPool = new();
    private float currentYawDeg;
    private int nextQuadIndex;

    // Change detection
    private int lastRaysPerVerticalLine;
    private float lastVerticalAngleDeg;
    private int lastMaxPoints;

    private bool PoolSizeChanged => quadPool.Count != maxPoints;

    private bool ScanSettingsChanged =>
        raysPerVerticalLine != lastRaysPerVerticalLine ||
        !Mathf.Approximately(verticalAngleDeg, lastVerticalAngleDeg) ||
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
        EnsurePointCloudRoot();
        PullGlobalSettings();
        BuildPool();
        SnapshotSettings();
    }

    private void Update()
    {
        PullGlobalSettings();

        if (ScanSettingsChanged)
        {
            SnapshotSettings();

            if (PoolSizeChanged)
                BuildPool();
        }

        currentYawDeg = Mathf.Repeat(currentYawDeg + spinningSpeedDegPerSec * Time.deltaTime, 360f);
        ScanVerticalFanAtYaw(currentYawDeg);
    }

    private void PullGlobalSettings()
    {
        if (globalSettings == null) return;

        raysPerVerticalLine = globalSettings.RaysPerVerticalLine;
        verticalAngleDeg = globalSettings.VerticalAngleDeg;
        maxDistance = globalSettings.MaxDistance;
    }

    private void SnapshotSettings()
    {
        lastRaysPerVerticalLine = raysPerVerticalLine;
        lastVerticalAngleDeg = verticalAngleDeg;
        lastMaxPoints = maxPoints;
    }

    private void EnsurePointCloudRoot()
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

    private void AttachGlobalSettingsIfNeeded()
    {
        if (globalSettings != null) return;

        GameObject settingsObject = GameObject.Find(globalSettingsObjectName);
        if (settingsObject != null)
            globalSettings = settingsObject.GetComponent<SpinningLidarGlobalSettings>();

        if (globalSettings == null)
            globalSettings = FindFirstObjectByType<SpinningLidarGlobalSettings>();

        Debug.Log(globalSettings != null
            ? $"[{name}] Attached SpinningLidarGlobalSettings: {globalSettings.name}"
            : $"[{name}] SpinningLidarGlobalSettings not found.");
    }

    private void ScanVerticalFanAtYaw(float yawDeg)
    {
        if (quadPool.Count == 0) return;

        Vector3 origin = transform.position;

        for (int i = 0; i < raysPerVerticalLine; i++)
        {
            float t = raysPerVerticalLine <= 1 ? 0.5f : (float)i / (raysPerVerticalLine - 1);
            float pitchDeg = Mathf.Lerp(-verticalAngleDeg * 0.5f, verticalAngleDeg * 0.5f, t);

            Vector3 localDirection = DirectionFromAngles(yawDeg, pitchDeg);
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

        for (int i = 0; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                Destroy(quadPool[i]);
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
        float halfVertical = verticalAngleDeg * 0.5f;

        Vector3 lower = DirectionFromAngles(0f, -halfVertical) * previewLength;
        Vector3 center = DirectionFromAngles(0f, 0f) * previewLength;
        Vector3 upper = DirectionFromAngles(0f, halfVertical) * previewLength;

        Gizmos.DrawLine(Vector3.zero, lower);
        Gizmos.DrawLine(Vector3.zero, center);
        Gizmos.DrawLine(Vector3.zero, upper);
        Gizmos.DrawLine(lower, upper);
    }
}