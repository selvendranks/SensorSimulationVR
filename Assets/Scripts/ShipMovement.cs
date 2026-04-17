using UnityEngine;

public class MoveAlongX : MonoBehaviour
{
    [SerializeField] private float endX = 40f;
    [SerializeField] private float speed = 2f;

    void Update()
    {
        Vector3 targetPosition = new Vector3(endX, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}