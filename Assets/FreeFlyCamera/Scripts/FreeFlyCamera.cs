using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeFlyCamera : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool activeCamera = true;

    [Header("Rotation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("Zoom")]
    [SerializeField] private bool enableScrollTranslation = true;
    [SerializeField] private float scrollSpeed = 10f;

    [Header("Movement")]
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float boostedSpeed = 20f;

    [Header("Vertical Movement")]
    [SerializeField] private Key moveUpKey = Key.E;
    [SerializeField] private Key moveDownKey = Key.Q;
    [SerializeField] private Key boostKey = Key.LeftShift;

    [Header("Acceleration")]
    [SerializeField] private bool enableSpeedAcceleration = true;
    [SerializeField] private float speedAccelerationFactor = 1.5f;

    [Header("Reset")]
    [SerializeField] private Key resetKey = Key.R;

    private CursorLockMode wantedMode = CursorLockMode.Locked;
    private float currentIncrease = 1f;
    private float currentIncreaseMem = 0f;

    private Vector3 initPosition;
    private Vector3 initRotation;

    private void Start()
    {
        initPosition = transform.position;
        initRotation = transform.eulerAngles;
    }

    private void OnEnable()
    {
        if (activeCamera)
            wantedMode = CursorLockMode.Locked;
    }

    private void SetCursorState()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            wantedMode = CursorLockMode.None;

        if (mouse.leftButton.wasPressedThisFrame)
            wantedMode = CursorLockMode.Locked;

        Cursor.lockState = wantedMode;
        Cursor.visible = wantedMode != CursorLockMode.Locked;
    }

    private void CalculateCurrentIncrease(bool moving)
    {
        currentIncrease = Time.deltaTime;

        if (!enableSpeedAcceleration || !moving)
        {
            currentIncreaseMem = 0f;
            return;
        }

        currentIncreaseMem += Time.deltaTime * (speedAccelerationFactor - 1f);
        currentIncrease = Time.deltaTime + Mathf.Pow(currentIncreaseMem, 3f) * Time.deltaTime;
    }

    private void Update()
    {
        if (!activeCamera)
            return;

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        SetCursorState();

        if (Cursor.visible)
            return;

        if (enableScrollTranslation && mouse != null)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            transform.Translate(Vector3.forward * scrollY * 0.01f * scrollSpeed, Space.Self);
        }

        if (enableMovement && keyboard != null)
        {
            Vector3 deltaPosition = Vector3.zero;
            float currentSpeed = keyboard[boostKey].isPressed ? boostedSpeed : movementSpeed;

            if (keyboard.wKey.isPressed) deltaPosition += transform.forward;
            if (keyboard.sKey.isPressed) deltaPosition -= transform.forward;
            if (keyboard.aKey.isPressed) deltaPosition -= transform.right;
            if (keyboard.dKey.isPressed) deltaPosition += transform.right;
            if (keyboard[moveUpKey].isPressed) deltaPosition += transform.up;
            if (keyboard[moveDownKey].isPressed) deltaPosition -= transform.up;

            CalculateCurrentIncrease(deltaPosition != Vector3.zero);
            transform.position += deltaPosition * currentSpeed * currentIncrease;
        }

        if (enableRotation && mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();

            transform.rotation *= Quaternion.AngleAxis(-mouseDelta.y * mouseSensitivity, Vector3.right);
            transform.rotation = Quaternion.Euler(
                transform.eulerAngles.x,
                transform.eulerAngles.y + mouseDelta.x * mouseSensitivity,
                0f
            );
        }

        if (keyboard != null && keyboard[resetKey].wasPressedThisFrame)
        {
            transform.position = initPosition;
            transform.eulerAngles = initRotation;
        }
    }
}