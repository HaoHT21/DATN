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

    private NPCAnimation npcAnimation;

    private Rigidbody2D rb;

    private Vector2 targetPoint;

    private bool hasTarget;

    private float timer;

    private NPCController controller;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<NPCController>();
        npcAnimation = GetComponent<NPCAnimation>();
    }

    void Start()
    {
        StartCoroutine(RandomMoveLoop());
    }

    IEnumerator RandomMoveLoop()
    {
        while (true)
        {
            if (!controller.IsPatrol())
            {
                hasTarget = false;
                yield return null;
                continue;
            }

            yield return GenerateNewPoint();

            timer = 0;

            npcAnimation.PlayRun();

            while (hasTarget)
            {
                if (!controller.IsPatrol())
                {
                    hasTarget = false;

                    rb.linearVelocity = Vector2.zero;

                    npcAnimation.PlayIdle();

                    break;
                }

                timer += Time.deltaTime;

                Vector2 direction =
                (targetPoint - (Vector2)transform.position).normalized;

                // Xoay theo hướng di chuyển
                controller.RotateCharacter(direction.x);

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

            npcAnimation.PlayIdle();

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