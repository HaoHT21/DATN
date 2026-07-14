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

    private Vector2 hitDirection;

    [Header("Visual")]
    public Transform enemyVisual;

    private Seeker seeker;

    private Path path;

    private int currentWaypoint;

    public float nextWaypointDistance = 0.2f;

    public float updateRate = 0.25f;


    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHeath health;

    private Transform target;

    private bool isHurting;
    private bool isDead;
    private bool isAlerted;
    private bool movementLocked;

    private string currentAnim;

    private Coroutine hurtCoroutine;

    public Transform Target => target;
    public bool IsHurting => isHurting;

    public bool HasTarget =>
        target != null;

    public Rigidbody2D RB =>
        rb;

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

        PlayAnimation("idle");
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
        InvokeRepeating(
        nameof(UpdatePath),
        0f,
        updateRate);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (isHurting)
            return;

        FindPlayer();

        if (movementLocked)
            return;

        if (target != null)
        {
            ChasePlayer();
        }
        else
        {
            StopMovement();
            PlayAnimation("idle");
        }
    }

    void UpdatePath()
    {
        if (movementLocked)
            return;

        if (target == null)
            return;

        if (!seeker.IsDone())
            return;

        seeker.StartPath(
            rb.position,
            target.position,
            OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (p.error)
            return;

        path = p;

        currentWaypoint = 0;
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            return;

        FollowPath();
    }

    void FollowPath()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 next =
            path.vectorPath[currentWaypoint];

        Vector2 dir =
            (next - rb.position).normalized;

        rb.linearVelocity =
            dir * moveSpeed;

        Flip(dir);

        PlayAnimation("run");

        if (Vector2.Distance(
            rb.position,
            next)
            < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    public void SetHitDirection(
    Vector2 direction
)
    {
        hitDirection =
            direction.normalized;
    }


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
            StopMovement();
    }


    public void PlayAnimation(string animName)
    {
        if (animator == null)
            return;

        if (currentAnim == "death")
            return;

        if (currentAnim == "hurt" &&
            animName != "death")
            return;

        // attack luôn cho phát lại
        if (
            currentAnim == animName &&
            animName != "attack"
        )
            return;

        currentAnim = animName;

        animator.Play(
            animName,
            0,
            0f
        );
    }

    void FindPlayer()
    {
        if (target != null)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    void ChasePlayer()
    {
        Vector2 dir =
            (
                (Vector2)target.position -
                (Vector2)transform.position
            ).normalized;

        rb.linearVelocity =
            dir * moveSpeed;

        Flip(dir);

        PlayAnimation("run");
    }

    void Flip(Vector2 dir)
    {
        if (enemyVisual == null)
            return;

        Vector3 scale =
            enemyVisual.localScale;

        if (dir.x > 0)
        {
            enemyVisual.rotation =
                Quaternion.Euler(
                    0,
                    0,
                    0
                );
        }
        else
        {
            enemyVisual.rotation =
                Quaternion.Euler(
                    0,
                    180,
                    0
                );
        }

        enemyVisual.localScale =
            scale;
    }


    public void LookAt(Vector2 targetPos)
    {
        Vector2 dir =
            targetPos -
            (Vector2)transform.position;

        Flip(dir);
    }


    //--------------------------------
    // HURT + KNOCKBACK
    //--------------------------------

    void HandleHurt()
    {
        //--------------------------------
        // Nếu đang bị hurt
        // thì reset coroutine cũ
        //--------------------------------

        if (
            hurtCoroutine != null
        )
        {
            StopCoroutine(
                hurtCoroutine
            );

            isHurting = false;
        }

        //--------------------------------
        // Chạy hurt mới
        //--------------------------------

        hurtCoroutine =
        StartCoroutine(
            HurtRoutine()
        );
    }


    IEnumerator HurtRoutine()
    {
        //--------------------------------
        // Đã chết thì bỏ
        //--------------------------------

        if (isDead)
            yield break;

        //--------------------------------
        // Khóa AI
        //--------------------------------

        isHurting = true;
        LockMovement(true);

        Vector2 dir =
            hitDirection;

        //--------------------------------
        // Phát animation hurt
        //--------------------------------

        currentAnim = "";

        PlayAnimation(
            "hurt"
        );

        //--------------------------------
        // Bật lùi NGAY LẬP TỨC
        //--------------------------------

        rb.linearVelocity =
        hitDirection * knockbackForce;


        //--------------------------------
        // Chờ thời gian hurt
        //--------------------------------

        yield return
        new WaitForSeconds(
            hurtDuration
        );

        //--------------------------------
        // Dừng knockback
        //--------------------------------

        rb.linearVelocity =
        Vector2.zero;

        //--------------------------------
        // Mở lại AI
        //--------------------------------

        LockMovement(false);
        isHurting =
        false;

        hurtCoroutine =
        null;

        currentAnim = "";

        //--------------------------------
        // Quay lại state cũ
        //--------------------------------

        if (target != null)
        {
            PlayAnimation(
                "run"
            );
        }
        else
        {
            PlayAnimation(
                "idle"
            );
        }
    }

    void HandleDeath()
    {
        isDead = true;

        rb.linearVelocity =
            Vector2.zero;

        PlayAnimation("death");
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            isAlerted
            ? Color.red
            : Color.yellow;
    }
}