using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class SurfaceTouchDetector : MonoBehaviour
{
    public string surfaceTag = "SnapSurface";

    [SerializeField] private HapticImpulsePlayer hapticPlayer;
    [SerializeField] private Transform newParent;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Start()
    {
        grab = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        Debug.Log("[SurfaceTouch] Grab reference initialized.");
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
            Debug.Log("[SurfaceTouch] newParent is NULL. Cannot parent object.");
            return;
        }

        Transform grabbedRoot = grab.transform;
        grabbedRoot.SetParent(newParent, true);

        Debug.Log("[SurfaceTouch] " + grabbedRoot.name + " is now child of " + newParent.name);
    }

    private void DetachGrabbedObject()
    {
        Transform grabbedRoot = grab.transform;
        grabbedRoot.SetParent(null, true);

        Debug.Log("[SurfaceTouch] " + grabbedRoot.name + " detached from parent.");
    }

    private void SendHapticPulse(float amplitude, float duration)
    {
        if (hapticPlayer == null)
        {
            Debug.Log("[SurfaceTouch] HapticImpulsePlayer is NULL.");
            return;
        }

        bool success = hapticPlayer.SendHapticImpulse(amplitude, duration);
        Debug.Log("[SurfaceTouch] Haptic success = " + success);
    }
}