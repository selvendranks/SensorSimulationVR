using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAlongX : MonoBehaviour
{
    [SerializeField] private float endX = 40f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stopThreshold = 0.001f;
    [SerializeField] private Key startKey = Key.Space;

    public bool HasReachedDestination { get; private set; }
    public bool IsMoving => isMoving;
    private bool isMoving = false;

    public void StartMovement()
    {
        if (HasReachedDestination) return;
        isMoving = true;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[startKey].wasPressedThisFrame)
        {
            StartMovement();
        }

        if (!isMoving || HasReachedDestination)
            return;

        Vector3 targetPosition = new Vector3(endX, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - endX) <= stopThreshold)
        {
            transform.position = targetPosition;
            HasReachedDestination = true;
            isMoving = false;
        }
    }
}