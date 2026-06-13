using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCircleShoot : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Ranges")]
    public float chaseDistance = 8f;
    public float attackDistance = 4f;
    public float retreatDistance = 2f;

    [Header("Circle Shot")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Tooltip("S? ð?n b?n ra xung quanh")]
    public int bulletsInCircle = 12;

    [Tooltip("10 giây b?n 1 l?n")]
    public float fireRate = 10f;

    [Header("Attack")]
    public float attackDuration = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    private Transform target;

    private bool isAttacking;
    private bool isDead;

    private float fireTimer;

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
            Vector2.Distance(transform.position,
                             target.position);

        Vector2 direction =
            (target.position - transform.position)
            .normalized;

        sprite.flipX = direction.x < 0;

        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        // Ngoài t?m phát hi?n
        if (distance > chaseDistance)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
            return;
        }

        // Quá g?n => lùi
        if (distance < retreatDistance)
        {
            rb.linearVelocity =
                -direction * moveSpeed;

            PlayAnimation("idle");
            return;
        }

        // Ðu?i theo
        if (distance > attackDistance)
        {
            rb.linearVelocity =
                direction * moveSpeed;

            PlayAnimation("idle");
            return;
        }

        // Trong t?m b?n
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

        CircleShoot();

        PlayAnimation("idle");

        isAttacking = false;
    }

    private void CircleShoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        float angleStep =
            360f / bulletsInCircle;

        for (int i = 0; i < bulletsInCircle; i++)
        {
            float angle =
                i * angleStep;

            Quaternion rotation =
                Quaternion.Euler(0, 0, angle);

            Instantiate(
                bulletPrefab,
                firePoint.position,
                rotation
            );
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player.transform;
            }
        }

        target = closest;
    }

    private void PlayAnimation(string animName)
    {
        if (anim == null)
            return;

        anim.Play(animName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance);
    }
}