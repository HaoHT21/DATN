using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Khoảng cách tối đa để đứng lại bắn")]
    public float attackDistance = 4f;

    [Tooltip("Nếu Player lại quá gần khoảng cách này, Enemy sẽ lùi lại")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ di chuyển khi lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Thời gian lùi tối đa")]
    public float retreatDuration = 1.5f;

    [Header("Attack Settings")]
    public float fireRate = 2f;         // Thời gian hồi đợt bắn tiếp theo
    public float attackDuration = 0.3f; // Thời gian chờ của Animation đòn đánh trước khi xả đạn

    [Header("Burst Settings")]
    public int burstCount = 3;          // Số lần xả đạn trong 1 đợt bắn (Burst)
    public float burstInterval = 0.15f;  // Khoảng thời gian giữa các lần xả đạn trong đợt

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;     // Prefab đạn
    public Transform firePoint;         // Vị trí xuất phát của đạn

    [Range(1, 20)]
    public int bulletsPerShot = 3;      // Số lượng viên đạn trong mỗi lần xả (Shotgun Pattern)

    [Range(0, 180)]
    public float spreadAngle = 30f;     // Góc xòe/tỏa của các viên đạn

    [Header("Vision Settings")]
    public LayerMask wallLayer;         // Layer của tường/vật cản để kiểm tra tầm nhìn (Line of Sight)

    [Header("Visual Settings")]
    public Transform enemyVisual;       // Reference tới phần hình ảnh/Sprite của Enemy

    // Component References
    private EnemyController controller;
    private Rigidbody2D rb;

    // State Flags & Timers
    private bool isAttacking;           // Đang trong tiến trình xả đạn (Coroutine)
    private bool isRetreating;          // Trạng thái lùi (dự phòng)
    private float retreatTimer;         // Bộ đếm thời gian lùi (dự phòng)
    private float fireTimer;            // Bộ đếm thời gian hồi đòn bắn

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Không làm gì nếu đang trong trạng thái bị trúng đòn (Stun/Hurt)
        if (controller.IsHurting)
            return;

        // Giảm thời gian hồi đòn bắn
        fireTimer -= Time.deltaTime;

        // 2. Nếu đang thực hiện Coroutine bắn -> Bỏ qua kiểm tra di chuyển để bắn trọn vẹn Burst
        if (isAttacking)
            return;

        // 3. Mất Target -> Không xử lý logic nhắm/bắn
        if (!controller.HasTarget)
            return;

        Transform target = controller.Target;
        Vector2 dir = target.position - transform.position;
        float distance = dir.magnitude;

        // Luôn xoay mặt về phía Target
        controller.LookAt(target.position);

        // -----------------------------------------------------------------
        // TRƯỜNG HỢP 1: PLAYER QUÁ GẦN -> LÙI LẠI (RETREAT)
        // -----------------------------------------------------------------
        if (distance < retreatDistance)
        {
            controller.LockMovement(true); // Khóa AI mặc định của Controller
            rb.linearVelocity = -dir.normalized * retreatSpeed; // Lùi ngược hướng Player
            controller.PlayAnimation("run");
            return;
        }

        // -----------------------------------------------------------------
        // TRƯỜNG HỢP 2: TRONG TẦM BẮN VÀ KHÔNG BỊ TƯỜNG CHE -> ĐỨNG BẮN
        // -----------------------------------------------------------------
        if (distance <= attackDistance && CanShootPlayer(target))
        {
            controller.LockMovement(true);
            controller.StopMovement(); // Dừng di chuyển hoàn toàn để đứng bắn
            controller.PlayAnimation("idle");

            Attack();
            return;
        }

        // -----------------------------------------------------------------
        // TRƯỜNG HỢP 3: NGOÀI TẦM BẮN HOẶC BỊ TƯỜNG CHE -> MỞ KHÓA CHO AI CHASE
        // -----------------------------------------------------------------
        controller.LockMovement(false);
    }

    // -----------------------------------------------------------------
    // QUẢN LÝ TIẾN TRÌNH TẤN CÔNG
    // -----------------------------------------------------------------
    void Attack()
    {
        if (isAttacking)
            return;

        // Đủ thời gian hồi đạn -> Bắt đầu Coroutine bắn
        if (fireTimer <= 0)
        {
            fireTimer = fireRate;
            StartCoroutine(AttackRoutine());
        }
    }

    // Coroutine xử lý bắn theo đợt (Burst Fire)
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        controller.LockMovement(true);
        controller.StopMovement();

        for (int i = 0; i < burstCount; i++)
        {
            controller.PlayAnimation("attack");

            // Chờ hết khoảng Windup của Animation đòn đánh
            yield return new WaitForSeconds(attackDuration);

            // Bắn đạn
            Shoot();

            // Nếu chưa phải viên cuối của đợt burst -> Chờ khoảng nghỉ giữa các viên
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // Hoàn thành đợt bắn -> Trả về trạng thái Idle và mở khóa di chuyển
        controller.PlayAnimation("idle");
        controller.LockMovement(false);
        isAttacking = false;
    }

    // -----------------------------------------------------------------
    // LOGIC BẮN ĐẠN & TÍNH GÓC XÒE (SPREAD ANGLE)
    // -----------------------------------------------------------------
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        // Tính góc bắt đầu (Lệch về bên trái một nửa tổng góc tỏa)
        float startAngle = -spreadAngle * 0.5f;

        // Bước góc giữa mỗi viên đạn
        float step = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = startAngle + step * i;

            // Tính góc xoay của viên đạn dựa trên xoay của FirePoint + góc lệch
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            // Sinh ra viên đạn tại FirePoint với góc xoay tương ứng
            Instantiate(bulletPrefab, firePoint.position, rot);
        }
    }

    // -----------------------------------------------------------------
    // KIỂM TRA TẦM NHÌN (LINE OF SIGHT) BẰNG RAYCAST
    // -----------------------------------------------------------------
    bool CanShootPlayer(Transform player)
    {
        Vector2 origin = firePoint.position;
        Vector2 targetPos = player.position;
        Vector2 dir = (targetPos - origin).normalized;
        float distance = Vector2.Distance(origin, targetPos);

        // Bắn Raycast từ FirePoint tới Player, chỉ kiểm tra va chạm với wallLayer
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, wallLayer);

        // Nếu không trúng wallLayer (hit.collider == null) -> Không bị cản tường -> Được bắn
        return hit.collider == null;
    }

    // -----------------------------------------------------------------
    // VẼ BÁN KÍNH TẦM BẮN & TẦM LÙI TRONG SCENE VIEW
    // -----------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Tầm bắn (Màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Tầm lùi (Màu hồng / Magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}