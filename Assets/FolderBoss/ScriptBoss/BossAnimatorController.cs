using UnityEngine;
using System.Collections;

public class BossAnimatorController : MonoBehaviour
{
    [Header("Renderer")]
    public SpriteRenderer sprite;

    [Header("Animator")]
    public Animator anim;

    [Header("Boss Health")]
    public BossHealth bossHealth;

    [Header("Boss Move")]
    public float moveSpeed = 3f;

    [Header("Detect Player")]
    public float detectRange = 10f;

    [Header("Attack Range")]
    public float attackRange = 2f;

    [Header("Melee Attack")]
    public GameObject meleeObject;

    [Header("Máu kích hoạt Glow (%)")]
    [Range(0f, 1f)]
    public float glowThreshold = 0.1f;

    [Header("Boss Visual")]
    public Transform bossVisual;

    [HideInInspector]
    public bool isCastingLaser;

    private bool isDead;
    private bool glowActivated;

    private Transform target;

    private Rigidbody2D rb;

    private float loseTargetTimer;
    private bool isImmuneState;

    private void Start()
    {
        CheckRenderer();
    }

    // =========================
    // AUTO FIND
    // =========================
    private void Reset()
    {
        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        sprite = GetComponent<SpriteRenderer>();

        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();
    }

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();
    }

    private void Update()
    {
        if (bossHealth == null)
            return;

        // =========================
        // DEATH
        // =========================
        if (!isDead &&
    bossHealth.currentHealth <= 0)
        {
            isDead = true;

            // Khóa toàn bộ hoạt động
            isCastingLaser = true;
            isImmuneState = true;

            // Dừng di chuyển
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            PlayDeath();

            // Tắt script sau khi chạy animation chết
            enabled = false;

            return;
        }

        // =========================
        // GLOW PHASE
        // =========================
        if (!glowActivated)
        {
            float hpPercent =
                (float)bossHealth.currentHealth /
                bossHealth.maxHealth;

            if (hpPercent <= glowThreshold)
            {
                glowActivated = true;

                PlayGlow();
            }
        }

        // =========================
        // TÌM PLAYER
        // =========================
        FindPlayer();

        // =========================
        // DI CHUYỂN
        // =========================
        BossMove();

        // =========================
        // IMMUNE STATE LOGIC
        // =========================
        if (target == null)
        {
            loseTargetTimer += Time.deltaTime;

            if (loseTargetTimer >= 10f && !isImmuneState)
            {
                isImmuneState = true;
                PlayImmune();
            }
        }
        else
        {
            // có player lại → reset
            loseTargetTimer = 0f;

            if (isImmuneState)
            {
                isImmuneState = false;
                PlayIdle();
            }
        }

        if (isImmuneState && bossHealth.currentHealth < bossHealth.maxHealth)
        {
            bossHealth.Heal(1); // chỉnh tốc độ hồi máu

            // chống vượt max HP
            if (bossHealth.currentHealth > bossHealth.maxHealth)
                bossHealth.currentHealth = bossHealth.maxHealth;
        }
    }

    // =====================================
    // FIND PLAYER
    // =====================================
    void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.transform.position
                );

            if (distance <= detectRange)
            {
                target = player.transform;
            }
            else
            {
                target = null;
            }
        }
    }


    // =====================================
    // MOVE TO PLAYER
    // =====================================
    void BossMove()
    {
        if (isCastingLaser || isImmuneState)
            return;
        if (isCastingLaser)
            return;
        if (target == null)
        {
            PlayIdle();
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        // Nếu gần player thì đứng lại
        if (distance <= attackRange)
        {
            PlayMelee();

            return;
        }
        else
        {
            PlayIdle();
        }

        // Di chuyển tới player
        Vector2 direction =
            (target.position - transform.position)
            .normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.deltaTime
        );

        Vector3 rot = bossVisual.localEulerAngles;

        // nhìn phải
        if (direction.x > 0.1f)
        {
            rot.y = 0f;
        }
        // nhìn trái
        else if (direction.x < -0.1f)
        {
            rot.y = 180f;
        }

        bossVisual.localEulerAngles = rot;
    }

    // =====================================
    // IDLE
    // =====================================
    public void PlayIdle()
    {
        if (isDead) return;

        anim.Play("idle");
    }
   

    // =====================================
    // IMMUNE
    // =====================================
    public void PlayImmune()
    {
        if (isDead) return;

        anim.Play("immune");
    }

    // =====================================
    // LASER CAST
    // =====================================
    public void PlayLaserCast()
    {
        if (isDead) return;

        anim.Play("laser_cast");
    }

    // =====================================
    // MELEE
    // =====================================
    public void PlayMelee()
    {
        if (isDead) return;

        anim.Play("melee");
    }

    // =====================================
    // SHIELD CAST
    // =====================================
    public void PlayShieldCast()
    {
        if (isDead) return;

        anim.Play("sheild_cast");
    }

    // =====================================
    // SHOOT
    // =====================================
    public void PlayShoot()
    {
        if (isDead) return;

        anim.Play("shoot");
    }

    // =====================================
    // GLOW
    // =====================================
    public void PlayGlow()
    {
        if (isDead) return;

        anim.Play("glow");

        Debug.Log("Boss vào phase cuối!");
    }

    // =====================================
    // DEATH
    // =====================================
    public void PlayDeath()
    {
        if (anim == null) return;

        anim.Play("death");

        Debug.Log("Boss chết!");
    }

    // =====================================
    // CHECK RENDERER
    // =====================================
    void CheckRenderer()
    {
        if (sprite == null)
        {
            Debug.LogError("Chưa gắn SpriteRenderer cho Boss!");
            return;
        }

        Debug.Log("Đã gắn SpriteRenderer: " + sprite.name);
    }

    private void OnDrawGizmosSelected()
    {
        // Detect Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}