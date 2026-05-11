using UnityEngine;
using UnityEngine.InputSystem;
using Dreamteck.Splines;

public class VehicleXRPauseInput : MonoBehaviour
{
    [SerializeField] private SplineFollower splineFollower;
    [SerializeField] private InputActionReference leftSelectAction;

    private float cachedSpeed;
    private bool isPaused;

    private void Awake()
    {
        if (splineFollower == null)
            splineFollower = FindFirstObjectByType<SplineFollower>();
    }

    private void Start()
    {
        if (splineFollower == null)
        {
            Debug.LogError("[VehicleXRPauseInput] SplineFollower not found.", this);
            enabled = false;
            return;
        }

        cachedSpeed = splineFollower.followSpeed;
    }

    private void OnEnable()
    {
        if (leftSelectAction != null && leftSelectAction.action != null)
        {
            leftSelectAction.action.performed += OnLeftSelectPerformed;
            leftSelectAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (leftSelectAction != null && leftSelectAction.action != null)
        {
            leftSelectAction.action.performed -= OnLeftSelectPerformed;
            leftSelectAction.action.Disable();
        }
    }

    private void OnLeftSelectPerformed(InputAction.CallbackContext context)
    {
        ToggleMovement();
    }

    public void ToggleMovement()
    {
        if (splineFollower == null)
            return;

        if (isPaused)
        {
            splineFollower.followSpeed = cachedSpeed;
            isPaused = false;
            Debug.Log("[VehicleXRPauseInput] Vehicle resumed.", this);
        }
        else
        {
            cachedSpeed = splineFollower.followSpeed;
            splineFollower.followSpeed = 0f;
            isPaused = true;
            Debug.Log("[VehicleXRPauseInput] Vehicle paused.", this);
        }
    }
}