using UnityEngine;

public class SolidLidarGlobalSettings : MonoBehaviour
{
    [SerializeField] private float horizontalAngleDeg = 120f;
    [SerializeField] private float verticalAngleDeg = 40f;
    [SerializeField] private float raysPerDegree = 2f;
    [SerializeField] private float maxDistance = 50f;

    public float HorizontalAngleDeg => horizontalAngleDeg;
    public float VerticalAngleDeg => verticalAngleDeg;
    public float RaysPerDegree => raysPerDegree;
    public float MaxDistance => maxDistance;

    public void SetHorizontalAngleDeg(float value)
    {
        horizontalAngleDeg = Mathf.Clamp(value, 1f, 180f);
        Debug.Log($"[SolidLidarGlobalSettings] HorizontalAngleDeg = {horizontalAngleDeg}", this);
    }

    public void SetVerticalAngleDeg(float value)
    {
        verticalAngleDeg = Mathf.Clamp(value, 1f, 180f);
        Debug.Log($"[SolidLidarGlobalSettings] VerticalAngleDeg = {verticalAngleDeg}", this);
    }

    public void SetRaysPerDegree(float value)
    {
        raysPerDegree = Mathf.Max(0.1f, value);
        Debug.Log($"[SolidLidarGlobalSettings] RaysPerDegree = {raysPerDegree}", this);
    }

    public void SetMaxDistance(float value)
    {
        maxDistance = Mathf.Max(0.1f, value);
        Debug.Log($"[SolidLidarGlobalSettings] MaxDistance = {maxDistance}", this);
    }
}