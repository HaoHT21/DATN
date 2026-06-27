using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAtlantisController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float attackCooldown = 3f;
    public int bulletCount = 3;
    public float bulletInterval = 0.3f;

    private float attackTimer;

    private bool isAttacking;
    private bool isDead;

    private Transform target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (bossHeath == null)
            bossHeath =
                GetComponent<BossHeath>();
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
            target =
                player.transform;
    }

    private void Update()
    {
        // ===== DEATH =====

        if (!isDead &&
            bossHeath != null &&
            bossHeath.currentHeath <= 0)
        {
            isDead = true;

            rb.linearVelocity =
                Vector2.zero;

            rb.simulated =
                false;

            PlayDeath();

            return;
        }

        if (isDead)
            return;

        if (target == null)
            return;

        if (isAttacking)
            return;

        // ===== ATTACK TIMER =====

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;

            StartCoroutine(
                AttackSkill()
            );

            return;
        }

        // ===== LOOK PLAYER =====

        Vector3 rot =
            bossVisual.localEulerAngles;

        if (
            target.position.x >
            transform.position.x
        )
        {
            rot.y = 0f;
        }
        else
        {
            rot.y = 180f;
        }

        bossVisual.localEulerAngles =
            rot;

        // ===== MOVE =====

        Vector2 direction =
            (
            target.position -
            transform.position
            ).normalized;

        rb.MovePosition(
            rb.position +
            direction *
            moveSpeed *
            Time.deltaTime
        );

        PlayIdle();
    }

    // ==================
    // ANIMATION
    // ==================

    public void PlayIdle()
    {
        if (isDead)
            return;

        anim.Play("idle");
    }

    public void PlayAttack()
    {
        if (isDead)
            return;

        anim.Play(
            "attack",
            0,
            0f
        );
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    // ==================
    // ATTACK SKILL
    // ==================

    private IEnumerator AttackSkill()
    {
        isAttacking = true;

        rb.linearVelocity =
            Vector2.zero;

        for (int i = 0; i < bulletCount; i++)
        {
            // reset animation
            PlayAttack();

            // đợi boss vung tay
            yield return new WaitForSeconds(
                0.3f
            );

            // bắn
            SpawnBullet();

            // đợi trước viên tiếp theo
            yield return new WaitForSeconds(
                bulletInterval
            );
        }

        PlayIdle();

        isAttacking = false;
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null ||
            firePoint == null ||
            target == null)
            return;

        Vector2 direction =
            (
            target.position -
            firePoint.position
            ).normalized;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(
                0,
                0,
                angle
            )
        );
    }
}