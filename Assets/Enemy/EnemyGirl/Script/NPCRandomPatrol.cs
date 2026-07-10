using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCRandomPatrol : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Random Area")]
    public float randomRadius = 6f;

    [Header("Layer")]
    public LayerMask wallLayer;

    [Header("Wait")]
    public float waitTime = 1f;

    [Header("Timeout")]
    public float reachTimeout = 3f;

    [Header("Animator")]
    public Animator animator;

    private Rigidbody2D rb;

    private Vector2 targetPoint;

    private bool hasTarget;

    private float timer;

    public 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(RandomMoveLoop());
    }

    IEnumerator RandomMoveLoop()
    {
        while (true)
        {
            yield return GenerateNewPoint();

            timer = 0;

            animator.Play("run");

            while (hasTarget)
            {
                timer += Time.deltaTime;

                Vector2 direction =
                (targetPoint - (Vector2)transform.position).normalized;

                // Xoay theo hướng di chuyển
                RotateCharacter(direction.x);

                rb.MovePosition(
                    rb.position +
                    direction * moveSpeed * Time.fixedDeltaTime
                );

                rb.MovePosition(
                    rb.position +
                    direction * moveSpeed * Time.fixedDeltaTime
                );

                if (Vector2.Distance(transform.position, targetPoint) < 0.15f)
                {
                    hasTarget = false;
                    break;
                }

                if (timer >= reachTimeout)
                {
                    hasTarget = false;
                    break;
                }

                yield return new WaitForFixedUpdate();
            }

            animator.Play("idle");

            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator GenerateNewPoint()
    {
        hasTarget = false;

        while (!hasTarget)
        {
            Vector2 random =
                (Vector2)transform.position +
                Random.insideUnitCircle * randomRadius;

            // Kiểm tra điểm nằm trong tường
            Collider2D hit =
                Physics2D.OverlapCircle(
                    random,
                    0.25f,
                    wallLayer);

            if (hit != null)
            {
                yield return null;
                continue;
            }

            // Kiểm tra đường đi có bị tường chắn
            RaycastHit2D ray =
                Physics2D.Linecast(
                    transform.position,
                    random,
                    wallLayer);

            if (ray.collider != null)
            {
                yield return null;
                continue;
            }

            targetPoint = random;
            hasTarget = true;
        }
    }

    void RotateCharacter(float moveX)
    {
        if (moveX > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (moveX < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

#if UNITY_EDITOR

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, randomRadius);

        if (hasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPoint, 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint);
        }
    }

#endif
}