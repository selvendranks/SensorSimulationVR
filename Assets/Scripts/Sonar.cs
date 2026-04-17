using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private float fanAngle = 60f;
    [SerializeField] private int raysPerFan = 30;
    [SerializeField] private float maxRayDistance = 50f;

    [Header("Visualisation")]
    [SerializeField] private GameObject hitQuadPrefab;
    [SerializeField] private float hitOffset = 0.02f;
    [SerializeField] private bool clearPreviousHits = true;

    [Header("Line Visuals")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.cyan;

    private readonly List<LineRenderer> lineRenderers = new();
    private readonly List<GameObject> spawnedHits = new();

    private void Start()
    {
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        CreateLineRenderers();
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
        if (clearPreviousHits)
            ClearHitQuads();

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

            LineRenderer lr = lineRenderers[i];

            if (Physics.Raycast(start, direction, out RaycastHit hit, maxRayDistance))
            {
                end = hit.point;
                lr.startColor = hitColor;
                lr.endColor = hitColor;

                if (hitQuadPrefab != null)
                {
                    Vector3 spawnPos = hit.point + hit.normal * hitOffset;
                    Quaternion spawnRot = Quaternion.LookRotation(hit.normal);
                    GameObject quad = Instantiate(hitQuadPrefab, spawnPos, spawnRot);
                    spawnedHits.Add(quad);
                }
            }
            else
            {
                lr.startColor = missColor;
                lr.endColor = missColor;
            }

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    private void ClearHitQuads()
    {
        for (int i = 0; i < spawnedHits.Count; i++)
        {
            if (spawnedHits[i] != null)
                Destroy(spawnedHits[i]);
        }

        spawnedHits.Clear();
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