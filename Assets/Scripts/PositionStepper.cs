using UnityEngine;
using UnityEngine.InputSystem;

public class PositionStepper : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [Header("Positions")]
    [SerializeField] private GameObject[] positionMarkers;

    [Header("Input")]
    [SerializeField] private InputActionReference stepInputAction;

    [Tooltip("How far the thumbstick must move from center (0=center, 1=fully pushed). 0.8 = 80% away from center.")]
    [SerializeField] private float stickDeadzone = 0.8f;

    [Header("Settings")]
    [SerializeField] private bool loop = false;
    [SerializeField] private float moveDuration = 0.3f;

    private int currentIndex = -1;
    private bool isMoving = false;
    private bool wasTriggered = false;

    private void OnEnable()
    {
        if (stepInputAction != null)
        {
            stepInputAction.action.Enable();
            stepInputAction.action.performed += OnStepPerformed;
            stepInputAction.action.canceled += OnStepCanceled;
        }
    }

    private void OnDisable()
    {
        if (stepInputAction != null)
        {
            stepInputAction.action.performed -= OnStepPerformed;
            stepInputAction.action.canceled -= OnStepCanceled;
        }
    }

    private void OnStepPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 stick = ctx.ReadValue<Vector2>();
        float distanceFromCenter = stick.magnitude;

        Debug.Log($"[PositionStepper] Stick distance from center: {distanceFromCenter:F2}");

        if (distanceFromCenter >= stickDeadzone && !wasTriggered)
        {
            wasTriggered = true;
            TryStep();
        }
    }

    private void OnStepCanceled(InputAction.CallbackContext ctx)
    {
        wasTriggered = false;
        Debug.Log("[PositionStepper] Stick returned to center — ready for next step.");
    }

    private void TryStep()
    {
        if (isMoving) return;

        if (positionMarkers == null || positionMarkers.Length == 0)
        {
            Debug.LogWarning("[PositionStepper] No position markers defined.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogWarning("[PositionStepper] No target object assigned.");
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= positionMarkers.Length)
        {
            if (loop)
                nextIndex = 0;
            else
            {
                Debug.Log("[PositionStepper] Reached last position.");
                return;
            }
        }

        // Skip null markers
        if (positionMarkers[nextIndex] == null)
        {
            Debug.LogWarning($"[PositionStepper] Marker at index {nextIndex} is NULL — skipping.");
            return;
        }

        currentIndex = nextIndex;
        Vector3 targetPos = positionMarkers[currentIndex].transform.position;
        StartCoroutine(MoveToPosition(targetPos));
        Debug.Log($"[PositionStepper] Moving to marker [{currentIndex}]: {positionMarkers[currentIndex].name} at {targetPos}");
    }

    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;

        Vector3 startPos = targetObject.transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            targetObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        targetObject.transform.position = targetPos;
        isMoving = false;
    }
}