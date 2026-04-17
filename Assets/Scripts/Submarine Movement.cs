using UnityEngine;

public class MoveAlongZ : MonoBehaviour
{
    [SerializeField] private float startZ = 5f;
    [SerializeField] private float endZ = 40f;
    [SerializeField] private float speed = 2f;

    private int direction = 1;

    void Update()
    {
        // Move along Z axis
        transform.Translate(Vector3.forward * direction * speed * Time.deltaTime, Space.World);

        // Check bounds and flip direction
        if (transform.position.z >= endZ)
            direction = -1;

        if (transform.position.z <= startZ)
            direction = 1;
    }
}
