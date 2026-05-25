using Fusion;
using UnityEngine;

public class BossAI : NetworkBehaviour
{
    [Header("Cấu hình")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2.5f;

    // --- BIẾN MẠNG ---
    [Networked] public bool IsDead { get; set; } // Biến quan trọng nhất để báo tử
    [Networked] public bool IsAttacking { get; set; }
    [Networked] public bool IsMoving { get; set; }
    [Networked] public bool IsFacingLeft { get; set; }
    [Networked] private TickTimer attackTimer { get; set; }

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Transform _target;

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // 1. Kiểm tra máu (Nếu bạn có script Health, hãy check ở đây)
        // Ví dụ: if (GetComponent<EnemyHealth>().currentHealth <= 0) IsDead = true;

        // 2. Nếu đã chết thì ngừng mọi logic AI
        if (IsDead)
        {
            IsMoving = false;
            IsAttacking = false;
            return;
        }

        FindClosestPlayer();
        if (_target == null) { IsMoving = false; return; }

        float dist = Vector3.Distance(transform.position, _target.position);

        if (dist <= detectionRange)
        {
            if (dist <= attackRange)
            {
                if (attackTimer.ExpiredOrNotRunning(Runner))
                {
                    IsMoving = false;
                    IsAttacking = true;
                    attackTimer = TickTimer.CreateFromSeconds(Runner, 1.2f); // Thời gian chờ chém

                    if (_target.TryGetComponent<PlayerHealth>(out var hp))
                        hp.Rpc_TakeDamage(30);
                }
            }
            else if (!IsAttacking)
            {
                IsMoving = true;
                Vector3 dir = (_target.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Runner.DeltaTime;
                IsFacingLeft = dir.x < 0;
            }
        }
        else { IsMoving = false; }

        if (IsAttacking && attackTimer.RemainingTime(Runner) < 0.6f) IsAttacking = false;
    }

    void FindClosestPlayer()
    {
        float minDistance = float.MaxValue;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < minDistance) { minDistance = d; _target = p.transform; }
        }
    }

    public override void Render()
    {
        if (_sprite != null) _sprite.flipX = IsFacingLeft;

        if (_animator != null)
        {
            _animator.SetBool("isWalking", IsMoving);
            _animator.SetBool("Attack", IsAttacking);

            // 3. Kích hoạt hiệu ứng chết cho tất cả máy khách
            _animator.SetBool("Death", IsDead);
        }
    }
}