using UnityEngine;
using UnityEngine.InputSystem;

public class UIToggler : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private InputActionReference toggleAction;

    private void OnEnable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed += OnTogglePerformed;
            toggleAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
            toggleAction.action.Disable();
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("[UIToggler] Target object is not assigned.", this);
            return;
        }

        targetObject.SetActive(!targetObject.activeSelf);
        Debug.Log($"[UIToggler] {targetObject.name} is now {(targetObject.activeSelf ? "visible" : "hidden")}.", this);
    }
}