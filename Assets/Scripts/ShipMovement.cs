using UnityEngine;

public class MoveAlongX : MonoBehaviour
{
    [SerializeField] private float endX = 40f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stopThreshold = 0.001f;

    public bool HasReachedDestination { get; private set; }

    void Update()
    {
        if (HasReachedDestination)
            return;

        Vector3 targetPosition = new Vector3(endX, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - endX) <= stopThreshold)
        {
            transform.position = targetPosition;
            HasReachedDestination = true;
        }
    }
}