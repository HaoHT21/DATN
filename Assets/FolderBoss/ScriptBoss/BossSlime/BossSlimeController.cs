using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossSlimeController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Skill Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float skillCooldown = 10f;
    public int bulletCount = 8;

    [Header("Summon Enemy")]
    public GameObject enemyPrefab;
    public int summonCount = 5;
    public float summonRadius = 3f;
    public float summonCooldown = 10f;

    private float summonTimer;

    private float skillTimer;
    private bool isCastingSkill;

    private Transform target;
    private Rigidbody2D rb;

    private bool isAttacking;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (bossHeath == null)
            bossHeath = GetComponent<BossHeath>();
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
    }

    private void Update()
    {
        // ===== DEATH =====
        if (!isDead &&
            bossHeath != null &&
            bossHeath.currentHeath <= 0)
        {
            isDead = true;

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            PlayDeath();

            Debug.Log("Boss Slime chết!");

            return;
        }

        if (isDead)
            return;

        skillTimer += Time.deltaTime;

        if (skillTimer >= skillCooldown && !isCastingSkill)
        {
            skillTimer = 0f;
            StartCoroutine(ShootSkill());
        }

        if (target == null || isCastingSkill)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        summonTimer += Time.deltaTime;

        if (summonTimer >= summonCooldown && !isCastingSkill)
        {
            summonTimer = 0f;
            StartCoroutine(SummonSkill());
        }

        // Lật hướng
        sprite.flipX =
            target.position.x < transform.position.x;

        isAttacking = false;

        // Move
        Vector2 direction =
            (target.position - transform.position)
            .normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.deltaTime
        );

        PlayIdle();
    }

    public void PlayIdle()
    {
        if (isDead) return;

        anim.Play("idle");
    }

    public void PlayAttack()
    {
        if (isDead) return;

        anim.Play("attack");
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    private System.Collections.IEnumerator ShootSkill()
    {
        isCastingSkill = true;

        anim.Play("attack");

        yield return new WaitForSeconds(0.5f);

        FireBullets();

        yield return new WaitForSeconds(0.5f);

        isCastingSkill = false;
    }

    private void FireBullets()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;

            Quaternion rot =
                Quaternion.Euler(0, 0, angle);

            Instantiate(
                bulletPrefab,
                firePoint.position,
                rot
            );
        }
    }

    private System.Collections.IEnumerator SummonSkill()
    {
        isCastingSkill = true;

        // Chạy animation attack
        anim.Play("attack");

        // Chờ animation chạy một chút
        yield return new WaitForSeconds(0.5f);

        SummonEnemies();

        // Đợi animation kết thúc
        yield return new WaitForSeconds(0.5f);

        isCastingSkill = false;
    }

    private void SummonEnemies()
    {
        if (enemyPrefab == null)
            return;

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 randomPos =
                (Vector2)transform.position +
                Random.insideUnitCircle * summonRadius;

            Instantiate(
                enemyPrefab,
                randomPos,
                Quaternion.identity
            );
        }
    }
}