using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMinoController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("RedBull Skill")]
    public float redBullCooldown = 10f;
    public float chargeSpeed = 10f;
    public float chargeDuration = 2f;

    [Header("RedBull Effect")]
    public GameObject redBullEffect;

    [Header("Shoot Skill")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootCooldown = 5f;
    public int bulletCount = 5;

    private float shootTimer;

    private Vector2 chargeDirection;

    private float redBullTimer;

    private bool isCastingSkill;
    private bool isDead;
    private bool isBuffed;

    private float originalSpeed;

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
            bossHeath = GetComponent<BossHeath>();

        originalSpeed = moveSpeed;
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;

        if (redBullEffect != null)
            redBullEffect.SetActive(false);
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
            return;
        }

        if (isDead)
            return;

        if (target == null)
            return;

        // ===== SKILL TIMER =====
        redBullTimer += Time.deltaTime;

        if (redBullTimer >= redBullCooldown &&
            !isCastingSkill &&
            !isBuffed)
        {
            redBullTimer = 0f;
            StartCoroutine(RedBullSkill());
            return;
        }

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootCooldown &&
            !isCastingSkill &&
            !isBuffed)
        {
            shootTimer = 0f;

            StartCoroutine(ShootSkill());

            return;
        }

        if (isCastingSkill || isBuffed)
            return;

        // ===== LOOK PLAYER =====
        Vector3 rot = bossVisual.localEulerAngles;

        if (target.position.x > transform.position.x)
            rot.y = 0f;
        else
            rot.y = 180f;

        bossVisual.localEulerAngles = rot;

        // ===== MOVE =====
        Vector2 direction =
            (target.position - transform.position)
            .normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.deltaTime
        );

        PlayIdle();
    }

    // ==========================
    // ANIMATION
    // ==========================

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

    public void PlayRedBull()
    {
        if (isDead) return;

        anim.Play("redbull");
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    // ==========================
    // REDBULL SKILL
    // ==========================

    private IEnumerator RedBullSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        chargeDirection =
            (target.position - transform.position)
            .normalized;

        PlayRedBull();

        // Đợi 0.2 giây rồi hiện effect
        yield return new WaitForSeconds(0.2f);

        // Hiện effect
        if (redBullEffect != null)
            redBullEffect.SetActive(true);

        yield return new WaitForSeconds(1f);

        isBuffed = true;

        float timer = 0f;

        while (timer < chargeDuration)
        {
            rb.MovePosition(
                rb.position +
                chargeDirection *
                chargeSpeed *
                Time.fixedDeltaTime
            );

            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;

        isBuffed = false;

        // Tắt effect
        if (redBullEffect != null)
            redBullEffect.SetActive(false);

        PlayIdle();

        isCastingSkill = false;
    }

    private IEnumerator ShootSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        PlayAttack();

        // đợi animation attack
        yield return new WaitForSeconds(0.3f);
        FireBullets();


        yield return new WaitForSeconds(0.55f);
        FireBullets();

        yield return new WaitForSeconds(0.3f);

        PlayIdle();

        isCastingSkill = false;
    }

    private void FireBullets()
    {
        if (bulletPrefab == null ||
            firePoint == null ||
            target == null)
            return;

        Vector2 direction =
            (target.position - firePoint.position).normalized;

        float centerAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float spread = 45f;

        if (bulletCount <= 1)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.Euler(0, 0, centerAngle)
            );

            return;
        }

        for (int i = 0; i < bulletCount; i++)
        {
            float angle =
                centerAngle -
                spread / 2f +
                (spread / (bulletCount - 1)) * i;

            Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.Euler(0, 0, angle)
            );
        }
    }
} 