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

    [Header("Invisible Skill")]
    public float invisibleCooldown = 12f;
    public float invisibleDuration = 4f;

    public BoxCollider2D bodyCollider;

    [Header("Random Move Area")]
    public float randomMoveRadius = 6f;
    public LayerMask wallLayer;

    [Header("Invisible Move")]
    public float invisibleMoveSpeed = 5f;

    private float invisibleTimer;
    private bool isInvisible;

    private Vector2 randomTarget;

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

        if (bodyCollider == null)
            bodyCollider =
                GetComponent<BoxCollider2D>();
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

        // ===== INVISIBLE =====

        invisibleTimer += Time.deltaTime;

        if (invisibleTimer >= invisibleCooldown &&
            !isCastingSkill &&
            !isInvisible)
        {
            invisibleTimer = 0f;

            StartCoroutine(
                InvisibleSkill()
            );

            return;
        }

        // Đang tàng hình chỉ random move
        if (isInvisible)
        {
            MoveRandomInvisible();
            return;
        }

        // Khóa toàn bộ skill khác
        if (isCastingSkill)
            return;

        // ===== CAST =====

        castTimer += Time.deltaTime;

        if (castTimer >= castCooldown)
        {
            castTimer = 0f;

            StartCoroutine(
                CastSkill()
            );

            return;
        }

        // ===== LOOK PLAYER =====

        Vector3 rot =
            bossVisual.localEulerAngles;

        rot.y =
            target.position.x >
            transform.position.x
            ? 0f
            : 180f;

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

    // =========================
    // ANIMATION
    // =========================

    public void PlayIdle()
    {
        if (isDead || isInvisible)
            return;

        anim.Play("idle");
    }

    public void PlayAttack()
    {
        if (isDead) return;

        anim.Play("attack");
    }

    public void PlayCast()
    {
        if (isDead || isInvisible)
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

        rb.linearVelocity =
            Vector2.zero;

        PlayCast();

        // đợi animation bắt đầu
        yield return new WaitForSeconds(
            0.5f
        );

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

    private void MoveRandomInvisible()
    {
        // Nếu tới gần điểm hoặc chưa có điểm
        if (Vector2.Distance(
            transform.position,
            randomTarget) < 0.5f)
        {
            PickRandomPoint();
        }

        Vector2 direction =
            (randomTarget -
            (Vector2)transform.position)
            .normalized;

        rb.MovePosition(
        rb.position +
        direction *
        invisibleMoveSpeed *
        Time.deltaTime
        );
    }

    private void PickRandomPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 pos =
                (Vector2)transform.position +
                Random.insideUnitCircle *
                randomMoveRadius;

            // kiểm tra có đụng tường không
            Collider2D wall =
                Physics2D.OverlapCircle(
                    pos,
                    0.5f,
                    wallLayer
                );

            if (wall == null)
            {
                randomTarget = pos;
                return;
            }
        }

        // nếu không tìm được
        randomTarget =
            transform.position;
    }

    private IEnumerator InvisibleSkill()
    {
        isCastingSkill = true;
        isInvisible = true;

        rb.linearVelocity =
            Vector2.zero;

        sprite.enabled = false;

        if (bodyCollider != null)
            bodyCollider.enabled = false;

        anim.enabled = false;

        PickRandomPoint();

        yield return new WaitForSeconds(
            invisibleDuration
        );

        sprite.enabled = true;

        if (bodyCollider != null)
            bodyCollider.enabled = true;

        anim.enabled = true;

        PlayIdle();

        isInvisible = false;
        isCastingSkill = false;
    }
}