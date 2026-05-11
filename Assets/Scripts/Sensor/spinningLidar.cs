using System.Collections.Generic;
using UnityEngine;

public class SpinningLidarQuadVisualizer : MonoBehaviour
{
    [Header("LiDAR Parameters")]
    [SerializeField] private int raysPerVerticalLine = 32;
    [SerializeField] private float spinningSpeedDegPerSec = 3600f;
    [SerializeField] private float verticalAngleDeg = 50f;

    [Header("Quad Visualization")]
    [SerializeField] private GameObject quadPrefab;
    [SerializeField] private float quadSize = 0.04f;
    [SerializeField] private int maxPoints = 50000;
    [SerializeField] private Transform pointCloudRoot;
    [SerializeField] private string pointCloudRootObjectName = "PointCloudRoot";

    [Header("Scan Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Optional Shared Settings")]
    [SerializeField] private SpinningLidarGlobalSettings globalSettings;
    [SerializeField] private string globalSettingsObjectName = "SpinningLidarGlobalSettings";

    private readonly List<GameObject> quadPool = new();
    private float currentYawDeg;
    private int nextQuadIndex;

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
        BuildPool();
    }

    private void Update()
    {
        if (globalSettings != null)
        {
            raysPerVerticalLine = globalSettings.RaysPerVerticalLine;
            verticalAngleDeg = globalSettings.VerticalAngleDeg;
            maxDistance = globalSettings.MaxDistance;
        }

        currentYawDeg = Mathf.Repeat(currentYawDeg + spinningSpeedDegPerSec * Time.deltaTime, 360f);
        ScanVerticalFanAtYaw(currentYawDeg);
    }

    private void EnsurePointCloudRoot()
    {
        if (pointCloudRoot != null)
            return;

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
        if (globalSettings != null)
            return;

        GameObject settingsObject = GameObject.Find(globalSettingsObjectName);
        if (settingsObject != null)
        {
            globalSettings = settingsObject.GetComponent<SpinningLidarGlobalSettings>();
        }

        if (globalSettings == null)
        {
            globalSettings = FindFirstObjectByType<SpinningLidarGlobalSettings>();
        }

        Debug.Log(globalSettings != null
            ? $"[{name}] Attached SpinningLidarGlobalSettings: {globalSettings.name}"
            : $"[{name}] SpinningLidarGlobalSettings not found.");
    }

    private void ScanVerticalFanAtYaw(float yawDeg)
    {
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

                nextQuadIndex = (nextQuadIndex + 1) % maxPoints;
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
        quadPool.Clear();

        for (int i = 0; i < maxPoints; i++)
        {
            GameObject quad = Instantiate(quadPrefab, pointCloudRoot);
            quad.name = $"LidarQuad_{i}";
            quad.SetActive(false);
            quadPool.Add(quad);
        }

        nextQuadIndex = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;

        float previewLength = Mathf.Min(maxDistance * 0.25f, 3f);
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