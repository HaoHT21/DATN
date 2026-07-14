using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCFollowPlayer : MonoBehaviour
{
    [Header("Follow")]
    public float moveSpeed = 3f;

    [Header("Distance")]
    public float stopDistance = 1.5f;
    public float resumeDistance = 2f;

    [Header("Teleport")]
    public float teleportDistance = 15f;
    public float teleportRadius = 2f;
    public LayerMask wallLayer;

    private NPCAnimation npcAnimation;

    private Rigidbody2D rb;
    private NPCController controller;

    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<NPCController>();
        npcAnimation = GetComponent<NPCAnimation>();
    }

    private void FixedUpdate()
    {
        if (!controller.IsFollow())
            return;

        if (controller.player == null)
            return;

        float playerDistance = Vector2.Distance(
        transform.position,
        controller.player.position);

        if (playerDistance >= teleportDistance)
        {
            TryTeleportNearPlayer();
            return;
        }

        float distance = Vector2.Distance(
        transform.position,
        controller.player.position);

        if (isMoving)
        {
            if (distance <= stopDistance)
                isMoving = false;
        }
        else
        {
            if (distance >= resumeDistance)
                isMoving = true;
        }

        if (!isMoving)
        {
            npcAnimation.PlayIdle();
            return;
        }

        Vector2 direction =
            ((Vector2)controller.player.position - rb.position).normalized;

        controller.RotateCharacter(direction.x);

        npcAnimation.PlayRun();

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void TryTeleportNearPlayer()
    {
        Vector2 center = controller.player.position;

        for (int i = 0; i < 20; i++)
        {
            Vector2 offset = Random.insideUnitCircle * teleportRadius;
            Vector2 candidate = center + offset;

            // Không được nằm trong tường
            if (Physics2D.OverlapCircle(candidate, 0.3f, wallLayer))
                continue;

            // Không có tường chắn giữa Player và vị trí dịch chuyển
            if (Physics2D.Linecast(center, candidate, wallLayer))
                continue;

            rb.position = candidate;

            npcAnimation.PlayIdle();

            return;
        }

        Debug.LogWarning(name + " không tìm được vị trí teleport hợp lệ.");
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, resumeDistance);

        if (controller != null && controller.player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                controller.player.position,
                teleportRadius);
        }
    }
#endif
}
