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

    private readonly List<LineRenderer> lineRenderers = new();

    private void Start()
    {
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
            }

            LineRenderer lr = lineRenderers[i];
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
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