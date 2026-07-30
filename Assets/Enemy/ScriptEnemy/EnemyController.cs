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

        if (target == null || movementLocked)
        {
            if (!movementLocked && target == null)
            {
                StopMovement();
                PlayAnimation("idle");
            }
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
        if (movementLocked || target == null || !seeker.IsDone() || isDead)
            return;

        seeker.StartPath(rb.position, target.position, OnPathComplete);
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

        if (currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
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
        if (value) StopMovement();
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
        if (target != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }
    #endregion

    #region Animation & Hurt & Death
    public void PlayAnimation(string animName, bool forceReset = false)
    {
        if (animator == null || currentAnim == "death") return;

        // Nếu trùng animation và không yêu cầu phát lại từ đầu thì bỏ qua
        if (currentAnim == animName && !forceReset) return;

        currentAnim = animName;

        // Chỉ phát lại từ frame 0 nếu là attack/hurt hoặc được yêu cầu forceReset
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
        LockMovement(true);

        // Reset trạng thái animation để phát hurt chuẩn xác
        currentAnim = "";
        PlayAnimation("hurt", true);

        // Đẩy lùi (Knockback)
        rb.linearVelocity = hitDirection * knockbackForce;

        yield return new WaitForSeconds(hurtDuration);

        // Xong Hurt
        rb.linearVelocity = Vector2.zero;
        isHurting = false;
        LockMovement(false);
        hurtCoroutine = null;

        // Reset currentAnim về rỗng để ép Animator chuyển sang state mới ngay lập tức
        currentAnim = "";
        PlayAnimation(target != null ? "run" : "idle", true);
    }

    void HandleDeath()
    {
        isDead = true;
        StopMovement();
        currentAnim = "";
        PlayAnimation("death", true);
    }
    #endregion
}