using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossFireController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Fireball Skill")]
    public GameObject fireballPrefab;
    public Transform fireballPoint;
    public float fireballCooldown = 8f;

    [Header("Fireball Burst")]
    public int fireballCount = 30;
    public float fireballInterval = 0.08f;
    public float randomAngle = 30f;

    [Header("Spit Fire Skill")]
    public GameObject spitFireObject;
    public float spitFireCooldown = 12f;
    public float spitFireDuration = 1f;

    [Header("Fireball Cast")]
    public float fireballCastTime = 1f;

    [Header("Triple Shot")]
    public int bulletsPerBurst = 3;
    public float burstInterval = 0.08f;

    private float fireballTimer;
    private float spitFireTimer;

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

        // ===== TIMER =====
        fireballTimer += Time.deltaTime;
        spitFireTimer += Time.deltaTime;

        // ===== FIREBALL =====
        if (fireballTimer >= fireballCooldown &&
            !isCastingSkill)
        {
            fireballTimer = 0f;
            StartCoroutine(FireballSkill());
            return;
        }

        // ===== SPIT FIRE =====
        if (spitFireTimer >= spitFireCooldown &&
            !isCastingSkill)
        {
            spitFireTimer = 0f;
            StartCoroutine(SpitFireSkill());
        }

        if (isCastingSkill)
            return;

        // ===== LOOK PLAYER =====
        Vector3 rot = bossVisual.localEulerAngles;

        if (target.position.x > transform.position.x)
        {
            rot.y = 0f;
        }
        else
        {
            rot.y = 180f;
        }

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

    // ==================================
    // ANIMATION
    // ==================================

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

    public void PlayFireball()
    {
        if (isDead) return;

        anim.Play("fireball");
    }

    public void PlaySpitFire()
    {
        if (isDead) return;

        anim.Play("spitfire");
    }

    public void PlayDeath()
    {
        if (anim == null)
            return;

        anim.Play("death");
    }

    // ==================================
    // FIREBALL
    // ==================================

    private IEnumerator FireballSkill()
    {
        isCastingSkill = true;

        // Dừng boss
        rb.linearVelocity = Vector2.zero;

        // Khóa animation khác
        PlayFireball();

        // Đợi animation bắt đầu
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < fireballCount; i++)
        {
            if (isDead)
                yield break;

            SpawnRandomFireball();

            yield return new WaitForSeconds(
                fireballInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    private void SpawnRandomFireball()
    {
        if (fireballPrefab == null ||
            fireballPoint == null)
            return;

        float randomOffset =
            Random.Range(-randomAngle, randomAngle);

        Instantiate(
            fireballPrefab,
            fireballPoint.position,
            fireballPoint.rotation *
            Quaternion.Euler(0, 0, randomOffset)
        );
    }

    // ==================================
    // SPIT FIRE
    // ==================================

    private IEnumerator SpitFireSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity = Vector2.zero;

        PlaySpitFire();

        // đợi animation bắt đầu
        yield return new WaitForSeconds(0.2f);

        if (spitFireObject != null)
            spitFireObject.SetActive(true);

        // giữ phun lửa 1 giây
        yield return new WaitForSeconds(spitFireDuration);

        if (spitFireObject != null)
            spitFireObject.SetActive(false);

        PlayIdle();

        isCastingSkill = false;
    }
}