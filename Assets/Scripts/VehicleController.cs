using UnityEngine;
using UnityEngine.InputSystem;
using Dreamteck.Splines;
using System;

public class VehicleXRPauseInput : MonoBehaviour
{
    [SerializeField] private SplineFollower splineFollower;
    [SerializeField] private InputActionReference leftSelectAction;

    [Header("Wheels")]
    [SerializeField] private GameObject bottom_left;
    [SerializeField] private GameObject bottom_right;
    [SerializeField] private GameObject top_left;
    [SerializeField] private GameObject top_right;

    private GameObject[] wheels;
    private float cachedSpeed;
    private bool isPaused;

    private void Awake()
    {
        if (splineFollower == null)
            splineFollower = FindFirstObjectByType<SplineFollower>();

        wheels = new GameObject[] { bottom_left, bottom_right, top_left, top_right };

        foreach (var wheel in wheels)
        {
            if (wheel == null)
                continue;

            int childCount = wheel.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = wheel.transform.GetChild(i);
                child.position = wheel.transform.position;
            }
        }
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

    private void Update()
    {
        if (isPaused)
            return;

        float spin = 200f * Time.deltaTime;

        foreach (var wheel in wheels)
        {
            if (wheel == null)
                continue;

            wheel.transform.Rotate(spin, 0f, 0f);
        }
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