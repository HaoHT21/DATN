using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BossController : MonoBehaviour
{
    public enum BossState { Idle, Moving, Thinking, UsingSkill, Dead }

    [Header("Base State")]
    protected BossState currentState = BossState.Idle;
    protected int currentPhase = 1;

    [Header("References")]
    public Animator anim;
    public BossHeath bossHeath;
    public Transform bossVisual;
    public SpriteRenderer spriteRenderer;
    public Collider2D[] hitColliders;
    public SpriteRenderer[] sprites;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float detectRange = 15f;
    public float randomMoveRadius = 3f;
    public float randomMoveInterval = 2f;
    public LayerMask wallLayer;

    [Header("Thinking Settings")]
    public float thinkMin = 0.5f;
    public float thinkMax = 1.5f;

    protected Rigidbody2D rb;
    protected Transform target;
    protected Vector2 randomTarget;
    protected float moveTimer;

    [Header("Phase Change VFX/SFX")]
    public GameObject phase2VFXObject;
    public AudioClip phase2SFX;
    public Transform vfxSpawnPoint;

    [Header("Dodge Settings (Phase 2 Only)")]
    public float dodgeDetectRadius = 4f;        // Vùng tròn phát hiện đạn
    [Range(0, 100)] public float dodgeChancePhase2 = 70f; // Tỉ lệ né ở Phase 2
    public float dodgeDistance = 3f;             // Khoảng cách lướt né
    public float dodgeDuration = 0.2f;            // Thời gian lướt né
    public float dodgeCooldown = 1.5f;            // Thời gian hồi né
    public LayerMask bulletLayer;                 // Layer đạn Player

    protected bool isDodging = false;
    protected float lastDodgeTime = -999f;
    protected bool canDodgeDuringSkill = false;  // Bật true trong Skill con nếu muốn cho phép né khi dùng skill đó
    private Coroutine currentDodgeCoroutine;

    // Structure đăng ký Skill
    [System.Serializable]
    protected struct SkillWeight
    {
        public System.Func<IEnumerator> skillMethod;
        public int weightPhase1;
        public int weightPhase2;

        public SkillWeight(System.Func<IEnumerator> method, int p1, int p2)
        {
            skillMethod = method;
            weightPhase1 = p1;
            weightPhase2 = p2;
        }
    }

    protected List<SkillWeight> bossSkills = new List<SkillWeight>();

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (bossHeath == null) bossHeath = GetComponent<BossHeath>();
    }

    protected virtual void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;

        if (phase2VFXObject != null) phase2VFXObject.SetActive(false);

        RegisterBossSkills();
        PickRandomPosition();
        StartCoroutine(MainAILoop());
    }

    protected virtual void Update()
    {
        CheckDeath();
        if (currentState == BossState.Dead || target == null) return;

        UpdatePhase();
        FlipToPlayer();

        // 1. QUAN TRỌNG: Quét né đạn liên tục ở Phase 2
        if (currentPhase >= 2)
        {
            CheckIncomingBullets();
        }

        // 2. Nếu đang lướt né -> Dừng mọi di chuyển khác
        if (isDodging) return;

        // 3. Chỉ thực thi di chuyển khi ở trạng thái Moving
        if (currentState == BossState.Moving)
        {
            ExecuteMovement();
        }
    }

    // ------------------------------------------------
    // Core AI Loop
    // ------------------------------------------------
    protected virtual IEnumerator MainAILoop()
    {
        while (currentState != BossState.Dead)
        {
            if (target == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance > detectRange)
            {
                currentState = BossState.Idle;
                PlayIdle();
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Đang suy nghĩ chọn hành động tiếp theo
            // Trạng thái suy nghĩ
            currentState = BossState.Thinking;
            PlayIdle();
            yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));

            // Chọn và thực thi hành động
            yield return StartCoroutine(DecideNextAction());
        }
    }

    protected virtual IEnumerator DecideNextAction()
    {
        int totalWeight = 0;
        foreach (var s in bossSkills)
            totalWeight += (currentPhase == 1) ? s.weightPhase1 : s.weightPhase2;

        // Nếu không có skill hoặc weight = 0 -> Đi dạo
        if (bossSkills.Count == 0 || totalWeight <= 0)
        {
            yield return StartCoroutine(DoMoveBehavior(2f));
            yield break;
        }

        int roll = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (var s in bossSkills)
        {
            int weight = (currentPhase == 1) ? s.weightPhase1 : s.weightPhase2;
            currentSum += weight;

            if (roll < currentSum)
            {
                if (s.skillMethod != null)
                {
                    currentState = BossState.UsingSkill;
                    canDodgeDuringSkill = false; // Mặc định không né khi ra chiêu

                    yield return StartCoroutine(s.skillMethod());

                    // QUAN TRỌNG: Sau khi dùng Skill xong, phải đưa về Idle
                    currentState = BossState.Idle;
                }
                yield break;
            }
        }
    }

    // Hàm cho phép Boss di chuyển dạo một khoảng thời gian ngắn
    protected IEnumerator DoMoveBehavior(float duration)
    {
        currentState = BossState.Moving;
        PlayRun(); // Bật Animation Run khi bắt đầu di chuyển dạo
        PickRandomPosition();

        float timer = 0f;
        while (timer < duration)
        {
            if (!isDodging) timer += Time.deltaTime;
            yield return null;
        }

        currentState = BossState.Idle;
        PlayIdle(); // Quay về Animation Idle khi kết thúc di chuyển dạo
    }

    protected abstract void RegisterBossSkills();

    // ------------------------------------------------
    // Logic Né Đạn Tối Ưu
    // ------------------------------------------------
    protected virtual void CheckIncomingBullets()
    {
        if (isDodging || Time.time < lastDodgeTime + dodgeCooldown) return;

        // Nếu đang trong Skill, chỉ né khi cờ canDodgeDuringSkill = true
        if (currentState == BossState.UsingSkill && !canDodgeDuringSkill) return;

        Collider2D[] incomingBullets = Physics2D.OverlapCircleAll(transform.position, dodgeDetectRadius, bulletLayer);
        if (incomingBullets.Length == 0) return;

        foreach (var bullet in incomingBullets)
        {
            Transform bulletTransform = bullet.transform;
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            // Xử lý lấy hướng đạn chuẩn xác
            Vector2 bulletDirection = Vector2.zero;
            if (bulletRb != null && bulletRb.linearVelocity.sqrMagnitude > 0.01f)
            {
                bulletDirection = bulletRb.linearVelocity.normalized;
            }
            else
            {
                bulletDirection = bulletTransform.right; // Nếu không có Rigidbody, lấy hướng mặc định của đạn
            }

            Vector2 dirToBoss = ((Vector2)transform.position - (Vector2)bulletTransform.position).normalized;

            // Kiểm tra đạn có đang hướng về phía Boss
            if (Vector2.Dot(bulletDirection, dirToBoss) > -0.2f)
            {
                if (Random.Range(0f, 100f) <= dodgeChancePhase2)
                {
                    TryPerformSafeDodge(bulletDirection);
                    break;
                }
            }
        }
    }

    protected void TryPerformSafeDodge(Vector2 bulletDirection)
    {
        Vector2 sideDir1 = Vector2.Perpendicular(bulletDirection);
        Vector2 sideDir2 = -sideDir1;

        bool preferFirst = Random.value > 0.5f;
        Vector2 primaryDir = preferFirst ? sideDir1 : sideDir2;
        Vector2 secondaryDir = preferFirst ? sideDir2 : sideDir1;

        Vector2 safeDodgeDir = Vector2.zero;

        // Kiểm tra hướng an toàn không vướng tường
        if (IsDirectionSafe(primaryDir, dodgeDistance))
        {
            safeDodgeDir = primaryDir;
        }
        else if (IsDirectionSafe(secondaryDir, dodgeDistance))
        {
            safeDodgeDir = secondaryDir;
        }
        else if (IsDirectionSafe(-bulletDirection, dodgeDistance))
        {
            safeDodgeDir = -bulletDirection; // Né tiến lên phía đạn
        }

        if (safeDodgeDir != Vector2.zero)
        {
            if (currentDodgeCoroutine != null) StopCoroutine(currentDodgeCoroutine);
            currentDodgeCoroutine = StartCoroutine(PerformDodge(safeDodgeDir));
        }
    }

    protected bool IsDirectionSafe(Vector2 direction, float distance)
    {
        float bossRadius = 0.5f;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, bossRadius, direction, distance, wallLayer);
        return hit.collider == null;
    }

    protected IEnumerator PerformDodge(Vector2 dodgeDir)
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + dodgeDir.normalized * dodgeDistance;

        rb.linearVelocity = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < dodgeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dodgeDuration;

            // Động tác lướt mượt bằng Ease-Out
            float easeOutProgress = 1f - Mathf.Pow(1f - progress, 3);
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, easeOutProgress));

            yield return null;
        }

        rb.MovePosition(targetPos);
        isDodging = false;

        // Nếu né xong mà không ở trạng thái Moving thì cho về Idle
        // if (currentState != BossState.Moving)
        // {
        //     PlayIdle();
        // }
    }

    // ------------------------------------------------
    // Movement Logic
    // ------------------------------------------------

    protected virtual void ExecuteMovement()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            moveTimer = randomMoveInterval;
            PickRandomPosition();
        }

        Vector2 dir = randomTarget - rb.position;
        if (dir.magnitude >= 0.1f)
        {
            PlayRun(); // Đang di chuyển thực sự -> Chạy Anim Run
            rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
            PlayIdle(); // Đã đến đích -> Quay về Anim Idle
        }
    }

    protected virtual void PickRandomPosition()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 candidate = (Vector2)transform.position + Random.insideUnitCircle * randomMoveRadius;
            if (!Physics2D.Linecast(transform.position, candidate, wallLayer))
            {
                randomTarget = candidate;
                return;
            }
        }
        randomTarget = transform.position;
    }

    // ------------------------------------------------
    // Phase & Helper Methods
    // ------------------------------------------------
    protected abstract void OnPhaseChange(int phase);

    protected virtual void DisableEffects()
    {
        if (phase2VFXObject != null) phase2VFXObject.SetActive(false);
    }

    protected virtual void UpdatePhase()
    {
        if (bossHeath == null || bossHeath.maxHeath <= 0) return;
        float hpPercent = (float)bossHeath.currentHeath / bossHeath.maxHeath;

        if (hpPercent <= 0.5f && currentPhase < 2)
        {
            currentPhase = 2;
            TriggerPhase2Effects();
            OnPhaseChange(2);
        }
    }

    protected virtual void TriggerPhase2Effects()
    {
        if (phase2VFXObject != null) phase2VFXObject.SetActive(true);

        if (phase2SFX != null && TryGetComponent<AudioSource>(out AudioSource audioSrc))
        {
            audioSrc.PlayOneShot(phase2SFX);
        }
    }

    protected virtual void CheckDeath()
    {
        if (currentState == BossState.Dead || bossHeath == null || bossHeath.currentHeath > 0) return;

        currentState = BossState.Dead;
        StopAllCoroutines();
        DisableEffects();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;

        PlayDeath();
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    protected void FlipToPlayer()
    {
        if (bossVisual == null || target == null) return;
        Vector3 rot = bossVisual.localEulerAngles;
        rot.y = target.position.x > transform.position.x ? 0 : 180;
        bossVisual.localEulerAngles = rot;
    }

    // Animation Calls
    protected void PlayIdle() => anim?.Play("idle");
    protected void PlayRun() => anim?.Play("run");
    protected void PlayAttack() => anim?.Play("attack", 0, 0);
    protected void Shoot() => anim?.Play("shoot", 0, 0);
    protected void FireBall() => anim?.Play("fireball");
    protected void SpitFire() => anim?.Play("spitfire");
    protected void IceBurst() => anim?.Play("ice");
    protected void Cast() => anim?.Play("cast");
    protected void RedBull() => anim?.Play("redbull");
    protected void Fly() => anim?.Play("fly");
    protected void Summon() => anim?.Play("summon");
    protected void PlayDeath() => anim?.Play("death");

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, randomMoveRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(randomTarget, 0.2f);

        // Hiển thị vòng tròn né đạn trong Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dodgeDetectRadius);
    }
}