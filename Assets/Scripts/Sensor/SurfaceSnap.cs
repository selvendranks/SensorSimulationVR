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
        if (hapticPlayer != null)
            Debug.Log($"[SurfaceTouch] HapticPlayer full path: {GetFullPath(hapticPlayer.transform)}", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SurfaceTouch] TriggerEnter hit on {gameObject.name}, grab={grab != null}, isSelected={grab?.isSelected}");
        if (grab == null)
        {
            Debug.LogWarning("[SurfaceTouch] grab is NULL — no XRGrabInteractable found in parent.");
            return;
        }

        if (!grab.isSelected)
        {
            Debug.Log("[SurfaceTouch] Object is not selected/held — ignoring trigger.");
            return;
        }

        if (!other.CompareTag(surfaceTag))
        {
            Debug.Log($"[SurfaceTouch] Collider '{other.name}' tag='{other.tag}' does not match surfaceTag='{surfaceTag}' — ignoring.");
            return;
        }

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

        // ← NEW: confirms which object this script is on and which haptic player it's using
        Debug.Log($"[SurfaceTouch] Attempting haptic on '{gameObject.name}' using player '{hapticPlayer.gameObject.name}' " +
                  $"at path: {GetFullPath(hapticPlayer.transform)} | amplitude={amplitude}, duration={duration}", this);

        bool success = hapticPlayer.SendHapticImpulse(amplitude, duration);

        // ← NEW: tells you if the impulse was accepted or silently rejected
        Debug.Log($"[SurfaceTouch] Haptic success = {success} (false = no active interactor on that player)", this);
    }

    // ← NEW helper: prints full hierarchy path of any transform
    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

}