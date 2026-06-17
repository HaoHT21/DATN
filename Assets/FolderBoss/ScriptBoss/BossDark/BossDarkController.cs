using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossDarkController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Cast Skill")]
    public GameObject bulletCastPrefab;
    public Transform castPoint;

    public float castCooldown = 6f;
    public int castCount = 3;
    public float castInterval = 0.5f;

    private float castTimer;

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
        castTimer += Time.deltaTime;

        if (castTimer >= castCooldown &&
            !isCastingSkill)
        {
            castTimer = 0f;

            StartCoroutine(CastSkill());

            return;
        }
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

    public void PlayCast()
    {
        if (isDead)
            return;

        anim.Play("cast");
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    private IEnumerator CastSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        PlayCast();

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < castCount; i++)
        {
            SpawnCastBullet();

            yield return new WaitForSeconds(
                castInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    private void SpawnCastBullet()
    {
        if (bulletCastPrefab == null ||
            castPoint == null)
            return;

        Instantiate(
            bulletCastPrefab,
            castPoint.position,
            Quaternion.identity
        );
    }
}