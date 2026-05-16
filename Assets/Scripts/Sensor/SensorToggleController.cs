using UnityEngine;

public class SensorVisualizerToggleController : MonoBehaviour
{
    [SerializeField] private Transform sensorAnchor;

    public void SetAllSensorVisualizersEnabled(bool isEnabled)
    {
        if (sensorAnchor == null)
        {
            Debug.LogWarning("[SensorVisualizerToggleController] SensorAnchor is not assigned.", this);
            return;
        }

        var spinningLidars = sensorAnchor.GetComponentsInChildren<SpinningLidarQuadVisualizer>(true);
        var solidLidars = sensorAnchor.GetComponentsInChildren<SolidStateLidarQuadVisualizer>(true);

        foreach (var lidar in spinningLidars)
        {
            lidar.enabled = isEnabled;
        }

        foreach (var lidar in solidLidars)
        {
            lidar.enabled = isEnabled;
        }

        Debug.Log($"[SensorVisualizerToggleController] Set visualizers enabled = {isEnabled}. Spinning: {spinningLidars.Length}, Solid: {solidLidars.Length}", this);
    }
}