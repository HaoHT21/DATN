using UnityEngine;
using System.Collections;

public class EnemyCircleShoot : MonoBehaviour
{
    [Header("Vision Settings")]
    public LayerMask wallLayer;         // Layer của tường để kiểm tra tầm nhìn (Line of Sight)

    [Header("Distance Settings")]
    [Tooltip("Khoảng cách tối đa để đứng bắn")]
    public float attackDistance = 4f;

    [Tooltip("Khoảng cách quá gần sẽ kích hoạt lùi")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ di chuyển khi lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Thời gian lùi tối đa")]
    public float retreatDuration = 1.5f;

    [Header("Circle Shoot Settings")]
    [Tooltip("Số lượng đạn bắn ra tạo thành vòng tròn (360 độ)")]
    public int bulletsInCircle = 12;

    [Tooltip("Thời gian hồi giữa mỗi đợt bắn vòng tròn")]
    public float fireRate = 3f;

    [Tooltip("Thời gian chờ của Animation attack trước khi xả đạn")]
    public float attackDuration = 0.3f;

    [Header("Bullet References")]
    public GameObject bulletPrefab;     // Prefab đạn
    public Transform firePoint;         // Vị trí xuất phát của đạn

    // Component References
    private EnemyController controller;
    private Rigidbody2D rb;

    // State Flags & Timers
    private bool isAttacking;
    private bool isRetreating;
    private float retreatTimer;
    private float fireTimer;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Không xử lý nếu đang trong trạng thái bị dính đòn (Stun/Hurt)
        if (controller.IsHurting)
            return;

        // Giảm timer hồi đòn
        fireTimer -= Time.deltaTime;

        // 2. Bỏ qua kiểm tra di chuyển khi đang trong tiến trình bắn
        if (isAttacking)
            return;

        // 3. Mất Target -> Dừng xử lý
        if (!controller.HasTarget)
            return;

        Transform target = controller.Target;
        Vector2 dir = target.position - transform.position;
        float distance = dir.magnitude;

        // Luôn xoay mặt về phía Player
        controller.LookAt(target.position);

        // -----------------------------------------------------------------
        // 1. QUÁ GẦN -> LÙI LẠI (RETREAT)
        // -----------------------------------------------------------------
        if (distance < retreatDistance)
        {
            controller.LockMovement(true);
            Retreat(dir);
            return;
        }

        // -----------------------------------------------------------------
        // 2. TRONG TRẦM ĐÁNH & KHÔNG BỊ CẢN TƯỜNG -> DỪNG VÀ BẮN
        // -----------------------------------------------------------------
        if (distance <= attackDistance && CanShootPlayer(target))
        {
            controller.LockMovement(true);
            controller.StopMovement();
            controller.PlayAnimation("idle");

            Attack();
            return;
        }

        // -----------------------------------------------------------------
        // 3. NGOÀI TẦM ĐÁNH HOẶC BỊ TƯỜNG CẢN -> TRẢ QUYỀN CHO AI CHASE
        // -----------------------------------------------------------------
        controller.LockMovement(false);
        isRetreating = false;
        retreatTimer = 0f;
    }

    // -----------------------------------------------------------------
    // LOGIC LÙI (RETREAT)
    // -----------------------------------------------------------------
    void Retreat(Vector2 dir)
    {
        isRetreating = true;
        retreatTimer += Time.deltaTime;

        Vector2 moveDir = -dir.normalized;
        rb.linearVelocity = moveDir * retreatSpeed;

        controller.PlayAnimation("run");

        // Hết thời gian lùi tối đa -> Dừng lại và phản công
        if (retreatTimer >= retreatDuration)
        {
            retreatTimer = 0f;
            isRetreating = false;
            controller.StopMovement();

            Attack();
        }
    }

    // -----------------------------------------------------------------
    // QUẢN LÝ TIẾN TRÌNH TẤN CÔNG (COROUTINE)
    // -----------------------------------------------------------------
    void Attack()
    {
        if (isAttacking)
            return;

        if (fireTimer <= 0)
        {
            fireTimer = fireRate;
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        controller.LockMovement(true);
        controller.StopMovement();

        controller.PlayAnimation("attack");

        // Đợi theo thời gian ra đòn của Animation
        yield return new WaitForSeconds(attackDuration);

        // Bắn đạn tỏa tròn 360 độ
        CircleShoot();

        controller.PlayAnimation("idle");
        controller.LockMovement(false);
        isAttacking = false;
    }

    // -----------------------------------------------------------------
    // LOGIC BẮN ĐẠN TỎA TRÒN (360 DEGREE SHOOTING)
    // -----------------------------------------------------------------
    void CircleShoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        float angleStep = 360f / bulletsInCircle;

        for (int i = 0; i < bulletsInCircle; i++)
        {
            float angle = i * angleStep;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);

            Instantiate(bulletPrefab, firePoint.position, rot);
        }
    }

    // -----------------------------------------------------------------
    // KIỂM TRA TẦM NHÌN TỚI PLAYER (RAYCAST CẢN TƯỜNG)
    // -----------------------------------------------------------------
    bool CanShootPlayer(Transform player)
    {
        if (firePoint == null)
            return false;

        Vector2 origin = firePoint.position;
        Vector2 targetPos = player.position;
        Vector2 dir = (targetPos - origin).normalized;
        float distance = Vector2.Distance(origin, targetPos);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, wallLayer);

        return hit.collider == null; // Trả về true nếu không có wallLayer cản đường
    }

    // -----------------------------------------------------------------
    // VẼ DEBUG TRONG SCENE VIEW
    // -----------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Vẽ đường Line of Sight (Đỏ: nhìn thấy / Xám: bị che)
        if (controller != null && controller.HasTarget && firePoint != null)
        {
            Gizmos.color = CanShootPlayer(controller.Target) ? Color.red : Color.gray;
            Gizmos.DrawLine(firePoint.position, controller.Target.position);
        }

        // Tầm đánh (Màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Tầm lùi (Màu hồng / Magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}