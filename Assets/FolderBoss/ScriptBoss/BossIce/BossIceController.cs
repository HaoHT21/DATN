using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossIceController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Ice Skill")]
    public GameObject icePrefab;
    public Transform icePoint;
    public float iceCooldown = 8f;

    [Header("Ice Burst")]
    public int iceCount = 20;

    [Header("Attack Ice")]
    public GameObject attackIcePrefab;
    public Transform attackIcePoint;
    public float attackIceCooldown = 5f;

    public int attackIceCount = 3;
    public float attackIceInterval = 0.3f;

    private float attackIceTimer;

    private float iceTimer;

    private bool isCastingSkill;
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

            return;
        }

        if (isDead)
            return;

        if (target == null)
            return;

        // ===== SKILL TIMER =====
        iceTimer += Time.deltaTime;

        if (iceTimer >= iceCooldown &&
            !isCastingSkill)
        {
            iceTimer = 0f;
            StartCoroutine(IceSkill());
            return;
        }

        if (isCastingSkill)
            return;

        attackIceTimer += Time.deltaTime;

        if (attackIceTimer >= attackIceCooldown &&
            !isCastingSkill)
        {
            attackIceTimer = 0f;

            StartCoroutine(AttackIceSkill());

            return;
        }

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

    // =========================
    // ANIMATION
    // =========================

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

    public void PlayIce()
    {
        if (isDead) return;

        anim.Play("ice");
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    // =========================
    // ICE SKILL
    // =========================

    private IEnumerator IceSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        PlayIce();

        yield return new WaitForSeconds(0.5f);

        SpawnIce();

        yield return new WaitForSeconds(0.5f);

        PlayIdle();

        isCastingSkill = false;
    }

    private void SpawnIce()
    {
        if (icePrefab == null ||
            icePoint == null)
            return;

        float angleStep =
            360f / iceCount;

        for (int i = 0; i < iceCount; i++)
        {
            float angle =
                i * angleStep;

            Quaternion rot =
                Quaternion.Euler(
                    0,
                    0,
                    angle
                );

            Instantiate(
                icePrefab,
                icePoint.position,
                rot
            );
        }
    }
    private IEnumerator AttackIceSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        for (int i = 0; i < attackIceCount; i++)
        {
            PlayAttack();

            yield return new WaitForSeconds(0.3f);

            SpawnAttackIce();

            yield return new WaitForSeconds(attackIceInterval);
        }

        PlayIdle();

        isCastingSkill = false;
    }

    private void SpawnAttackIce()
    {
        if (attackIcePrefab == null ||
            attackIcePoint == null ||
            target == null)
            return;

        Vector2 direction =
            (target.position - attackIcePoint.position)
            .normalized;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        Instantiate(
            attackIcePrefab,
            attackIcePoint.position,
            Quaternion.Euler(0, 0, angle)
        );
    }
}