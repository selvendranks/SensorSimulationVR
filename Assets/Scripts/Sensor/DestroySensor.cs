using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DestroySelectedByRay : MonoBehaviour
{
    [SerializeField] private InputActionReference destroyAction;
    [SerializeField] private XRRayInteractor rayInteractor;

    private void Awake()
    {
        if (rayInteractor == null)
            rayInteractor = GetComponent<XRRayInteractor>();

        Debug.Log($"[{name}] RayInteractor = {(rayInteractor != null ? "FOUND" : "NULL")}");
    }

    private void OnEnable()
    {
        if (destroyAction != null && destroyAction.action != null)
        {
            destroyAction.action.Enable();
            Debug.Log($"[{name}] destroyAction enabled: {destroyAction.action.name}");
        }
    }

    private void OnDisable()
    {
        if (destroyAction != null && destroyAction.action != null)
            destroyAction.action.Disable();
    }

    private void Update()
    {
        if (rayInteractor == null || destroyAction == null || destroyAction.action == null)
            return;

        if (!destroyAction.action.WasPressedThisFrame())
            return;

        if (!rayInteractor.hasSelection)
        {
            Debug.Log($"[{name}] Trigger pressed, but ray has no selection.");
            return;
        }

        var interactable = rayInteractor.firstInteractableSelected as XRGrabInteractable;

        if (interactable == null)
        {
            Debug.Log($"[{name}] Selected object is not an XRGrabInteractable.");
            return;
        }

        Debug.Log($"[{name}] Destroying: {interactable.gameObject.name}");

        if (interactable.interactionManager != null)
        {
            interactable.interactionManager.SelectExit(
                (IXRSelectInteractor)rayInteractor,
                (IXRSelectInteractable)interactable
            );
        }

        Destroy(interactable.gameObject);
    }
}