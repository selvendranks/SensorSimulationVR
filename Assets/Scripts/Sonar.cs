using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private float fanAngle = 60f;
    [SerializeField] private int raysPerFan = 30;
    [SerializeField] private float maxRayDistance = 50f;

    [Header("Line Visuals")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Hit Point Visuals")]
    [SerializeField] private Material hitQuadMaterial;
    [SerializeField] private float hitQuadSize = 0.05f;
    [SerializeField] private Transform hitPointsParent;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;

    private readonly List<LineRenderer> lineRenderers = new();
    private readonly List<GameObject> spawnedHitQuads = new();
    private readonly List<Vector3> hitPoints = new();

    private void Start()
    {
        CreateLineRenderers();

        if (hitPointsParent == null)
        {
            GameObject parentObj = new GameObject("SonarHitPoints");
            hitPointsParent = parentObj.transform;
        }
    }

    private void Update()
    {
        ScanFan();
    }

    private void CreateLineRenderers()
    {
        ClearLineRenderers();

        raysPerFan = Mathf.Max(2, raysPerFan);

        for (int i = 0; i < raysPerFan; i++)
        {
            GameObject lineObj = new GameObject("SonarRay_" + i);
            lineObj.transform.SetParent(transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            lineRenderers.Add(lr);
        }
    }

    private void ScanFan()
    {
        if (lineRenderers.Count != raysPerFan)
            CreateLineRenderers();

        float halfAngle = fanAngle * 0.5f;

        for (int i = 0; i < raysPerFan; i++)
        {
            float t = i / (float)(raysPerFan - 1);
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

            Vector3 direction = Quaternion.AngleAxis(currentAngle, transform.forward) * (-transform.up);
            direction.Normalize();

            Vector3 start = transform.position;
            Vector3 end = start + direction * maxRayDistance;

            if (Physics.Raycast(start, direction, out RaycastHit hit, maxRayDistance))
            {
                end = hit.point;
                TrySpawnHitQuad(hit);
            }

            LineRenderer lr = lineRenderers[i];
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    private void TrySpawnHitQuad(RaycastHit hit)
    {
        Vector3 hitPoint = hit.point;

        for (int i = 0; i < hitPoints.Count; i++)
        {
            if (Vector3.Distance(hitPoints[i], hitPoint) < minDistanceBetweenPoints)
                return;
        }

        hitPoints.Add(hitPoint);

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "SonarHitQuad";
        quad.transform.SetParent(hitPointsParent);
        quad.transform.position = hitPoint;
        quad.transform.localScale = Vector3.one * hitQuadSize;

        quad.transform.rotation = Quaternion.LookRotation(hit.normal);

        Collider col = quad.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = quad.GetComponent<Renderer>();
        if (renderer != null && hitQuadMaterial != null)
            renderer.material = hitQuadMaterial;

        spawnedHitQuads.Add(quad);
    }

    public void ClearHitQuads()
    {
        for (int i = 0; i < spawnedHitQuads.Count; i++)
        {
            if (spawnedHitQuads[i] != null)
                Destroy(spawnedHitQuads[i]);
        }

        spawnedHitQuads.Clear();
        hitPoints.Clear();
    }

    public List<Vector3> GetHitPoints()
    {
        return new List<Vector3>(hitPoints);
    }

    private void ClearLineRenderers()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
                Destroy(lineRenderers[i].gameObject);
        }

        lineRenderers.Clear();
    }
}