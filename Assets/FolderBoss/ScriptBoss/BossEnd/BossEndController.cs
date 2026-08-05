using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEndController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public BossHeath bossHeath;
    public Transform bossVisual;

    [Header("Phase Visual Objects")]
    [Tooltip("Các GameObject / Effect chỉ bật ở Phase 1 (Sẽ tự tắt khi sang Phase 2)")]
    public GameObject[] phase1Objects;

    [Tooltip("Các GameObject / Visual / Aura mới xuất hiện ở Phase 2 trở đi")]
    public GameObject[] phase2Objects;

    [Header("Range")]
    public float detectRange = 15f;
    public float keepDistance = 5f;
    public float dangerDistance = 2f;

    [Header("Move")]
    public float moveSpeed = 3f;
    public float dashSpeed = 10f;

    [Header("Thinking")]
    public float thinkMin = .5f;
    public float thinkMax = 1.5f;

    [Header("Phase")]
    public int currentPhase = 1;
    bool changingPhase;

    [Header("Phase 3 Dodge")]
    public LayerMask bulletLayer;
    public LayerMask wallLayer;
    public float detectBulletRadius = 4f;
    public float dodgeDistance = 4f;
    bool dodging;

    [Header("UI")]
    public bool enableManaUI = true;

    [Header("Skills")]
    public BossSkillShoot shootSkill;
    public BossSkillDashShoot dashShootSkill;
    public BossSkillBulletRain bulletRainSkill;
    public BossSkillTeleport teleportSkill;

    protected bool movementLocked;
    protected bool usingSkill;

    protected Rigidbody2D rb;
    protected Transform target;

    protected bool isThinking;
    protected bool isMoving;
    protected bool isDead;

    [Header("Phase 3 Dodge Settings")]
    [Tooltip("Boss chỉ được phép né bao nhiêu lần")]
    public int maxDodgeCount = 5;
    private int currentDodgeCount = 0;

    //--------------------------------
    // UI Mana (Số lần né)
    //--------------------------------

    [Header("Boss Mana UI")]
    public Image manaFill;
    public Image manaUIRoot;

    private float targetManaFill = 1f;
    public float manaSmoothSpeed = 8f;

    private bool canDodge = true;

    //--------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        //--------------------------------
        // Auto Find UI Mana
        //--------------------------------

        if (!enableManaUI)
            return;

        manaUIRoot = FindImageByName("BossManaBar");
        manaFill = FindImageByName("BossManaFill");

        if (manaUIRoot != null)
        {
            manaUIRoot.gameObject.SetActive(false);
        }

        if (manaFill != null)
        {
            manaFill.gameObject.SetActive(false);
            manaFill.fillAmount = 1f;
        }
    }

    protected virtual void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;

        // Khởi tạo trạng thái GameObject ban đầu theo Phase 1
        SetPhaseVisuals(1);
    }

    //--------------------------------

    void Update()
    {
        // Smooth Mana UI
        if (enableManaUI && manaFill != null)
        {
            manaFill.fillAmount = Mathf.Lerp(
                manaFill.fillAmount,
                targetManaFill,
                manaSmoothSpeed * Time.deltaTime
            );
        }

        // DEATH
        if (!isDead && bossHeath.currentHeath <= 0)
        {
            isDead = true;

            StopAllCoroutines();

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            LockMovement(true);

            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            PlayDeath();

            StartCoroutine(DestroyRoutine());

            return;
        }

        FlipToPlayer();

        // Né đạn ưu tiên cao nhất (Phase 3)
        if (currentPhase == 3 && !dodging && canDodge)
        {
            DetectDangerBullet();
        }

        if (dodging)
            return;

        if (movementLocked || usingSkill || isThinking || isMoving)
            return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > detectRange)
        {
            PlayIdle();
            return;
        }

        UpdatePhase();

        StartCoroutine(Think());
    }

    //--------------------------------

    protected virtual IEnumerator Think()
    {
        isThinking = true;

        PlayIdle();

        yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance < dangerDistance)
        {
            int action = Random.Range(0, 2);

            switch (action)
            {
                case 0:
                    yield return StartCoroutine(DashBack());
                    break;

                case 1:
                    yield return StartCoroutine(WalkBack());
                    break;
            }
        }
        else
        {
            if (currentPhase == 1)
            {
                int action = Random.Range(0, 7);

                switch (action)
                {
                    case 0: yield return StartCoroutine(WalkToPlayer()); break;
                    case 1: yield return StartCoroutine(CircleMove()); break;
                    case 2: yield return StartCoroutine(DashSide()); break;
                    case 3: yield return StartCoroutine(UseShootSkill()); break;
                    case 4: yield return StartCoroutine(UseDashShootSkill()); break;
                    case 5: yield return StartCoroutine(bulletRainSkill.Cast()); break;
                    case 6: yield return StartCoroutine(UseTeleportSkill()); break;
                }
            }
            else
            {
                int roll = Random.Range(0, 100);

                if (roll < 10) yield return StartCoroutine(WalkToPlayer());
                else if (roll < 20) yield return StartCoroutine(CircleMove());
                else if (roll < 40) yield return StartCoroutine(UseShootSkill());
                else if (roll < 60) yield return StartCoroutine(UseDashShootSkill());
                else if (roll < 80) yield return StartCoroutine(UseTeleportSkill());
                else yield return StartCoroutine(bulletRainSkill.Cast());
            }
        }

        isThinking = false;
    }

    protected virtual void UpdatePhase()
    {
        if (changingPhase)
            return;

        float hpPercent = (float)bossHeath.currentHeath / bossHeath.maxHeath;

        if (hpPercent <= .35f && currentPhase < 3)
        {
            StartCoroutine(ChangePhase(3));
        }
        else if (hpPercent <= .7f && currentPhase < 2)
        {
            StartCoroutine(ChangePhase(2));
        }
    }

    protected IEnumerator ChangePhase(int phase)
    {
        changingPhase = true;

        currentPhase = phase;

        LockMovement(true);

        PlayAttack();

        // Ẩn/Hiện GameObjects tương ứng theo Phase vừa chuyển
        SetPhaseVisuals(phase);

        yield return new WaitForSeconds(1f);

        if (phase == 2)
        {
            thinkMin = .2f;
            thinkMax = .7f;
        }

        if (phase == 3)
        {
            thinkMin = .1f;
            thinkMax = .4f;
        }

        LockMovement(false);

        changingPhase = false;
    }

    /// <summary>
    /// Hàm xử lý bật/tắt GameObject giữa các Phase
    /// </summary>
    private void SetPhaseVisuals(int phase)
    {
        if (phase == 1)
        {
            ToggleObjectArray(phase1Objects, true);
            ToggleObjectArray(phase2Objects, false);
        }
        else if (phase >= 2)
        {
            ToggleObjectArray(phase1Objects, false);
            ToggleObjectArray(phase2Objects, true);
        }
    }

    private void ToggleObjectArray(GameObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    void DetectDangerBullet()
    {
        Collider2D[] bullets = Physics2D.OverlapCircleAll(
            transform.position,
            detectBulletRadius,
            bulletLayer
        );

        foreach (Collider2D bullet in bullets)
        {
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb == null) continue;

            // 1. Xác định hướng bay của đạn:
            // Nếu đạn có linearVelocity thì lấy linearVelocity, 
            // nếu không có (đang aim / dùng MovePosition) thì lấy hướng đầu đạn (transform.right)
            Vector2 bulletDirection = bulletRb.linearVelocity.sqrMagnitude > 0.01f
                ? bulletRb.linearVelocity.normalized
                : (Vector2)bullet.transform.right;

            Vector2 toBoss = (transform.position - bullet.transform.position).normalized;

            float dot = Vector2.Dot(bulletDirection, toBoss);

            if (dot > .8f)
            {
                StartCoroutine(TeleportDodge(bulletRb));
                return;
            }
        }
    }

    protected IEnumerator TeleportDodge(Rigidbody2D bulletRb)
    {
        dodging = true;
        isMoving = true;

        currentDodgeCount++;
        targetManaFill = 1f - (float)currentDodgeCount / maxDodgeCount;

        if (currentDodgeCount >= maxDodgeCount)
        {
            canDodge = false;
        }

        PlayRun();

        Vector2 bulletDir = bulletRb.linearVelocity.normalized;
        Vector2 bestPos = rb.position;
        float bestScore = -9999f;

        Collider2D bossCol = GetComponent<Collider2D>();
        Vector2 colSize = bossCol != null ? bossCol.bounds.size : new Vector2(1f, 1f);

        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector2 targetPos = rb.position + dir * dodgeDistance;

            RaycastHit2D hit = Physics2D.BoxCast(rb.position, colSize, 0f, dir, dodgeDistance, wallLayer);
            if (hit.collider != null) continue;

            Collider2D wallOverlap = Physics2D.OverlapBox(targetPos, colSize * 0.9f, 0f, wallLayer);
            if (wallOverlap != null) continue;

            float playerDist = Mathf.Abs(Vector2.Distance(targetPos, target.position) - keepDistance);
            Vector2 toPos = (targetPos - bulletRb.position).normalized;
            float bulletDot = Vector2.Dot(bulletDir, toPos);
            float bulletDanger = bulletDot > 0.7f ? 100f : 0f;

            float score = -playerDist - bulletDanger;

            if (score > bestScore)
            {
                bestScore = score;
                bestPos = targetPos;
            }
        }

        rb.position = bestPos;

        yield return new WaitForSeconds(0.25f);

        isMoving = false;
        dodging = false;
    }

    protected IEnumerator UseShootSkill()
    {
        usingSkill = true;
        yield return StartCoroutine(shootSkill.Cast());
        usingSkill = false;
    }

    protected IEnumerator WalkToPlayer()
    {
        isMoving = true;
        PlayRun();

        float timer = Random.Range(.5f, 1.5f);

        while (timer > 0)
        {
            Vector2 dir = (target.position - transform.position).normalized;

            if (Physics2D.Raycast(rb.position, dir, 0.6f, wallLayer))
                break;

            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator WalkBack()
    {
        isMoving = true;
        PlayRun();

        float timer = 1f;

        while (timer > 0)
        {
            Vector2 dir = (transform.position - target.position).normalized;

            if (Physics2D.Raycast(rb.position, dir, 0.6f, wallLayer))
                break;

            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator DashBack()
    {
        isMoving = true;
        PlayRun();

        Vector2 dir = (transform.position - target.position).normalized;
        float timer = .3f;

        while (timer > 0)
        {
            if (Physics2D.Raycast(rb.position, dir, 0.8f, wallLayer))
                break;

            rb.MovePosition(rb.position + dir * dashSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator DashSide()
    {
        isMoving = true;
        PlayRun();

        Vector2 dir = Random.value > .5f ? Vector2.right : Vector2.left;
        float timer = .3f;

        while (timer > 0)
        {
            if (Physics2D.Raycast(rb.position, dir, 0.8f, wallLayer))
                break;

            rb.MovePosition(rb.position + dir * dashSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator CircleMove()
    {
        isMoving = true;
        PlayRun();

        float timer = 1f;
        Vector2 side = Random.value > .5f ? Vector2.right : Vector2.left;

        while (timer > 0)
        {
            rb.MovePosition(rb.position + side * moveSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator UseDashShootSkill()
    {
        usingSkill = true;
        yield return StartCoroutine(dashShootSkill.Cast());
        usingSkill = false;
    }

    public void LockMovement(bool value)
    {
        movementLocked = value;

        if (value)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void FlipToPlayer()
    {
        if (target == null)
            return;

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
    }

    protected IEnumerator UseTeleportSkill()
    {
        usingSkill = true;
        yield return StartCoroutine(teleportSkill.Cast());
        usingSkill = false;
    }

    protected IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private Image FindImageByName(string imageName)
    {
        Image[] images = Resources.FindObjectsOfTypeAll<Image>();

        foreach (Image img in images)
        {
            if (img.name == imageName)
                return img;
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enableManaUI) return;
        if (!other.CompareTag("Player")) return;

        if (manaUIRoot != null) manaUIRoot.gameObject.SetActive(true);
        if (manaFill != null) manaFill.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!enableManaUI) return;
        if (!other.CompareTag("Player")) return;

        if (manaUIRoot != null) manaUIRoot.gameObject.SetActive(false);
        if (manaFill != null) manaFill.gameObject.SetActive(false);
    }

    void PlayDeath() { anim.Play("death"); }
    public void PlayIdle() { anim.Play("idle"); }
    public void PlayRun() { anim.Play("run"); }
    public void PlayAttack() { anim.Play("attack", 0, 0f); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, keepDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectBulletRadius);
    }
}