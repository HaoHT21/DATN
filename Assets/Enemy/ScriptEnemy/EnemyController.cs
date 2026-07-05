using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 6f;

    [Header("Search")]
    public float maxSearchTime = 2f;

    [Header("Vision")]
    public LayerMask wallLayer;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Wander")]
    public float wanderRadius = 3f;
    public float wanderInterval = 2f;

    [Header("Alert")]
    public float alertedRange = 20f;

    [Header("Animation")]
    public float hurtDuration = 0.2f;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    private Vector2 lastHitPosition;

    [Header("Visual")]
    public Transform enemyVisual;


    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHeath health;

    private Transform target;
    private Vector2 lastKnownPlayerPosition;
    private Vector2 wanderTarget;

    private bool searchingPlayer;
    private bool isWaiting;
    private bool isHurting;
    private bool isDead;
    private bool isAlerted;
    private bool movementLocked;

    private float originalDetectRange;
    private float searchTimer;
    private float wanderTimer;

    private string currentAnim;

    private Coroutine hurtCoroutine;

    public Transform Target => target;

    public bool HasTarget =>
        target != null;

    public Rigidbody2D RB =>
        rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHeath>();

        originalDetectRange = detectRange;

        if (health != null)
        {
            health.OnHurt += HandleHurt;
            health.OnHurt += AlertNearestPlayer;
            health.OnDeath += HandleDeath;
        }

        wanderTimer = wanderInterval;

        PickNewWanderPoint();

        PlayAnimation("idle");
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHurt -= HandleHurt;
            health.OnHurt -= AlertNearestPlayer;
            health.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (isHurting)
            return;

        FindPlayer();

        // Bị khóa thì không chạy AI di chuyển
        if (movementLocked)
            return;

        if (target != null)
        {
            ChasePlayer();
            return;
        }

        if (searchingPlayer)
        {
            SearchLastPosition();
            return;
        }

        Wander();
    }

    public void SetHitPosition(
    Vector2 hitPosition)
    {
        lastHitPosition =
            hitPosition;
    }


    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
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

    bool CanSeePlayer(Transform player)
    {
        Vector2 origin =
            transform.position;

        Vector2 targetPos =
            player.position;

        Vector2 direction =
            (targetPos - origin)
            .normalized;

        float distance =
            Vector2.Distance(
                origin,
                targetPos
            );

        RaycastHit2D hit =
            Physics2D.Raycast(
                origin,
                direction,
                distance,
                wallLayer
            );

        return hit.collider == null;
    }


    void FindPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag(
                "Player"
            );

        float closest =
            detectRange;

        Transform foundTarget =
            null;

        foreach (GameObject player in players)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.transform.position
                );

            if (
                distance <= closest &&
                CanSeePlayer(player.transform)
            )
            {
                closest =
                    distance;

                foundTarget =
                    player.transform;
            }
        }

        if (foundTarget != null)
        {
            target =
                foundTarget;

            lastKnownPlayerPosition =
                target.position;

            searchingPlayer =
                false;
        }
        else
        {
            if (target != null)
            {
                searchingPlayer =
                    true;

                searchTimer =
                    maxSearchTime;
            }

            target = null;
        }
    }


    void AlertNearestPlayer()
    {
        isAlerted = true;

        detectRange =
            alertedRange;

        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player == null)
            return;

        target =
            player.transform;

        lastKnownPlayerPosition =
            player.transform.position;

        searchingPlayer =
            false;
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


    void SearchLastPosition()
    {
        searchTimer -=
            Time.deltaTime;

        Vector2 dir =
            (
                lastKnownPlayerPosition -
                (Vector2)transform.position
            ).normalized;

        rb.linearVelocity =
            dir * moveSpeed;

        Flip(dir);

        PlayAnimation("run");

        if (
            Vector2.Distance(
                transform.position,
                lastKnownPlayerPosition
            ) < 0.2f
            ||
            searchTimer <= 0
        )
        {
            searchingPlayer =
                false;

            isAlerted =
                false;

            detectRange =
                originalDetectRange;

            rb.linearVelocity =
                Vector2.zero;

            PlayAnimation("idle");

            PickNewWanderPoint();
        }
    }


    void Wander()
    {
        float distance =
            Vector2.Distance(
                transform.position,
                wanderTarget
            );

        if (distance < 0.1f)
        {
            rb.linearVelocity =
                Vector2.zero;

            PlayAnimation("idle");

            if (!isWaiting)
            {
                isWaiting =
                    true;

                wanderTimer =
                    wanderInterval;
            }

            wanderTimer -=
                Time.deltaTime;

            if (wanderTimer <= 0)
            {
                isWaiting =
                    false;

                PickNewWanderPoint();
            }

            return;
        }

        Vector2 dir =
            (
                wanderTarget -
                (Vector2)transform.position
            ).normalized;

        rb.linearVelocity =
            dir * moveSpeed * 0.5f;

        Flip(dir);

        PlayAnimation("run");
    }


    void PickNewWanderPoint()
    {
        const int maxTry =
            10;

        for (
            int i = 0;
            i < maxTry;
            i++
        )
        {
            Vector2 random =
                Random.insideUnitCircle *
                wanderRadius;

            Vector2 point =
                (Vector2)
                transform.position +
                random;

            //--------------------------------
            // Kiểm tra có vật cản
            //--------------------------------

            Vector2 dir =
                (
                    point -
                    (Vector2)
                    transform.position
                ).normalized;

            float distance =
                Vector2.Distance(
                    transform.position,
                    point
                );

            RaycastHit2D hit =
                Physics2D.Raycast(
                    transform.position,
                    dir,
                    distance,
                    wallLayer
                );

            //--------------------------------
            // Không có tường
            //--------------------------------

            if (
                hit.collider ==
                null
            )
            {
                wanderTarget =
                    point;

                return;
            }
        }

        //--------------------------------
        // fallback
        //--------------------------------

        wanderTarget =
            transform.position;
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

        //--------------------------------
        // Tính hướng bật lùi
        //--------------------------------

        Vector2 dir =
        (
            (Vector2)
            transform.position -
            lastHitPosition
        ).normalized;

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
        dir *
        knockbackForce;

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

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            wanderRadius
        );
    }
}