using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemySeeker : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float nextWaypointDistance = 0.2f;

    [Header("Path Update")]
    public float updateRate = 0.5f;

    private Transform target;
    private Seeker seeker;
    private Rigidbody2D rb;

    private Path path;
    private int currentWaypoint;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            target = player.transform;
            InvokeRepeating(nameof(UpdatePath), 0f, updateRate);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player có Tag: " + playerTag);
        }
    }

    void UpdatePath()
    {
        if (target == null)
            return;

        if (seeker.IsDone())
        {
            seeker.StartPath(
                rb.position,
                target.position,
                OnPathComplete
            );
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction =
            ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        rb.linearVelocity = direction * moveSpeed;

        float distance = Vector2.Distance(
            rb.position,
            path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }
}