using System.Collections.Generic;
using UnityEngine;

public class PointCloudRecorder : MonoBehaviour
{
    private List<Vector3> savedPoints = new();

    public IReadOnlyList<Vector3> SavedPoints => savedPoints;
    public int PointCount => savedPoints?.Count ?? 0;

    public void RecordPoint(Vector3 point)
    {
        if (savedPoints == null)
            savedPoints = new List<Vector3>();

        savedPoints.Add(point);
    }

    public void SaveCloudData()
    {
        int count = savedPoints?.Count ?? 0;
        Debug.Log($"[PointCloudRecorder] Cloud saved — {count} points stored.", this);
    }

    public void ClearCloudData()
    {
        int count = savedPoints?.Count ?? 0;
        savedPoints?.Clear();
        Debug.Log($"[PointCloudRecorder] Cloud cleared — {count} points removed.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (savedPoints == null || savedPoints.Count == 0)
            return;

        Gizmos.color = Color.green;

        foreach (Vector3 p in savedPoints)
        {
            if (p == null) continue;
            Gizmos.DrawSphere(p, 0.02f);
        }
    }
}