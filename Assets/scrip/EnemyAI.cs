using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.2f;

    [Header("Cấu hình AI & Tấn công")]
    public float attackRate = 1.36f;
    public int damage = 10;
    public float attackRange = 1.1f;
    public float chaseRange = 5.0f;

    // Trạng thái cục bộ
    public bool IsAttacking { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsFacingLeft { get; private set; }

    private float _attackTimer;
    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private Transform _target;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Tìm người chơi gần nhất
        FindClosestPlayer();

        if (_target == null)
        {
            IsMoving = false;
            UpdateAnimations();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

        // 2. LOGIC ĐUỔI THEO & TẤN CÔNG
        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > stoppingDistance)
            {
                // Di chuyển
                IsAttacking = false;
                IsMoving = true;

                Vector3 direction = (_target.position - transform.position).normalized;
                _rb.MovePosition(transform.position + (direction * moveSpeed * Time.deltaTime));

                // Hướng mặt
                if (direction.x > 0.01f) IsFacingLeft = false;
                else if (direction.x < -0.01f) IsFacingLeft = true;
            }
            else
            {
                // Tấn công
                IsMoving = false;
                if (_attackTimer <= 0)
                {
                    PerformAttack();
                }
            }
        }
        else
        {
            IsMoving = false;
            IsAttacking = false;
        }

        // Đếm ngược thời gian tấn công
        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

        // Reset trạng thái tấn công sau khi animation chạy được một phần
        if (IsAttacking && _attackTimer < (attackRate - 0.5f))
            IsAttacking = false;

        UpdateAnimations();
    }

    void PerformAttack()
    {
        IsAttacking = true;
        _attackTimer = attackRate;

        // Gọi trực tiếp hàm sát thương (không dùng Rpc)
        if (_target.TryGetComponent<PlayerHealth>(out var pHealth))
            pHealth.TakeDamage(damage);
    }

    void UpdateAnimations()
    {
        if (_sprite != null) _sprite.flipX = IsFacingLeft;
        if (_animator != null)
        {
            _animator.SetBool("isWalking", IsMoving);
            _animator.SetBool("Attack", IsAttacking);
        }
    }

    void FindClosestPlayer()
    {
        float minDistance = float.MaxValue;
        Transform closest = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}