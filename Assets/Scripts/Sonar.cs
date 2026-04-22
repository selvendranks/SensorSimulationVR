using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private float fanAngle = 60f;
    [SerializeField] private int raysPerFan = 30;
    [SerializeField] private float maxRayDistance = 50f;
    [SerializeField] private MoveAlongX shipMovement;

    [Header("Line Visuals")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Hit Point Visuals")]
    [SerializeField] private Material hitQuadMaterial;
    [SerializeField] private float hitQuadSize = 0.05f;
    [SerializeField] private Transform hitPointsParent;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool verboseDebug = false;

    private readonly List<LineRenderer> lineRenderers = new();
    private readonly List<GameObject> spawnedHitQuads = new();
    private readonly List<Vector3> hitPoints = new();
    private readonly List<List<Vector3>> scanRows = new();

    private bool wasScanningLastFrame;

    private void Start()
    {
        CreateLineRenderers();

        if (shipMovement == null)
            shipMovement = GetComponentInParent<MoveAlongX>();

        if (hitPointsParent == null)
        {
            GameObject parentObj = new GameObject("SonarHitPoints");
            hitPointsParent = parentObj.transform;
        }

        HideSonarLines();
    }

    private void Update()
    {
        if (shipMovement == null)
            return;

        if (!shipMovement.IsMoving)
        {
            if (wasScanningLastFrame)
            {
                HideSonarLines();
                wasScanningLastFrame = false;
                Log("Ship stopped or has not started yet. Sonar hidden.");
            }

            return;
        }

        if (!wasScanningLastFrame)
        {
            ShowSonarLines();
            wasScanningLastFrame = true;
            Log("Ship started moving. Sonar scanning started.");
        }

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
            lr.enabled = false;

            lineRenderers.Add(lr);
        }
    }

    private void ScanFan()
    {
        if (lineRenderers.Count != raysPerFan)
            CreateLineRenderers();

        float halfAngle = fanAngle * 0.5f;
        List<Vector3> currentRow = new List<Vector3>();

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

                if (TryRegisterHitPoint(hit, out Vector3 acceptedPoint))
                {
                    currentRow.Add(acceptedPoint);
                }
            }

            LineRenderer lr = lineRenderers[i];
            lr.enabled = true;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        if (currentRow.Count >= 2)
        {
            scanRows.Add(currentRow);
            Log($"Stored scan row with {currentRow.Count} points. Total rows: {scanRows.Count}");
        }
    }

    private bool TryRegisterHitPoint(RaycastHit hit, out Vector3 acceptedPoint)
    {
        acceptedPoint = hit.point;
        Vector3 hitPoint = hit.point;

        for (int i = 0; i < hitPoints.Count; i++)
        {
            if (Vector3.Distance(hitPoints[i], hitPoint) < minDistanceBetweenPoints)
                return false;
        }

        hitPoints.Add(hitPoint);
        SpawnHitQuad(hit);
        return true;
    }

    private void SpawnHitQuad(RaycastHit hit)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "SonarHitQuad";
        quad.transform.SetParent(hitPointsParent);
        quad.transform.position = hit.point;
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

    private void HideSonarLines()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
                lineRenderers[i].enabled = false;
        }
    }

    private void ShowSonarLines()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
                lineRenderers[i].enabled = true;
        }
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
        scanRows.Clear();

        Log("Cleared hit quads, hit points, and scan rows.");
    }

    public List<Vector3> GetHitPoints()
    {
        return new List<Vector3>(hitPoints);
    }

    public List<List<Vector3>> GetScanRows()
    {
        List<List<Vector3>> copy = new List<List<Vector3>>();

        for (int i = 0; i < scanRows.Count; i++)
        {
            copy.Add(new List<Vector3>(scanRows[i]));
        }

        return copy;
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

    private void Log(string message)
    {
        if (verboseDebug)
            Debug.Log($"[Sonar] {message}", this);
    }
}