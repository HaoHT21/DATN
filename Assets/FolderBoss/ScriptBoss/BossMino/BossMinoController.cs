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
    public float redBullDuration = 5f;
    public float speedMultiplier = 2f;

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

        if (isCastingSkill)
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

        PlayRedBull();

        // Thời gian uống RedBull
        yield return new WaitForSeconds(1f);

        // Buff
        isBuffed = true;
        moveSpeed = originalSpeed * speedMultiplier;

        isCastingSkill = false;

        yield return new WaitForSeconds(
            redBullDuration
        );

        // Hết buff
        moveSpeed = originalSpeed;
        isBuffed = false;
    }
} 