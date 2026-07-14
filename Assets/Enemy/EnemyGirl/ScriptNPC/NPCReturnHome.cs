using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCReturnHome : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Distance")]
    public float arriveDistance = 0.15f;

    [Header("Teleport")]
    public float teleportDistance = 20f;
    public float teleportRadius = 1f;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private NPCController controller;
    private NPCAnimation npcAnimation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<NPCController>();
        npcAnimation = GetComponent<NPCAnimation>();
    }

    private void FixedUpdate()
    {
        if (!controller.IsReturnHome())
            return;

        if (controller.homePoint == null)
            return;

        float homeDistance = Vector2.Distance(
    transform.position,
    controller.homePoint.position);

        if (homeDistance >= teleportDistance)
        {
            TryTeleportHome();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            controller.homePoint.position);

        if (distance <= arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;

            npcAnimation.PlayIdle();

            controller.ArriveHome();

            return;
        }

        Vector2 direction =
            ((Vector2)controller.homePoint.position - rb.position).normalized;

        controller.RotateCharacter(direction.x);

        npcAnimation.PlayRun();

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void TryTeleportHome()
    {
        Vector2 center = controller.homePoint.position;

        for (int i = 0; i < 20; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * teleportRadius;
            Vector2 candidate = center + randomOffset;

            // Không được nằm trong tường
            if (Physics2D.OverlapCircle(candidate, 0.3f, wallLayer))
                continue;

            transform.position = candidate;

            npcAnimation.PlayIdle();

            controller.ArriveHome();

            return;
        }

        // Nếu không tìm được vị trí thì teleport đúng điểm Home
        transform.position = center;

        npcAnimation.PlayIdle();

        controller.ArriveHome();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (controller != null && controller.homePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, controller.homePoint.position);
        }

        Gizmos.color = Color.magenta;

        if (controller != null && controller.homePoint != null)
        {
            Gizmos.DrawWireSphere(
                controller.homePoint.position,
                teleportRadius);
        }
    }
#endif
}