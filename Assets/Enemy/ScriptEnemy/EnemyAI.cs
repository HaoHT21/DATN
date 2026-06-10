using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public float attackRate = 1.36f;
    public int damage = 10;

    private float attackTimer;
    private bool isDead;
    private bool isFacingLeft;

    private Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Transform target;

    private bool playerInAttackRange;

    private string currentAnim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead) return;

        FindClosestPlayer();

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;

        if (direction.x != 0)
            isFacingLeft = direction.x < 0;

        sprite.flipX = isFacingLeft;

        if (!playerInAttackRange)
        {
            rb.linearVelocity = direction * moveSpeed;
            PlayAnimation("idle");
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (attackTimer <= 0)
            {
                Attack();
            }
        }

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInAttackRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInAttackRange = false;
        }
    }

    private void Attack()
    {
        attackTimer = attackRate;

        PlayAnimation("attack");

        if (target != null &&
            target.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.TakeDamage(damage);
        }

        Invoke(nameof(ReturnToIdle), 0.4f);
    }

    private void ReturnToIdle()
    {
        if (!isDead)
            PlayAnimation("idle");
    }

    public void PlayHurt()
    {
        Debug.Log("HURT CALLED");

        if (isDead) return;

        PlayAnimation("hurt");

        CancelInvoke(nameof(ReturnToIdle));
        Invoke(nameof(ReturnToIdle), 0.3f);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        PlayAnimation("death");

        Destroy(gameObject, 1f);
    }

    private void PlayAnimation(string animName)
    {
        if (currentAnim == animName) return;

        currentAnim = animName;
        anim.Play(animName);
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(
                transform.position,
                p.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }

        target = closest;
    }
}