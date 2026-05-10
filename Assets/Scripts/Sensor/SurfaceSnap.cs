using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SurfaceTouchDetector : MonoBehaviour
{
    [Header("Surface")]
    [SerializeField] private string surfaceTag = "SnapSurface";

    [Header("Auto Find")]
    [SerializeField] private string hapticPlayerObjectName = "HapticPlayer";
    [SerializeField] private string newParentObjectName = "SensorAnchor";

    [Header("Optional Direct References")]
    [SerializeField] private HapticImpulsePlayer hapticPlayer;
    [SerializeField] private Transform newParent;

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponentInParent<XRGrabInteractable>();

        if (hapticPlayer == null)
        {
            GameObject hapticObject = GameObject.Find(hapticPlayerObjectName);
            if (hapticObject != null)
                hapticPlayer = hapticObject.GetComponent<HapticImpulsePlayer>();

            if (hapticPlayer == null)
                hapticPlayer = FindFirstObjectByType<HapticImpulsePlayer>();
        }

        if (newParent == null)
        {
            GameObject parentObject = GameObject.Find(newParentObjectName);
            if (parentObject != null)
                newParent = parentObject.transform;
        }

        Debug.Log($"[SurfaceTouch] Grab reference initialized: {(grab != null)}");
        Debug.Log($"[SurfaceTouch] Haptic player found: {(hapticPlayer != null ? hapticPlayer.name : "NULL")}");
        Debug.Log($"[SurfaceTouch] New parent found: {(newParent != null ? newParent.name : "NULL")}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (grab == null)
            return;

        if (!grab.isSelected)
            return;

        if (!other.CompareTag(surfaceTag))
            return;

        Debug.Log("[SurfaceTouch] Sensor touched surface: " + other.name);

        AttachGrabbedObject();
        SendHapticPulse(0.2f, 0.3f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (grab == null)
            return;

        if (!other.CompareTag(surfaceTag))
            return;

        Debug.Log("[SurfaceTouch] Sensor left surface: " + other.name);

        DetachGrabbedObject();
        SendHapticPulse(0.1f, 0.15f);
    }

    private void AttachGrabbedObject()
    {
        if (newParent == null)
        {
            Debug.LogWarning("[SurfaceTouch] newParent is NULL. Cannot parent object.");
            return;
        }

        Transform grabbedRoot = grab.transform;
        grabbedRoot.SetParent(newParent, true);

        Debug.Log("[SurfaceTouch] " + grabbedRoot.name + " is now child of " + newParent.name);
    }

    private void DetachGrabbedObject()
    {
        if (grab == null)
            return;

        Transform grabbedRoot = grab.transform;
        grabbedRoot.SetParent(null, true);

        Debug.Log("[SurfaceTouch] " + grabbedRoot.name + " detached from parent.");
    }

    private void SendHapticPulse(float amplitude, float duration)
    {
        if (hapticPlayer == null)
        {
            Debug.LogWarning("[SurfaceTouch] HapticImpulsePlayer is NULL.");
            return;
        }

        bool success = hapticPlayer.SendHapticImpulse(amplitude, duration);
        Debug.Log("[SurfaceTouch] Haptic success = " + success);
    }
}