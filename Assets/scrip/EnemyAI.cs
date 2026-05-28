using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.2f;

    [Header("Cấu hình AI & Tấn công")]
    public float attackRate = 1.36f;
    public int damage = 10;
    public float chaseRange = 5.0f;

    [Header("Drop Settings")]
    public GameObject coinPrefab;

    private float _attackTimer;
    private bool _isAttacking;
    private bool _isMoving;
    private bool _isFacingLeft;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private Transform _target;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        FindClosestPlayer();

        if (_target == null)
        {
            _rb.linearVelocity = Vector2.zero;
            _isMoving = false;
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

            if (distanceToPlayer <= chaseRange)
            {
                if (distanceToPlayer > stoppingDistance)
                {
                    // DI CHUYỂN VẬT LÝ
                    _isAttacking = false;
                    _isMoving = true;
                    Vector2 direction = (_target.position - transform.position).normalized;
                    _rb.linearVelocity = direction * moveSpeed;

                    _isFacingLeft = direction.x < 0;
                }
                else
                {
                    // DỪNG LẠI TẤN CÔNG
                    _rb.linearVelocity = Vector2.zero;
                    _isMoving = false;
                    if (_attackTimer <= 0) PerformAttack();
                }
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                _isMoving = false;
                _isAttacking = false;
            }
        }

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;
        if (_isAttacking && _attackTimer < (attackRate - 0.5f)) _isAttacking = false;

        UpdateAnimations();
    }

    void PerformAttack()
    {
        _isAttacking = true;
        _attackTimer = attackRate;
        if (_target.TryGetComponent<PlayerHealth>(out var pHealth))
            pHealth.TakeDamage(damage);
    }

    // HÀM NÀY GỌI KHI MÁU <= 0
    public void Die()
    {
        if (coinPrefab != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }

    void UpdateAnimations()
    {
        if (_sprite != null) _sprite.flipX = _isFacingLeft;
        if (_animator != null)
        {
            _animator.SetBool("isWalking", _isMoving);
            _animator.SetBool("Attack", _isAttacking);
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = float.MaxValue;
        Transform closest = null;
        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDistance) { minDistance = dist; closest = p.transform; }
        }
        _target = closest;
    }
}