using UnityEngine;

public class MoveAlongX : MonoBehaviour
{
    [SerializeField] private float startX = 5f;
    [SerializeField] private float endX = 40f;
    [SerializeField] private float speed = 2f;

    private int direction = 1;

    void Update()
    {
        // Move along X axis
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);

        // Check bounds and flip direction
        if (transform.position.x >= endX)
            direction = -1;

        if (transform.position.x <= startX)
            direction = 1;
    }
}
