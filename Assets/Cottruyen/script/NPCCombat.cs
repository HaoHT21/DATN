using UnityEngine;
using System.Collections;

public class NPCCombat : MonoBehaviour
{
    private enum NPCPhase { LightPhase, DarkPhase, SupremePhase }

    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 3f;
    public float attackRange = 5f;
    public float attackCooldown = 3f;

    [Header("Hiệu ứng kỹ năng (VFX Prefabs)")]
    public GameObject skillEffect1; // Kỹ năng 1: Bắn thẳng cơ bản
    public GameObject skillEffect2; // Kỹ năng 2: Bắn thẳng nâng cao
    public GameObject spreadSkillEffect; // Kỹ năng 3: Bắn tỏa (Ulti)

    [Header("Cấu hình đạn tỏa (Spread Shot)")]
    public int spreadProjectilesCount = 5;
    public float spreadAngleTotal = 60f;

    [Header("Vị trí bắn (Nguồn phát hiệu ứng)")]
    public Transform firePoint;

    [Header("Cấu hình Sát thương cho từng chiêu")]
    public int damageSkill1 = 10;   // Sát thương Chiêu 1
    public int damageSkill2 = 15;   // Sát thương Chiêu 2
    public int damageSkill3 = 25;   // Sát thương Chiêu 3 (Ulti)

    private Transform playerTransform;
    private float nextAttackTime = 0f;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isAttacking = false;
    private int currentSkillIndex = 0;

    private NPCPhase currentPhase = NPCPhase.LightPhase;

    // Các biến lưu trữ tên đầy đủ của toàn bộ trạng thái hoạt ảnh cho từng Phase
    private string currentIdleState;
    private string currentWalkState;
    private string currentAttack1State;
    private string currentAttack2State;
    private string currentUltiState;

    void OnEnable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (firePoint == null) firePoint = this.transform;
        isAttacking = false;

        // THIẾT LẬP ĐẦY ĐỦ TÊN GỌI CHO CẢ 3 TRẠNG THÁI (PHASE) KHỚP VỚI ANIMATOR
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            string controllerName = animator.runtimeAnimatorController.name;

