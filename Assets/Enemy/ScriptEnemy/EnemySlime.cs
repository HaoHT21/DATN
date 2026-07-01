using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemySlime : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 6f;

    [Header("Wander")]
    public float wanderRadius = 3f;
    public float wanderInterval = 2f;

    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackWindupTime = 0.3f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;

    [Header("Vision")]
    public LayerMask wallLayer;

    [Header("Hitbox")]
    public GameObject attackHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isPreparingAttack;
    private bool isAttacking;

    private float attackTimer;
    private Vector2 lockedAttackPosition;
    private Vector2 dashDirection;

    private Vector2 lastKnownPlayerPosition;

    private Transform target;

    private Vector2 wanderTarget;   
    private float wanderTimer;

    [Header("Search")]
    public float maxSearchTime = 2f;

    private float searchTimer;
    private bool isWaiting;
    private bool searchingPlayer;

    private bool isDead;
    private bool isHurt;

    private EnemyHeath health;

    private string currentAnim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        wanderTimer = wanderInterval;
        PickNewWanderPoint();

        health = GetComponent<EnemyHeath>();

        if (health != null)
        {
            health.OnHurt += HandleHurt;
            health.OnHurt += AlertNearestPlayer;
            health.OnDeath += HandleDeath;
        }

    }

    private void PlayAnimation(string animName)
    {
        if (currentAnim == animName)
            return;

        currentAnim = animName;
        animator.Play(animName);
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

        if (isHurt)
            return;

        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FindPlayer();

        if (target == null)
        {
            if (searchingPlayer)
            {
                SearchLastPosition();
                return;
            }

            Wander();
            return;
        }

        //------------------------------------------------
        // WINDUP
        //------------------------------------------------
        if (isPreparingAttack)
        {
            attackTimer -= Time.deltaTime;

            rb.linearVelocity = Vector2.zero;

            if (attackTimer <= 0)
            {
                isPreparingAttack = false;

                dashDirection =
                    (lockedAttackPosition -
                     (Vector2)transform.position).normalized;

                isAttacking = true;
                attackTimer = dashDuration;
            }

            return;
        }

        //------------------------------------------------
        // DASH ATTACK
        //------------------------------------------------
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            rb.linearVelocity =
                dashDirection * dashSpeed;

            if (attackHitbox != null)
                attackHitbox.SetActive(true);

            if (attackTimer <= 0)
            {
                isAttacking = false;

                rb.linearVelocity = Vector2.zero;

                if (attackHitbox != null)
                    attackHitbox.SetActive(false);

                if (!isHurt)
                    PlayAnimation("run");
            }

            return;
        }

        //------------------------------------------------
        // CHECK ATTACK RANGE
        //------------------------------------------------
        float distance =
            Vector2.Distance(
                transform.position,
                target.position);

        if (distance <= attackRange)
        {
            isPreparingAttack = true;

            attackTimer = attackWindupTime;

            lockedAttackPosition =
                target.position;

            PlayAnimation("attack");

            return;
        }

        //------------------------------------------------
        // CHASE
        //------------------------------------------------
        Vector2 direction =
            ((Vector2)target.position -
             (Vector2)transform.position).normalized;

        rb.linearVelocity =
            direction * moveSpeed;

        if (!isHurt)
            PlayAnimation("idle");

        if (direction.x > 0.05f)
            spriteRenderer.flipX = false;
        else if (direction.x < -0.05f)
            spriteRenderer.flipX = true;
    }

    private bool CanSeePlayer(Transform player)
    {
        Vector2 origin = transform.position;
        Vector2 targetPos = player.position;

        Vector2 direction =
            (targetPos - origin).normalized;

        float distance =
            Vector2.Distance(origin, targetPos);
        
        Debug.DrawRay(
        transform.position,
        direction * distance,
        Color.red);

        RaycastHit2D hit =
            Physics2D.Raycast(
                origin,
                direction,
                distance,
                wallLayer);

        return hit.collider == null;
    }

    private void FindPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = detectRange;

        bool foundPlayer = false;

        foreach (GameObject player in players)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.transform.position);

            if (distance <= closestDistance &&
                CanSeePlayer(player.transform))
            {
                foundPlayer = true;

                closestDistance = distance;

                target = player.transform;

                lastKnownPlayerPosition =
                    player.transform.position;
            }
        }

        if (!foundPlayer)
        {
            if (target != null)
            {
                searchingPlayer = true;
                searchTimer = maxSearchTime;
            }

            target = null;
        }
        else
        {
            searchingPlayer = false;
        }
    }

    private void PickNewWanderPoint()
    {
        Vector2 randomDirection =
            Random.insideUnitCircle * wanderRadius;

        wanderTarget =
            (Vector2)transform.position + randomDirection;
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;

        float distance =
            Vector2.Distance(
                transform.position,
                wanderTarget);

        // Đã tới điểm wander
        if (distance < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;

            if (!isWaiting)
            {
                isWaiting = true;
                wanderTimer = wanderInterval;
            }

            if (wanderTimer <= 0)
            {
                isWaiting = false;
                PickNewWanderPoint();
            }

            return;
        }

        if (wanderTimer <= 0)
        {
            wanderTimer = wanderInterval;
            PickNewWanderPoint();
        }

        Vector2 direction =
            (wanderTarget -
             (Vector2)transform.position).normalized;

        rb.linearVelocity =
            direction * (moveSpeed * 0.5f);

        if (!isHurt)
            PlayAnimation("run");

        if (direction.x > 0.05f)
            spriteRenderer.flipX = false;
        else if (direction.x < -0.05f)
            spriteRenderer.flipX = true;
    }

    private void SearchLastPosition()
    {
        searchTimer -= Time.deltaTime;

        Vector2 direction =
            (lastKnownPlayerPosition -
             (Vector2)transform.position).normalized;

        rb.linearVelocity =
            direction * moveSpeed;

        // Đã tới nơi
        if (Vector2.Distance(
                transform.position,
                lastKnownPlayerPosition) < 0.2f)
        {
            searchingPlayer = false;
            rb.linearVelocity = Vector2.zero;

            PickNewWanderPoint();

            return;
        }

        // Hết thời gian tìm kiếm
        if (searchTimer <= 0)
        {
            searchingPlayer = false;

            rb.linearVelocity = Vector2.zero;

            PickNewWanderPoint();
        }
    }

    private void AlertNearestPlayer()
    {
        detectRange = 20f;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        target = player.transform;
        lastKnownPlayerPosition = player.transform.position;
        searchingPlayer = false;
    }

    private Coroutine hurtCoroutine;

    void HandleHurt()
    {
        if (isDead)
            return;

        if (hurtCoroutine != null)
            StopCoroutine(hurtCoroutine);

        hurtCoroutine =
            StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        isHurt = true;

        rb.linearVelocity = Vector2.zero;

        PlayAnimation("hurt");

        yield return new WaitForSeconds(0.2f);

        isHurt = false;

        hurtCoroutine = null;
    }

    void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;

        isAttacking = false;
        isPreparingAttack = false;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        PlayAnimation("death");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}