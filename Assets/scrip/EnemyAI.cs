using Fusion;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.2f;

    [Header("Cấu hình AI & Tấn công")]
    public float attackRate = 1.36f;
    public int damage = 10;
    public float attackRange = 1.1f;
    public float chaseRange = 5.0f;

    // --- BIẾN MẠNG: Đồng bộ trạng thái cho tất cả máy khách ---
    [Networked] public bool IsAttacking { get; set; }
    [Networked] public bool IsMoving { get; set; } // Đồng bộ trạng thái đi bộ
    [Networked] public bool IsFacingLeft { get; set; } // Đồng bộ hướng mặt
    [Networked] private TickTimer attackTimer { get; set; }

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private Transform _target;

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        // 1. Chỉ máy Host/Server mới có quyền tính toán logic di chuyển
        if (!Object.HasStateAuthority) return;

        // Tìm người chơi ở gần nhất
        FindClosestPlayer();

        if (_target == null)
        {
            IsMoving = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

        // 2. LOGIC PHÁT HIỆN & ĐUỔI THEO
        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > stoppingDistance)
            {
                IsAttacking = false;
                IsMoving = true;

                Vector3 direction = (_target.position - transform.position).normalized;

                // Di chuyển bằng MovePosition (Phải để Rigidbody là Kinematic)
                Vector2 nextPos = (Vector2)transform.position + ((Vector2)direction * moveSpeed * Runner.DeltaTime);
                _rb.MovePosition(nextPos);

                // Đồng bộ hướng mặt
                if (direction.x > 0.01f) IsFacingLeft = false;
                else if (direction.x < -0.01f) IsFacingLeft = true;
            }
            // 3. LOGIC TẤN CÔNG
            else
            {
                IsMoving = false;
                if (attackTimer.ExpiredOrNotRunning(Runner))
                {
                    IsAttacking = true;
                    attackTimer = TickTimer.CreateFromSeconds(Runner, attackRate);

                    if (_target.TryGetComponent<PlayerHealth>(out var pHealth))
                        pHealth.Rpc_TakeDamage(damage);
                }
            }
        }
        else
        {
            IsMoving = false;
            IsAttacking = false;
        }

        // Tự động tắt trạng thái chém sau một khoảng thời gian để reset animation
        if (IsAttacking && attackTimer.RemainingTime(Runner) < (attackRate - 0.5f))
            IsAttacking = false;
    }

    // HÀM QUÉT PLAYER: Tìm người ở gần nhất để đuổi
    void FindClosestPlayer()
    {
        float minDistance = float.MaxValue;
        Transform closest = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            // Bỏ qua nếu Player đã chết
            if (p.TryGetComponent<PlayerHealth>(out var hp) && hp.currentHealth <= 0) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        _target = closest;
    }

    // Render chạy trên TẤT CẢ các máy khách để hiển thị hình ảnh
    public override void Render()
    {
        if (_sprite != null) _sprite.flipX = IsFacingLeft;

        if (_animator != null)
        {
            // Sử dụng các biến mạng [Networked] để ép Animator chạy giống nhau
            _animator.SetBool("isWalking", IsMoving);
            _animator.SetBool("Attack", IsAttacking);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}