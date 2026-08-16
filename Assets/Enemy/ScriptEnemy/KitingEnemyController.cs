using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KitingEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Kiting & Distance Settings")]
    [Tooltip("Khoảng cách kích hoạt né tránh khi Player lại gần")]
    public float safeDistance = 6f;

    [Tooltip("Khoảng cách tối thiểu để chuyển sang chế độ đi vòng ra sau lưng Player")]
    public float flankDistance = 8f;

    [Tooltip("Khoảng cách chọn điểm né ngẫu nhiên xung quanh")]
    public float kitingRadius = 5f;

    [Tooltip("Khoảng cách đi tuần khi không thấy Player")]
    public float wanderRadius = 4f;
    public float wanderWaitTime = 2f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Layer tường/vật cản để tránh spam điểm xuyên tường")]
    public LayerMask wallLayer;
    [Tooltip("Tag của tường/vật cản cần né tránh")]
    public string wallTag = "Wall";
    [Tooltip("Bán kính kiểm tra va chạm tại điểm sinh ngẫu nhiên")]
    public float pointCheckRadius = 0.5f;
    public int maxTryGeneratePoints = 15;

    [Header("Visual & Animation")]
    public Transform enemyVisual;
    public float hurtDuration = 0.2f;
    public float knockbackForce = 4f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHeath health;

    // References & States
    private Transform playerTransform;
    private PlayerStealth playerStealth;
    private Vector2 currentTargetPoint;

    private bool isKiting;
    private bool isWandering;
    private bool isHurting;
    private bool isDead;
    private bool movementLocked;

    private Coroutine wanderCoroutine;
    private Coroutine hurtCoroutine;
    private Vector2 hitDirection;
    private string currentAnim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHeath>();

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
    }

    private void Update()
    {
        if (isDead || isHurting) return;

        LocatePlayer();

        if (ShouldFlee())
        {
            isKiting = true;
            isWandering = false;

            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
                wanderCoroutine = null;
            }

            if (NeedsNewKitingPoint())
            {
                GenerateValidKitingPoint();
            }
        }
        else
        {
            isKiting = false;

            if (!isWandering && !movementLocked)
            {
                StartWandering();
            }
        }
    }

    private void FixedUpdate()
    {
        if (movementLocked || isDead || isHurting) return;

        MoveToTargetPoint();
    }

    #region Trigger & Collision System

    // Xử lý va chạm Trigger với Player -> Destroy
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    // Xử lý va chạm Vật lý (Physics) thông thường với Player -> Destroy
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Direct Movement Logic

    private void MoveToTargetPoint()
    {
        if (currentTargetPoint == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");
            return;
        }

        float distanceToTarget = Vector2.Distance(rb.position, currentTargetPoint);

        if (distanceToTarget > 0.2f)
        {
            Vector2 moveDirection = (currentTargetPoint - rb.position).normalized;
            rb.linearVelocity = moveDirection * moveSpeed;

            FlipVisual(moveDirection);
            PlayAnimation("run");
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnimation("idle");

            if (isWandering && wanderCoroutine == null)
            {
                wanderCoroutine = StartCoroutine(WanderWaitRoutine());
            }
        }
    }

    #endregion

    #region AI Kiting & Flanking Core Logic

    private bool ShouldFlee()
    {
        if (playerTransform == null) return false;
        if (playerStealth != null && playerStealth.IsHidden) return false;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distToPlayer <= safeDistance;
    }

    private bool NeedsNewKitingPoint()
    {
        if (currentTargetPoint == Vector2.zero) return true;

        float distToCurrentTarget = Vector2.Distance(transform.position, currentTargetPoint);
        float distPlayerToTarget = Vector2.Distance(playerTransform.position, currentTargetPoint);

        return distToCurrentTarget < 0.3f || distPlayerToTarget < safeDistance * 0.7f;
    }

    private void GenerateValidKitingPoint()
    {
        if (playerTransform == null) return;

        Vector2 playerPos = playerTransform.position;
        Vector2 enemyPos = transform.position;
        Vector2 dirAwayFromPlayer = (enemyPos - playerPos).normalized;

        for (int i = 0; i < maxTryGeneratePoints; i++)
        {
            Vector2 candidatePoint;

            if (Random.value < 0.3f && Vector2.Distance(enemyPos, playerPos) < flankDistance)
            {
                Vector2 behindPlayerDir = -playerTransform.right;
                candidatePoint = playerPos + (behindPlayerDir + Random.insideUnitCircle * 0.5f).normalized * (safeDistance + 2f);
            }
            else
            {
                float randomAngle = Random.Range(-75f, 75f);
                Vector2 spreadDir = Quaternion.Euler(0, 0, randomAngle) * dirAwayFromPlayer;
                candidatePoint = enemyPos + spreadDir * kitingRadius;
            }

            if (!IsPointAndPathClear(enemyPos, candidatePoint))
                continue;

            if (Vector2.Distance(candidatePoint, playerPos) < safeDistance * 0.8f)
                continue;

            currentTargetPoint = candidatePoint;
            return;
        }

        for (float angle = 15f; angle <= 180f; angle += 15f)
        {
            Vector2 altDir1 = Quaternion.Euler(0, 0, angle) * dirAwayFromPlayer;
            Vector2 altPoint1 = enemyPos + altDir1 * (kitingRadius * 0.5f);
            if (IsPointAndPathClear(enemyPos, altPoint1))
            {
                currentTargetPoint = altPoint1;
                return;
            }

            Vector2 altDir2 = Quaternion.Euler(0, 0, -angle) * dirAwayFromPlayer;
            Vector2 altPoint2 = enemyPos + altDir2 * (kitingRadius * 0.5f);
            if (IsPointAndPathClear(enemyPos, altPoint2))
            {
                currentTargetPoint = altPoint2;
                return;
            }
        }

        currentTargetPoint = enemyPos;
    }

    private bool IsPointAndPathClear(Vector2 startPos, Vector2 targetPos)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPos, pointCheckRadius);
        foreach (var col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;

            if (((1 << col.gameObject.layer) & wallLayer) != 0 || col.CompareTag(wallTag))
            {
                return false;
            }
        }

        Vector2 pathDir = targetPos - startPos;
        float pathDist = pathDir.magnitude;

        RaycastHit2D hit = Physics2D.CircleCast(startPos, pointCheckRadius * 0.5f, pathDir.normalized, pathDist, wallLayer);
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Wandering System

    private void StartWandering()
    {
        isWandering = true;
        PickWanderPoint();
    }

    private void PickWanderPoint()
    {
        for (int i = 0; i < maxTryGeneratePoints; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;

            if (IsPointAndPathClear(transform.position, randomPoint))
            {
                currentTargetPoint = randomPoint;
                return;
            }
        }
    }

    private IEnumerator WanderWaitRoutine()
    {
        yield return new WaitForSeconds(wanderWaitTime);
        if (isWandering && !isKiting)
        {
            PickWanderPoint();
        }
        wanderCoroutine = null;
    }

    #endregion

    #region Helpers & Player Detection

    private void LocatePlayer()
    {
        if (playerTransform != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerStealth = playerObj.GetComponent<PlayerStealth>();
        }
    }

    private void FlipVisual(Vector2 dir)
    {
        if (enemyVisual == null || Mathf.Abs(dir.x) < 0.01f) return;

        Vector3 scale = enemyVisual.localScale;
        scale.x = dir.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        enemyVisual.localScale = scale;
    }

    public void SetHitDirection(Vector2 direction)
    {
        hitDirection = direction.normalized;
    }

    public void PlayAnimation(string animName, bool forceReset = false)
    {
        if (animator == null || currentAnim == "death") return;
        if (currentAnim == animName && !forceReset) return;

        currentAnim = animName;

        if (animName == "hurt" || forceReset)
            animator.Play(animName, 0, 0f);
        else
            animator.Play(animName);
    }

    #endregion

    #region Hurt & Death

    private void HandleHurt()
    {
        if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
        hurtCoroutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        if (isDead) yield break;

        isHurting = true;
        rb.linearVelocity = hitDirection * knockbackForce;

        PlayAnimation("hurt", true);

        yield return new WaitForSeconds(hurtDuration);

        rb.linearVelocity = Vector2.zero;
        isHurting = false;
        hurtCoroutine = null;

        PlayAnimation("idle", true);
    }

    private void HandleDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        PlayAnimation("death", true);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flankDistance);

        if (currentTargetPoint != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetPoint, 0.3f);
            Gizmos.DrawLine(transform.position, currentTargetPoint);
        }
    }
}