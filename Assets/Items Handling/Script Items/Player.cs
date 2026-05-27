using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public int damageAmount = 20;
    public float attackRate = 0.5f;

    // Các biến trạng thái thường
    public bool IsFacingRight { get; private set; } = true;
    public bool IsAttacking { get; private set; }

    private float _attackTimer;
    private Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    private Collider2D[] hitResults = new Collider2D[10];

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Thiết lập Rigidbody
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        // 1. INPUT DI CHUYỂN
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = moveDir * moveSpeed;

        // 2. FLIP (Xoay hướng)
        if (moveX > 0) IsFacingRight = true;
        else if (moveX < 0) IsFacingRight = false;
        sprite.flipX = !IsFacingRight;

        // 3. XỬ LÝ ĐĂNG CẤP TẤN CÔNG (Cooldown)
        if (IsAttacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0) IsAttacking = false;
        }

        // 4. INPUT TẤN CÔNG
        if (Input.GetButtonDown("Fire1") && !IsAttacking)
        {
            IsAttacking = true;
            _attackTimer = attackRate;
            anim.SetTrigger("Attack"); // Kích hoạt animation Attack
            DealDamageToEnemies();
        }

        // 5. ANIMATION DI CHUYỂN
        anim.SetBool("isWalking", moveDir.magnitude > 0.1f);
    }

    void DealDamageToEnemies()
    {
        // Dùng OverlapCircleAll thay vì NonAlloc để hết cảnh báo
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damageAmount);
            }
        }
    }
}