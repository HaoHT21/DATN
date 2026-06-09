using UnityEngine;

public enum RangedEnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Máu & Di chuyển")]
    public float moveSpeed = 2.5f;

    [Header("Phát hiện & Tấn công")]
    public float detectionRange = 8f;
    public float attackRange = 6f;
    [Tooltip("Khoảng cách tối thiểu — enemy sẽ lùi lại nếu Player quá gần")]
    public float minAttackRange = 2.5f;
    public int damage = 12;
    public float attackCooldown = 1.5f;
    public float projectileSpeed = 9f;

    [Header("Đạn")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Tooltip("Bật nếu muốn gọi SpawnProjectile() từ Animation Event thay vì bắn ngay")]
    public bool spawnBulletOnAnimationEvent;

    [Header("Tuần tra (để trống = đứng yên)")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1.5f;
    public float patrolReachDistance = 0.25f;

    [Header("Drop khi chết")]
    public GameObject coinPrefab;
    public int coinDropCount = 5;

    private RangedEnemyState _state = RangedEnemyState.Idle;
    private float _attackTimer;
    private float _patrolWaitTimer;
    private int _patrolIndex;
    private bool _isFacingLeft;
    private bool _lastFacingLeft;
    private bool _facingInitialized;
    private bool _isAttacking;

    private Vector3 _firePointDefaultLocalPos;
    private Vector3 _spawnPosition;
    private Transform _target;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _spawnPosition = transform.position;

        if (firePoint != null)
            _firePointDefaultLocalPos = firePoint.localPosition;
    }

    private void Update()
    {
        FindClosestPlayer();

        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;

        if (_target == null || !IsPlayerInDetectionRange())
        {
            HandlePatrolOrIdle();
        }
        else
        {
            float distance = Vector2.Distance(transform.position, _target.position);
            UpdateCombat(distance);
        }

        if (_isAttacking && _attackTimer < attackCooldown - 0.4f)
            _isAttacking = false;

        UpdateAnimations();
    }

    private void UpdateCombat(float distanceToPlayer)
    {
        Vector2 toPlayer = (_target.position - transform.position).normalized;
        _isFacingLeft = toPlayer.x < 0f;

        if (distanceToPlayer <= attackRange && distanceToPlayer >= minAttackRange)
        {
            _state = RangedEnemyState.Attack;
            _rb.linearVelocity = Vector2.zero;

            if (_attackTimer <= 0f)
                PerformRangedAttack();
        }
        else if (distanceToPlayer > attackRange)
        {
            _state = RangedEnemyState.Chase;
            _isAttacking = false;
            _rb.linearVelocity = toPlayer * moveSpeed;
        }
        else
        {
            // Player quá gần — lùi lại để giữ khoảng cách bắn
            _state = RangedEnemyState.Chase;
            _isAttacking = false;
            _rb.linearVelocity = -toPlayer * moveSpeed;
        }
    }

    private void HandlePatrolOrIdle()
    {
        _isAttacking = false;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            _state = RangedEnemyState.Idle;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        _state = RangedEnemyState.Patrol;

        if (_patrolWaitTimer > 0f)
        {
            _patrolWaitTimer -= Time.deltaTime;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform waypoint = patrolPoints[_patrolIndex];
        if (waypoint == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toWaypoint = waypoint.position - transform.position;
        float distance = toWaypoint.magnitude;

        if (distance <= patrolReachDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            _patrolWaitTimer = patrolWaitTime;
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            return;
        }

        Vector2 direction = toWaypoint / distance;
        _rb.linearVelocity = direction * moveSpeed;
        _isFacingLeft = direction.x < 0f;
    }

    private void PerformRangedAttack()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[{name}] Chưa gán bulletPrefab cho RangedEnemyAI.");
            return;
        }

        _isAttacking = true;
        _attackTimer = attackCooldown;

        if (_animator != null)
            _animator.SetTrigger("Attack");

        if (!spawnBulletOnAnimationEvent)
            SpawnProjectile();
    }

    /// <summary>
    /// Có thể gọi từ Animation Event thay cho SpawnProjectile tự động.
    /// </summary>
    public void SpawnProjectile()
    {
        if (bulletPrefab == null || _target == null)
            return;

        Vector3 spawnPos = GetFirePointWorldPosition();
        Vector2 direction = (_target.position - spawnPos).normalized;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        if (bullet.TryGetComponent(out EnemyProjectile projectile))
            projectile.Initialize(direction, projectileSpeed, damage);
    }

    public void Die()
    {
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinDropCount; i++)
            {
                Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private bool IsPlayerInDetectionRange()
    {
        if (_target == null) return false;
        return Vector2.Distance(transform.position, _target.position) <= detectionRange;
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }

        _target = closest;
    }

    private Vector3 GetFirePointWorldPosition()
    {
        if (firePoint == null)
            return transform.position;

        Vector3 localPos = _firePointDefaultLocalPos;
        if (_isFacingLeft)
            localPos.x = -localPos.x;

        return transform.TransformPoint(localPos);
    }

    private void UpdateFacing()
    {
        if (_sprite != null)
            _sprite.flipX = _isFacingLeft;

        if (firePoint == null)
            return;

        if (!_facingInitialized || _lastFacingLeft != _isFacingLeft)
        {
            Vector3 pos = _firePointDefaultLocalPos;
            if (_isFacingLeft)
                pos.x = -pos.x;

            firePoint.localPosition = pos;
            _lastFacingLeft = _isFacingLeft;
            _facingInitialized = true;
        }
    }

    private void UpdateAnimations()
    {
        UpdateFacing();

        if (_animator == null) return;

        bool isMoving = _rb.linearVelocity.sqrMagnitude > 0.01f;
        _animator.SetBool("isWalking", isMoving);
        _animator.SetBool("Attack", _isAttacking);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, minAttackRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
                Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (next != null)
                    Gizmos.DrawLine(patrolPoints[i].position, next.position);
            }
        }
    }
}
