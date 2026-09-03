using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;       // Khoảng cách tối thiểu để kích hoạt đòn đánh
    public float attackWindupTime = 0.3f;  // Thời gian gồng/chuẩn bị trước khi lướt (Windup)
    public float dashSpeed = 10f;          // Tốc độ lướt tới khi tấn công
    public float dashDuration = 0.3f;      // Thời lượng của đòn lướt

    [Header("Recovery Settings")]
    public float recoveryTime = 1f;        // Thời gian nghỉ/hồi sức sau khi kết thúc đòn đánh

    [Header("Hitbox")]
    public GameObject attackHitbox;        // Gameobject vùng gây sát thương (Hitbox)

    // Component References
    private EnemyController controller;
    private Rigidbody2D rb;
    private Animator animator;

    // State Flags & Timers
    private bool isPreparingAttack;        // Đang trong trạng thái chuẩn bị đánh (Windup)
    private bool isAttacking;              // Đang trong trạng thái lướt đánh (Dash)
    private bool isRecovering;             // Đang trong trạng thái hồi phục sau đánh (Recovery)

    private float attackTimer;             // Bộ đếm thời gian cho Windup & Dash
    private float recoveryTimer;           // Bộ đếm thời gian hồi phục

    private Vector2 dashDirection;         // Hướng lướt theo vị trí khóa của Player
    private Vector2 lockedAttackPosition;  // Vị trí mục tiêu được chốt tại thời điểm ra đòn
    private string currentAnim;            // Lưu tên animation hiện tại để tránh gọi lặp

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Ẩn Hitbox ban đầu
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // -----------------------------------------------------------------
        // 1. KIỂM TRA ĐIỀU KIỆN HỦY TẤN CÔNG (MẤT TARGET HOẶC BỊ TRÚNG ĐAN)
        // -----------------------------------------------------------------
        if (!controller.HasTarget)
        {
            if (isPreparingAttack || isAttacking || isRecovering)
            {
                CancelAttack();
            }
            return;
        }

        if (controller.IsHurting)
        {
            return;
        }

        Transform target = controller.Target;
        float distance = Vector2.Distance(transform.position, target.position);

        // -----------------------------------------------------------------
        // 2. XỬ LÝ TRẠNG THÁI NGHỈ / HỒI PHỤC SAU KHI TẤN CÔNG (RECOVERY)
        // -----------------------------------------------------------------
        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;

            controller.StopMovement();
            controller.PlayAnimation("idle");

            // Trường hợp mất Player khi đang đứng nghỉ
            if (!controller.HasTarget)
            {
                isRecovering = false;
                controller.LockMovement(false);
                return;
            }

            // Hết thời gian nghỉ -> Cho phép di chuyển lại bình thường
            if (recoveryTimer <= 0)
            {
                isRecovering = false;
                controller.LockMovement(false);
            }

            return;
        }

        // -----------------------------------------------------------------
        // 3. XỬ LÝ TRẠNG THÁI GỒNG ĐÁNH (WINDUP)
        // -----------------------------------------------------------------
        if (isPreparingAttack)
        {
            attackTimer -= Time.deltaTime;

            controller.StopMovement();
            controller.LookAt(lockedAttackPosition); // Quay mặt về vị trí đã khóa

            // Hết thời gian gồng -> Chuyển sang Lướt đánh (Dash)
            if (attackTimer <= 0)
            {
                isPreparingAttack = false;
                isAttacking = true;
                attackTimer = dashDuration;

                // Tính toán hướng lướt dựa trên vị trí Player đã khóa trước đó
                dashDirection = (lockedAttackPosition - (Vector2)transform.position).normalized;
            }

            return;
        }

        // -----------------------------------------------------------------
        // 4. XỬ LÝ TRẠNG THÁI LƯỚT ĐÁNH (DASH & ACTIVATING HITBOX)
        // -----------------------------------------------------------------
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            // Thực hiện lực lướt vật lý
            rb.linearVelocity = dashDirection * dashSpeed;

            // Bật Hitbox gây sát thương
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(true);
            }

            // Hết thời gian lướt -> Chuyển sang trạng thái Hồi phục (Recovery)
            if (attackTimer <= 0)
            {
                StartRecovery();
            }

            return;
        }

        // -----------------------------------------------------------------
        // 5. KHỞI TẠO TẤN CÔNG KHI PLAYER VÀO TẦM ĐÁNH (ATTACK RANGE)
        // -----------------------------------------------------------------
        if (distance <= attackRange && !isPreparingAttack && !isAttacking && !isRecovering)
        {
            isPreparingAttack = true;
            attackTimer = attackWindupTime;
            lockedAttackPosition = target.position; // Khóa vị trí hiện tại của Player

            controller.LockMovement(true);         // Khóa di chuyển từ EnemyController
            controller.StopMovement();
            controller.LookAt(lockedAttackPosition);
            controller.PlayAnimation("attack");
        }
        else
        {
            // Ngoài tầm đánh và không làm gì -> Mở khóa di chuyển cho AI chase
            controller.LockMovement(false);
        }
    }

    // -----------------------------------------------------------------
    // CÁC HÀM BỔ TRỢ QUẢN LÝ TRẠNG THÁI
    // -----------------------------------------------------------------

    // Hủy toàn bộ tiến trình tấn công (đưa về trạng thái ban đầu)
    void CancelAttack()
    {
        isPreparingAttack = false;
        isAttacking = false;
        isRecovering = false;
        attackTimer = 0;
        recoveryTimer = 0;

        rb.linearVelocity = Vector2.zero;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        controller.LockMovement(false);
        controller.StopMovement();
        controller.PlayAnimation("idle");

        currentAnim = "";
    }

    // Bắt đầu đếm ngược thời gian nghỉ sau khi đòn lướt kết thúc
    void StartRecovery()
    {
        isPreparingAttack = false;
        isAttacking = false;
        attackTimer = 0;

        rb.linearVelocity = Vector2.zero;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        isRecovering = true;
        recoveryTimer = recoveryTime;

        controller.LockMovement(true);
        controller.PlayAnimation("idle");

        currentAnim = "";
    }

    // Hàm gọi Animation tránh trùng lặp state
    void PlayAnimation(string animName)
    {
        if (animator == null) return;
        if (currentAnim == animName) return;

        currentAnim = animName;
        animator.Play(animName);
    }

    // Vẽ bán kính tầm đánh trong Scene View để dễ căn chỉnh Inspector
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}