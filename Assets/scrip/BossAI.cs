using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class BossAI : MonoBehaviour
{
    public float moveSpeed = 3f, detectionRange = 10f, attackRange = 2.5f;
    public GameObject coinPrefab; // Gán Prefab xu vào đây

    private bool _isDead;
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
        if (_isDead) return;
        FindPlayer();
        if (_target == null) return;

        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist <= detectionRange)
        {
            if (dist > attackRange)
            {
                // DI CHUYỂN VẬT LÝ
                Vector2 dir = (_target.position - transform.position).normalized;
                _rb.linearVelocity = dir * moveSpeed;

                _sprite.flipX = dir.x < 0;
                _animator.SetBool("isWalking", true);
            }
            else
            {
                _rb.linearVelocity = Vector2.zero; // Dừng lại để tấn công
                _animator.SetBool("isWalking", false);
                _animator.SetTrigger("Attack");
            }
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool("isWalking", false);
        }
    }

    void FindPlayer() { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p) _target = p.transform; }

    public void SetDead()
    {
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _animator.SetBool("Death", true);

        // Rơi xu khi Boss chết
        if (coinPrefab != null)
        {
            for (int i = 0; i < 15; i++) // Boss rơi nhiều xu hơn enemy
            {
                Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 1f);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }

        Destroy(gameObject, 2f);
    }
}