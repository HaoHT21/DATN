using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public int damageAmount = 20;
    public float attackRate = 0.5f;

    [Networked]
    public bool IsFacingRight { get; set; } = true;

    [Networked]
    public bool IsAttacking { get; set; }

    [Networked]
    private TickTimer attackTimer { get; set; }

    private Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    private Collider2D[] hitResults =
        new Collider2D[10];

    public override void Spawned()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out PlayerInputData data))
            return;

        // ===== MOVE =====

        Vector2 moveDir =
            data.MovementDirection.normalized;

        // Dùng velocity thay vì MovePosition
        rb.linearVelocity =
            moveDir * moveSpeed;

        // ===== FLIP =====

        if (Object.HasStateAuthority)
        {
            if (moveDir.x > 0)
            {
                IsFacingRight = true;
            }
            else if (moveDir.x < 0)
            {
                IsFacingRight = false;
            }
        }

        // ===== ATTACK =====

        if (Object.HasStateAuthority)
        {
            if (IsAttacking &&
                attackTimer.Expired(Runner))
            {
                IsAttacking = false;
            }

            if (data.IsAttackPressed &&
                attackTimer.ExpiredOrNotRunning(Runner))
            {
                IsAttacking = true;

                attackTimer =
                    TickTimer.CreateFromSeconds(
                        Runner,
                        attackRate
                    );

                DealDamageToEnemies();
            }
        }

        // ===== ANIMATION =====

        if (anim != null)
        {
            anim.SetBool(
                "isWalking",
                moveDir.magnitude > 0.1f
            );
        }
    }

    public override void Render()
    {
        if (sprite != null)
        {
            sprite.flipX = !IsFacingRight;
        }

        if (anim != null)
        {
            anim.SetBool(
                "Attack",
                IsAttacking
            );
        }
    }

    void DealDamageToEnemies()
    {
        int hitCount =
            Physics2D.OverlapCircleNonAlloc(
                transform.position,
                attackRange,
                hitResults,
                enemyLayer
            );

        for (int i = 0; i < hitCount; i++)
        {
            if (hitResults[i]
                .TryGetComponent(
                    out EnemyHealth enemy))
            {
                enemy.Rpc_TakeDamage(
                    damageAmount
                );
            }
        }
    }
}