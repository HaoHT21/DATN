using UnityEngine;
using System.Collections;

public class EnemyShotgun : MonoBehaviour
{
    public enum ShotMode
    {
        Shotgun,
        RandomRain
    }

    [Header("Mode Settings")]
    public ShotMode shotMode;           // Chế độ bắn: Shotgun (xòe quạt) hoặc RandomRain (mưa đạn ngẫu nhiên)

    [Header("Vision Settings")]
    public LayerMask wallLayer;         // Layer của tường để kiểm tra tầm nhìn (Line of Sight)

    [Header("Distance Settings")]
    [Tooltip("Khoảng cách tối đa để đứng bắn")]
    public float attackDistance = 5f;

    [Tooltip("Khoảng cách quá gần sẽ kích hoạt lùi")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ di chuyển khi lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Thời gian lùi tối đa")]
    public float retreatDuration = 1.5f;

    [Header("Attack Settings")]
    [Tooltip("Thời gian hồi giữa mỗi đợt tấn công")]
    public float fireRate = 3f;

    [Tooltip("Thời gian chờ của Animation attack trước khi xả đạn")]
    public float attackDuration = 0.3f;

    [Header("Burst Settings")]
    [Tooltip("Số lần bắn liên tiếp trong một đợt")]
    public int burstCount = 3;

    [Tooltip("Khoảng nghỉ giữa từng phát bắn trong đợt")]
    public float burstInterval = 0.2f;

    [Header("Shotgun Mode Settings")]
    [Tooltip("Số lượng đạn bắn ra cùng lúc trong 1 phát")]
    public int bulletsPerShot = 5;

    [Tooltip("Góc rộng xòe quạt (hình chữ V)")]
    public float spreadAngle = 45f;

    [Header("Random Rain Mode Settings")]
    [Tooltip("Tổng số viên đạn bắn ra liên tục")]
    public int randomBulletCount = 20;

    [Tooltip("Khoảng nghỉ cực ngắn giữa từng viên đạn")]
    public float bulletInterval = 0.05f;

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
        // KIỂM TRA ĐIỀU KIỆN TẤN CÔNG & GIỮ KHOẢNG CÁCH
        // -----------------------------------------------------------------
        if (distance <= attackDistance && CanShootPlayer(target))
        {
            controller.LockMovement(true);
            controller.StopMovement();
            controller.PlayAnimation("idle");

            Attack();
            return;
        }

        controller.LockMovement(false);

        // Kiểm tra khoảng cách đứng bắn dự phòng
        if (distance <= attackDistance)
        {
            controller.LockMovement(true);
            controller.StopMovement();
            controller.PlayAnimation("idle");

            Attack();
            return;
        }

        // Ngoài tầm đánh -> Trả quyền di chuyển lại cho AI Chase
        controller.LockMovement(false);
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
            retreatTimer = 0;
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

        for (int i = 0; i < burstCount; i++)
        {
            controller.PlayAnimation("attack");

            yield return new WaitForSeconds(attackDuration);

            // -------------------------------------------------------------
            // PHÂN NHÁNH XỬ LÝ THEO SHOT MODE
            // -------------------------------------------------------------
            switch (shotMode)
            {
                case ShotMode.Shotgun:
                    ShootShotgun();
                    break;

                case ShotMode.RandomRain:
                    // Chờ Coroutine xả đạn mưa kết thúc trước khi sang lượt tiếp theo
                    yield return StartCoroutine(ShootRandomV());
                    break;
            }

            // Khoảng nghỉ giữa các đợt burst
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        controller.PlayAnimation("idle");
        controller.LockMovement(false);
        isAttacking = false;
    }

    // -----------------------------------------------------------------
    // CHẾ ĐỘ 1: BẮN MƯA ĐẠN NGẪU NHIÊN TRONG KHUNG HÌNH CHỮ V (RANDOM RAIN)
    // -----------------------------------------------------------------
    IEnumerator ShootRandomV()
    {
        if (bulletPrefab == null || firePoint == null)
            yield break;

        for (int i = 0; i < randomBulletCount; i++)
        {
            // Random góc ngẫu nhiên trong khoảng [-spreadAngle/2, spreadAngle/2]
            float angle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);

            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            Instantiate(bulletPrefab, firePoint.position, rot);

            yield return new WaitForSeconds(bulletInterval);
        }
    }

    // -----------------------------------------------------------------
    // CHẾ ĐỘ 2: BẮN SHOTGUN XÒE QUẠT ĐỀU NHAU (SHOTGUN)
    // -----------------------------------------------------------------
    void ShootShotgun()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        float startAngle = -spreadAngle * 0.5f;
        float step = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = startAngle + step * i;

            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

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