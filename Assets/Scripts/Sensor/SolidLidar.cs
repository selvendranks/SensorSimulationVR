using System.Collections.Generic;
using UnityEngine;

public class SolidStateLidarQuadVisualizer : MonoBehaviour
{
    [Header("LiDAR Window")]
    [SerializeField] private float horizontalAngleDeg = 120f;
    [SerializeField] private float verticalAngleDeg = 40f;
    [SerializeField] private float raysPerDegree = 2f;

    [Header("Quad Visualization")]
    [SerializeField] private GameObject quadPrefab;
    [SerializeField] private float quadSize = 0.04f;
    [SerializeField] private int maxPoints = 5000;

    [Header("Scan Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float scanHz = 2f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private readonly List<GameObject> quadPool = new();
    private float scanTimer;

    private int HorizontalRayCount => Mathf.Max(1, Mathf.CeilToInt(horizontalAngleDeg * raysPerDegree));
    private int VerticalRayCount => Mathf.Max(1, Mathf.CeilToInt(verticalAngleDeg * raysPerDegree));
    private int TotalRayCount => HorizontalRayCount * VerticalRayCount;
    private int VisualPointCount => Mathf.Min(maxPoints, TotalRayCount);
    private float ScanInterval => scanHz > 0f ? 1f / scanHz : 0f;

    private void Start()
    {
        if (quadPrefab == null)
        {
            Debug.LogError($"{name}: Quad Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        RebuildPool();
        PerformScan();
    }

    private void Update()
    {
        if (scanHz <= 0f)
            return;

        scanTimer += Time.deltaTime;

        if (scanTimer >= ScanInterval)
        {
            scanTimer = 0f;

            if (quadPool.Count != VisualPointCount)
                RebuildPool();

            PerformScan();
        }
    }

    private void PerformScan()
    {
        HideAllQuads();

        Vector3 origin = transform.position;
        int activeQuadIndex = 0;

        for (int v = 0; v < VerticalRayCount; v++)
        {
            float vT = VerticalRayCount <= 1 ? 0.5f : (float)v / (VerticalRayCount - 1);
            float pitch = Mathf.Lerp(-verticalAngleDeg * 0.5f, verticalAngleDeg * 0.5f, vT);

            for (int h = 0; h < HorizontalRayCount; h++)
            {
                float hT = HorizontalRayCount <= 1 ? 0.5f : (float)h / (HorizontalRayCount - 1);
                float yaw = Mathf.Lerp(-horizontalAngleDeg * 0.5f, horizontalAngleDeg * 0.5f, hT);

                Vector3 localDirection = DirectionFromAngles(yaw, pitch);
                Vector3 worldDirection = transform.TransformDirection(localDirection);

                if (Physics.Raycast(origin, worldDirection, out RaycastHit hit, maxDistance, collisionMask))
                {
                    if (activeQuadIndex < quadPool.Count)
                    {
                        GameObject quad = quadPool[activeQuadIndex];
                        quad.transform.position = hit.point;

                        Vector3 toSensor = origin - hit.point;
                        if (toSensor.sqrMagnitude > 0.0001f)
                            quad.transform.rotation = Quaternion.LookRotation(toSensor.normalized);

                        quad.transform.localScale = Vector3.one * quadSize;
                        quad.SetActive(true);
                        activeQuadIndex++;
                    }
                }
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

    private void RebuildPool()
    {
        for (int i = 0; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                Destroy(quadPool[i]);
        }

        quadPool.Clear();

        for (int i = 0; i < VisualPointCount; i++)
        {
            GameObject quad = Instantiate(quadPrefab, transform);
            quad.name = $"LidarQuad_{i}";
            quad.SetActive(false);
            quadPool.Add(quad);
        }
    }

    private void HideAllQuads()
    {
        for (int i = 0; i < quadPool.Count; i++)
        {
            if (quadPool[i] != null)
                quadPool[i].SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;

        float previewLength = Mathf.Min(maxDistance * 0.25f, 3f);

        Vector3 bottomLeft = DirectionFromAngles(
            -horizontalAngleDeg * 0.5f,
            -verticalAngleDeg * 0.5f
        ) * previewLength;

        Vector3 bottomRight = DirectionFromAngles(
            horizontalAngleDeg * 0.5f,
            -verticalAngleDeg * 0.5f
        ) * previewLength;

        Vector3 topRight = DirectionFromAngles(
            horizontalAngleDeg * 0.5f,
            verticalAngleDeg * 0.5f
        ) * previewLength;

        Vector3 topLeft = DirectionFromAngles(
            -horizontalAngleDeg * 0.5f,
            verticalAngleDeg * 0.5f
        ) * previewLength;

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