using UnityEngine;
using UnityEngine.InputSystem;
using Dreamteck.Splines;
using System;

public class VehicleXRPauseInput : MonoBehaviour
{
    [SerializeField] private SplineFollower splineFollower;
    [SerializeField] private InputActionReference leftSelectAction;

    // wheels
    [SerializeField] private GameObject bottom_left;
    [SerializeField] private GameObject bottom_right;
    [SerializeField] private GameObject top_left;
    [SerializeField] private GameObject top_right;

    private float cachedSpeed;
    private bool isPaused;


    private void Awake()
    {
        if (splineFollower == null)
            splineFollower = FindFirstObjectByType<SplineFollower>();

        // move all wheel-children to their parent's position
        int bl_len = bottom_left.transform.childCount;
        int br_len = bottom_right.transform.childCount;
        int tl_len = top_left.transform.childCount;
        int tr_len = top_right.transform.childCount;

        int max_len = Math.Max(Math.Max(bl_len, br_len), Math.Max(tl_len, tr_len));
        for (int i = 0; i < max_len; i++)
        {
            if (i < bl_len)
            {
                GameObject bl_child = bottom_left.transform.GetChild(i).gameObject;
                bl_child.transform.position = bottom_left.transform.position;
            }
            if (i < br_len)
            {
                GameObject br_child = bottom_right.transform.GetChild(i).gameObject;
                br_child.transform.position = bottom_right.transform.position;
            }
            if (i < tl_len)
            {
                GameObject tl_child = top_left.transform.GetChild(i).gameObject;
                tl_child.transform.position = top_left.transform.position;
            }
            if (i < tr_len)
            {
                GameObject tr_child = top_right.transform.GetChild(i).gameObject;
                tr_child.transform.position = top_right.transform.position;
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
        if (!isPaused)
        {
            bottom_left.transform.Rotate(200f * Time.deltaTime, 0, 0);
            bottom_right.transform.Rotate(200f * Time.deltaTime, 0, 0);
            top_left.transform.Rotate(200f * Time.deltaTime, 0, 0);
            top_right.transform.Rotate(200f * Time.deltaTime, 0, 0);
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