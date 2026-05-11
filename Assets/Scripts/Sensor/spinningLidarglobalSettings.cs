using UnityEngine;

public class SpinningLidarGlobalSettings : MonoBehaviour
{
    [SerializeField] private int raysPerVerticalLine = 32;
    [SerializeField] private float verticalAngleDeg = 50f;
    [SerializeField] private float maxDistance = 50f;

    public int RaysPerVerticalLine => raysPerVerticalLine;
    public float VerticalAngleDeg => verticalAngleDeg;
    public float MaxDistance => maxDistance;

    public void SetRaysPerVerticalLine(float value)
    {
        raysPerVerticalLine = Mathf.Max(1, Mathf.RoundToInt(value));
        Debug.Log($"RaysPerVerticalLine = {raysPerVerticalLine}");
    }

    public void SetVerticalAngleDeg(float value)
    {
        verticalAngleDeg = Mathf.Clamp(value, 1f, 180f);
        Debug.Log($"VerticalAngleDeg = {verticalAngleDeg}");
    }

    public void SetMaxDistance(float value)
    {
        maxDistance = Mathf.Max(0.1f, value);
        Debug.Log($"MaxDistance = {maxDistance}");
    }
}