            if (controllerName.Contains("SP_BSS") || controllerName.Contains("Supreme"))
            {
                currentPhase = NPCPhase.SupremePhase;

                // --- TRẠNG THÁI DẠNG 3: SUPREME PHASE ---
                currentIdleState = "SP_Idel Animation";      // Khớp ảnh SP_BSS
                currentWalkState = "SP_Walk Animation";      // Khớp ảnh SP_BSS
                currentAttack1State = "SP_Attack Animation";    // Khớp ảnh SP_BSS
                currentAttack2State = "SP_Attack2 Animation";   // Khớp ảnh SP_BSS
                currentUltiState = "SP_Ulti Animation";      // Khớp ảnh SP_BSS
                Debug.Log($"<color=cyan>⚡ [{gameObject.name}] Đã cấu hình chuỗi Animation: SUPREME PHASE</color>");
            }
            else if (controllerName.Contains("Dark") || controllerName.Contains("D_BSS"))
            {
                currentPhase = NPCPhase.DarkPhase;

                // --- TRẠNG THÁI DẠNG 2: DARK PHASE ---
                currentIdleState = "DarkIdel Animation";     // Khớp ảnh Dark Phase
                currentWalkState = "DarkWalk Animation";     // Khớp ảnh Dark Phase
                currentAttack1State = "DarkAttack1 Animation";  // Khớp ảnh Dark Phase
                currentAttack2State = "DarkAttack2 Animation";  // Khớp ảnh Dark Phase
                currentUltiState = "DarkUlti Animation";     // Khớp ảnh Dark Phase
                Debug.Log($"<color=purple>🔮 [{gameObject.name}] Đã cấu hình chuỗi Animation: DARK PHASE</color>");
            }
            else
            {
                currentPhase = NPCPhase.LightPhase;

                // --- TRẠNG THÁI DẠNG 1: LIGHT PHASE ---
                currentIdleState = "Idel Animation";         // Khớp ảnh Light Phase
                currentWalkState = "Walk Animation";         // Khớp ảnh Light Phase
                currentAttack1State = "Attack Animation";       // Đánh thường 1 (Mặc định)
                currentAttack2State = "Attack Animation";       // Đánh thường 2 (Dạng 1 dùng chung Attack)
                currentUltiState = "Ulti Animation";         // Khớp ảnh Light Phase
                Debug.Log($"<color=yellow>☀️ [{gameObject.name}] Đã cấu hình chuỗi Animation: LIGHT PHASE</color>");
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        FlipTowardsPlayer();

        if (isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttackRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (animator != null)
        {
            animator.Play(currentWalkState);
        }
    }

    void StopMoving()
    {
        if (animator != null)
        {
            animator.Play(currentIdleState);
        }
    }

    void FlipTowardsPlayer()
    {
        if (spriteRenderer == null || playerTransform == null) return;

        if (playerTransform.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    IEnumerator PerformAttackRoutine()
    {
        isAttacking = true;
        string targetAnimationName = "";

        // CHỌN ĐÚNG KHỚP TÊN ANIMATION THEO CHỈ SỐ SKILL ĐANG LUÂN PHIÊN
        if (currentSkillIndex == 0)
        {
            targetAnimationName = currentAttack1State;
        }
        else if (currentSkillIndex == 1)
        {
            targetAnimationName = currentAttack2State;
        }
        else if (currentSkillIndex == 2)
        {
            targetAnimationName = currentUltiState;
        }

        // Thực hiện ép Animator phát ngay lập tức trạng thái hoạt ảnh đòn đánh
        if (animator != null)
        {
            animator.Play(targetAnimationName);
        }

        // Chờ đúng 1 Frame vật lý để Animator đồng bộ cập nhật sang clip mới
        yield return new WaitForEndOfFrame();

        // TỰ ĐỘNG ĐO ĐỘ DÀI GIÂY THỰC TẾ CỦA HOẠT ẢNH ĐANG PHÁT
        float attackDuration = 0.5f;
        if (animator != null)
        {
            attackDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        }

        // Tạo prefabs đạn tương ứng ra thế giới game
        ExecuteSkill(currentSkillIndex);

        // Luân phiên chuyển kỹ năng cho lượt sau (0 -> 1 -> 2 -> quay về 0)
        currentSkillIndex = (currentSkillIndex + 1) % 3;

        // Chờ quái chạy diễn xuất hết toàn bộ thời gian của đòn đánh
        yield return new WaitForSeconds(attackDuration);

        // Đưa quái về trạng thái đứng im cơ bản sau khi kết thúc chuỗi ra chiêu
        if (animator != null)
        {
            animator.Play(currentIdleState);
        }

        isAttacking = false;
    }

    void ExecuteSkill(int skillIndex)
    {
        if (playerTransform == null) return;

        Vector3 shootDirection = (playerTransform.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

        switch (skillIndex)
        {
            case 0:
                // Bắn chiêu 1 kèm sát thương chiêu 1
                SpawnProjectile(skillEffect1, firePoint.position, baseAngle, damageSkill1);
                break;
            case 1:
                // Bắn chiêu 2 kèm sát thương chiêu 2
                SpawnProjectile(skillEffect2, firePoint.position, baseAngle, damageSkill2);
                break;
            case 2:
                if (spreadSkillEffect != null)
                {
                    float startAngle = baseAngle - (spreadAngleTotal / 2f);
                    float angleStep = spreadAngleTotal / (spreadProjectilesCount - 1);

                    for (int i = 0; i < spreadProjectilesCount; i++)
                    {
                        float currentAngle = startAngle + (angleStep * i);
                        // Bắn chiêu 3 (Ulti tỏa) kèm sát thương chiêu 3 cho toàn bộ đạn tỏa
                        SpawnProjectile(spreadSkillEffect, firePoint.position, currentAngle, damageSkill3);
                    }
                }
                break;
        }
    }

    // ĐÃ CẬP NHẬT: Thêm tham số 'damageToApply' để truyền sát thương động sang từng loại đạn
    void SpawnProjectile(GameObject prefab, Vector3 position, float angle, int damageToApply)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.Euler(0, 0, angle));

        // KIỂM TRA VÀ TRUYỀN SÁT THƯƠNG ĐỘNG CHO SCRIPT NPCBULLET GẮN TRÊN ĐẠN
        if (effect.TryGetComponent<NPCBullet>(out var npcBullet))
        {
            npcBullet.damage = damageToApply;
        }

        Rigidbody2D rb = effect.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float projectileSpeed = 8f;
            Vector2 forceDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rb.linearVelocity = forceDirection * projectileSpeed;
        }
        Destroy(effect, 3f);
    }
}