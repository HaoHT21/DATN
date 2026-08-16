using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Animation")]
    public float hurtDuration = 0.2f;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    [Header("Pathfinding")]
    public float nextWaypointDistance = 0.2f;
    public float updateRate = 0.25f;

    [Header("Visual")]
    public Transform enemyVisual;

    [Header("Detection & Wandering")]
    public float loseTargetDistance = 10f;
    public float wanderRadius = 5f;      // Bán kính tìm điểm đi tuần ngẫu nhiên
    public float wanderWaitTime = 2f;    // Thời gian đứng nghỉ giữa các lần đi tuần

    // Cache Stealth
    private PlayerStealth playerStealth;

    // Components
    private Seeker seeker;
    private Path path;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHeath health;

    // State variables
    private Transform target;
    private int currentWaypoint;
    private bool isHurting;
    private bool isDead;
    private bool movementLocked;
    private string currentAnim;
    private Vector2 hitDirection;
    private Coroutine hurtCoroutine;

    // Wandering State Variables
    private bool isWandering;
    private Vector2 wanderTarget;
    private Coroutine wanderCoroutine;

    // Public Getters
    public Transform Target => target;
    public bool IsHurting => isHurting;
    public bool HasTarget => target != null;
    public Rigidbody2D RB => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHeath>();
        seeker = GetComponent<Seeker>();

        if (health != null)
        {
            health.OnHurt += HandleHurt;
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHurt -= HandleHurt;
            health.OnDeath -= HandleDeath;
        }
    }

    private void Start()
    {
        PlayAnimation("idle");
        InvokeRepeating(nameof(UpdatePath), 0f, updateRate);
    }

    private void Update()
    {
        if (isDead || isHurting) return;

        FindPlayer();

        // Kiểm tra xem Player có đang trốn không
        if (target != null && playerStealth != null && playerStealth.IsHidden)
        {
            // Mất dấu Player -> Xóa target và chuyển sang đi tuần ngẫu nhiên
            ClearTargetAndStartWander();
            return;
        }

        // Nếu không có target và chưa bắt đầu Wandering -> Kích hoạt Wander
        if (target == null && !isWandering && !movementLocked)
        {
            StartWandering();
        }
    }

    private void FixedUpdate()
    {
        if (movementLocked || isDead || isHurting) return;

        FollowPath();
    }

    #region Pathfinding Logic
    void UpdatePath()
    {
        if (movementLocked || !seeker.IsDone() || isDead) return;

        // Ưu tiên 1: Đuổi theo Player nếu có Target
        if (target != null)
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        // Ưu tiên 2: Đi tuần đến điểm Wander ngẫu nhiên
        else if (isWandering && wanderTarget != Vector2.zero)
        {
            seeker.StartPath(rb.position, wanderTarget, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FollowPath()
    {
        if (path == null) return;

        // Khi đã đến cuối đường đi (Đã tới nơi)
        if (currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");

            // Nếu đang đi tuần mà tới điểm cần tới -> Bắt đầu chờ để tìm điểm tiếp theo
            if (isWandering && wanderCoroutine == null)
            {
                wanderCoroutine = StartCoroutine(WanderWaitRoutine());
            }
            return;
        }

        Vector2 next = path.vectorPath[currentWaypoint];
        Vector2 dir = (next - rb.position).normalized;

        rb.linearVelocity = dir * moveSpeed;

        Flip(dir);
        PlayAnimation("run");

        if (Vector2.Distance(rb.position, next) < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
    #endregion

    #region Wandering Logic
    void StartWandering()
    {
        isWandering = true;
        PickNewWanderTarget();
    }

    void StopWandering()
    {
        isWandering = false;
        wanderTarget = Vector2.zero;
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
    }

    void PickNewWanderTarget()
    {
        // Chọn 1 điểm ngẫu nhiên trong bán kính wanderRadius
        Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
        wanderTarget = randomPoint;
        UpdatePath();
    }

    IEnumerator WanderWaitRoutine()
    {
        // Nghỉ giữa các lần đi tuần
        yield return new WaitForSeconds(wanderWaitTime);
        if (isWandering && target == null)
        {
            PickNewWanderTarget();
        }
        wanderCoroutine = null;
    }

    void ClearTargetAndStartWander()
    {
        target = null;
        playerStealth = null;
        StopMovement();
        StartWandering();
    }
    #endregion

    #region Movement Helpers
    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        path = null;
        currentWaypoint = 0;
    }

    public void LockMovement(bool value)
    {
        movementLocked = value;
        if (value)
        {
            StopMovement();
            StopWandering();
        }
    }

    public void LookAt(Vector2 targetPos)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;
        Flip(dir);
    }

    private void Flip(Vector2 dir)
    {
        if (enemyVisual == null || Mathf.Abs(dir.x) < 0.01f) return;

        Vector3 scale = enemyVisual.localScale;
        scale.x = dir.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        enemyVisual.localScale = scale;
    }

    void FindPlayer()
    {
        // Kiểm tra nếu đã có target
        if (target != null)
        {
            if (playerStealth != null && playerStealth.IsHidden)
            {
                ClearTargetAndStartWander();
            }
            return;
        }

        // Tìm Player trong Scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStealth stealth = player.GetComponent<PlayerStealth>();

            // Nhìn thấy Player -> HỦY WANDER và QUAY LẠI ĐUỔI THEO
            if (stealth == null || !stealth.IsHidden)
            {
                target = player.transform;
                playerStealth = stealth;

                // Tắt trạng thái Wander để tập trung chốt target
                StopWandering();
                StopMovement();
            }
        }
    }
    #endregion

    #region Animation & Hurt & Death
    public void PlayAnimation(string animName, bool forceReset = false)
    {
        if (animator == null || currentAnim == "death") return;

        if (currentAnim == animName && !forceReset) return;

        currentAnim = animName;

        if (animName == "attack" || animName == "hurt" || forceReset)
        {
            animator.Play(animName, 0, 0f);
        }
        else
        {
            animator.Play(animName);
        }
    }

    public void SetHitDirection(Vector2 direction)
    {
        hitDirection = direction.normalized;
    }

    void HandleHurt()
    {
        if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
        hurtCoroutine = StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        if (isDead) yield break;

        isHurting = true;
        StopWandering();
        LockMovement(true);

        currentAnim = "";
        PlayAnimation("hurt", true);

        rb.linearVelocity = hitDirection * knockbackForce;

        yield return new WaitForSeconds(hurtDuration);

        rb.linearVelocity = Vector2.zero;
        isHurting = false;
        LockMovement(false);
        hurtCoroutine = null;

        currentAnim = "";
        PlayAnimation(target != null ? "run" : "idle", true);
    }

    void HandleDeath()
    {
        isDead = true;
        StopWandering();
        StopMovement();
        currentAnim = "";
        PlayAnimation("death", true);
    }
    #endregion
}