using Fusion;
using UnityEngine;

public partial class PlayerController : NetworkBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 5f;

    [Header("Cấu hình chiến đấu")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public int damageAmount = 20;
    public float attackRate = 0.5f;

    [Networked] public bool IsFacingRight { get; set; } = true;
    [Networked] public bool IsAttacking { get; set; }
    [Networked] private TickTimer attackTimer { get; set; }

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Collider2D[] _hitResults = new Collider2D[10];

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    public override void FixedUpdateNetwork()
    {
        // Fusion xử lý Input và Prediction (Dự đoán) ở đây
        if (GetInput(out PlayerInputData data))
        {
            // 1. DI CHUYỂN
            Vector3 moveVec = new Vector3(data.MovementDirection.x, data.MovementDirection.y, 0).normalized;

            // Thay vì transform.Translate, dùng cách này để NetworkTransform bắt tọa độ tốt hơn
            transform.position += moveVec * moveSpeed * Runner.DeltaTime;

            // 2. XOAY MẶT (Chỉ thực hiện trên máy có quyền điều khiển hoặc Server)
            if (Object.HasStateAuthority)
            {
                if (data.MovementDirection.x > 0) IsFacingRight = true;
                else if (data.MovementDirection.x < 0) IsFacingRight = false;
            }

            // 3. LOGIC CHIẾN ĐẤU
            if (Object.HasStateAuthority)
            {
                if (IsAttacking && attackTimer.Expired(Runner)) IsAttacking = false;

                if (data.IsAttackPressed && attackTimer.ExpiredOrNotRunning(Runner))
                {
                    IsAttacking = true;
                    attackTimer = TickTimer.CreateFromSeconds(Runner, attackRate);
                    DealDamageToEnemies();
                }
            }

            // Animation đi bộ (Cập nhật cục bộ để mượt, không cần qua mạng)
            if (_animator != null)
            {
                _animator.SetBool("isWalking", data.MovementDirection.magnitude > 0.1f);
            }
        }
    }

    public override void Render()
    {
        // Đồng bộ hướng mặt và chém trên TẤT CẢ các máy khách
        if (_sprite != null) _sprite.flipX = !IsFacingRight;
        if (_animator != null) _animator.SetBool("Attack", IsAttacking);
    }

    void DealDamageToEnemies()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, _hitResults, enemyLayer);
        for (int i = 0; i < hitCount; i++)
        {
            if (_hitResults[i].TryGetComponent<EnemyHealth>(out var eHealth))
            {
                eHealth.Rpc_TakeDamage(damageAmount);
            }
        }
    }
}