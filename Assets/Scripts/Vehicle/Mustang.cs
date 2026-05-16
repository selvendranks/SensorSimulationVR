using UnityEngine;

public class Mustang_WaypointDriver : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float waypointThreshold = 1f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        MoveTowardsWaypoint();
        RotateTowardsWaypoint();
        CheckWaypointReached();
    }

    void MoveTowardsWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );
    }

    void RotateTowardsWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void CheckWaypointReached()
    {
        float distance = Vector3.Distance(
            transform.position,
            waypoints[currentWaypointIndex].position
        );

        if (distance <= waypointThreshold)
        {
            // This line makes it LOOP back to start
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    // Shows the route in Scene view as yellow lines
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}