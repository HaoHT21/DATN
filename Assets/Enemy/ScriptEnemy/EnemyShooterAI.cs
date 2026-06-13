using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShooterAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Ranges")]
    public float chaseDistance = 8f;      // Bắt đầu đuổi
    public float attackDistance = 4f;     // Đứng bắn
    public float retreatDistance = 2f;    // Quá gần thì lùi

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float fireRate = 10f;          // 10 giây bắn 1 lần
    public int bulletsPerShot = 3;        // số đạn mỗi lần bắn

    [Header("Attack")]
    public float attackDuration = 1.2f; // thời gian animation attack

    private bool isAttacking;

    private float fireTimer;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    private Transform target;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FindClosestPlayer();

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
            return;
        }

        float distance =
            Vector2.Distance(transform.position, target.position);

        Vector2 direction =
            (target.position - transform.position).normalized;

        sprite.flipX = direction.x < 0;

        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        // Ngoài vùng phát hiện
        if (distance > chaseDistance)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
            return;
        }

        // Quá gần -> chạy lùi
        if (distance < retreatDistance)
        {
            rb.linearVelocity = -direction * moveSpeed;

            sprite.flipX = direction.x > 0;

            PlayAnimation("idle");
            return;
        }

        // Đuổi theo
        if (distance > attackDistance)
        {
            rb.linearVelocity = direction * moveSpeed;
            PlayAnimation("idle");
            return;
        }

        // Trong tầm bắn
        rb.linearVelocity = Vector2.zero;

        if (fireTimer <= 0)
        {
            fireTimer = fireRate;
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        PlayAnimation("attack");

        yield return new WaitForSeconds(attackDuration);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation);

            yield return new WaitForSeconds(0.3f);
        }

        PlayAnimation("idle");

        isAttacking = false;
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation);
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist =
                Vector2.Distance(
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

    private void PlayAnimation(string animName)
    {
        if (anim == null) return;

        anim.Play(animName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}