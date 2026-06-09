using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BossEnemyAI : MonoBehaviour
{
    [Header("Di chuyển")]
    public float moveSpeed = 3f;

    [Header("Phát hiện & Tấn công")]
    public float detectionRange = 10f;
    public float attackRange = 2.5f;

    [Header("Chọn đòn đánh")]
    [Range(0f, 1f)]
    [Tooltip("Xác suất chọn Attack 1. Attack 2 = 1 - giá trị này.")]
    public float attack1Weight = 0.5f;

    [Header("Attack Recovery")]
    public float attackRecoveryTime = 0.8f;
    public bool facePlayerDuringRecovery = true;

    [Header("Tên Animation (phải khớp Animator State)")]
    public string idleAnimation = "Idle";
    public string chaseAnimation = "Run";
    public string attack1Animation = "Attack1";
    public string attack2Animation = "Attack2";
    public string hurtAnimation = "Take hit";
    public string deathAnimation = "Death";

    [Header("Hitbox (tùy chọn — bật/tắt qua Animation Event)")]
    public GameObject attack1Hitbox;
    public GameObject attack2Hitbox;

    [Header("Hitbox Facing")]
    [Tooltip("Pivot flip hitbox theo trục X. Để trống sẽ tự tạo tại gốc Boss.")]
    public Transform hitboxPivot;

    [Header("Drop khi chết")]
    public GameObject coinPrefab;
    public int coinDropCount = 15;
    public float destroyDelay = 2f;

    [Header("Visual (tùy chọn — xoay Y thay vì flipX)")]
    public Transform bossVisual;

    public BossEnemyStateMachine StateMachine { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public SpriteRenderer Sprite { get; private set; }

    public bool IsDead => StateMachine != null && StateMachine.CurrentStateId == BossEnemyState.Death;

    private BossEnemyContext _context;
    private Transform _hitboxPivot;
    private bool _isFacingLeft;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>();

        Rigidbody.gravityScale = 0f;
        Rigidbody.freezeRotation = true;

        SetupHitboxFacing();

        _context = new BossEnemyContext(this);
        StateMachine = new BossEnemyStateMachine();
        StateMachine.Initialize(
            _context,
            new BossIdleState(),
            new BossChaseState(),
            new BossAttack1State(),
            new BossAttack2State(),
            new BossAttackRecoveryState(),
            new BossHurtState(),
            new BossDeathState()
        );
    }

    private void Start()
    {
        if (attack1Hitbox != null) attack1Hitbox.SetActive(false);
        if (attack2Hitbox != null) attack2Hitbox.SetActive(false);

        StateMachine.ChangeState(BossEnemyState.Idle);
    }

    private void Update()
    {
        if (IsDead) return;

        FindClosestPlayer();
        StateMachine.Update();
    }

    public void RequestHurt()
    {
        if (IsDead) return;

        BossEnemyState current = StateMachine.CurrentStateId;
        if (current == BossEnemyState.Attack1 ||
            current == BossEnemyState.Attack2 ||
            current == BossEnemyState.Hurt ||
            current == BossEnemyState.Death)
            return;

        StateMachine.ChangeState(BossEnemyState.Hurt);
    }

    public void EnterDeath()
    {
        if (IsDead) return;
        StateMachine.ChangeState(BossEnemyState.Death);
        enabled = false;
    }

    public void HandleDeathRewards()
    {
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinDropCount; i++)
            {
                Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 1f);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }

        SendMessage("OnBossDeath", SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>Gọi từ Animation Event khi attack animation kết thúc.</summary>
    public void OnAttackAnimationFinished()
    {
        _context.AttackAnimationFinished = true;
    }

    /// <summary>Gọi từ Animation Event khi hurt animation kết thúc.</summary>
    public void OnHurtAnimationFinished()
    {
        _context.HurtAnimationFinished = true;
    }

    /// <summary>
    /// Flip hitbox qua HitboxPivot.localScale.x — không đổi localPosition, tránh lệch khi bossVisual xoay Y.
    /// </summary>
    public void UpdateHitboxFacing(bool facingLeft)
    {
        if (_hitboxPivot == null || _isFacingLeft == facingLeft)
            return;

        _isFacingLeft = facingLeft;

        Vector3 scale = _hitboxPivot.localScale;
        scale.x = facingLeft ? -1f : 1f;
        _hitboxPivot.localScale = scale;
    }

    private void SetupHitboxFacing()
    {
        _hitboxPivot = hitboxPivot;

        if (_hitboxPivot == null)
        {
            Transform existing = transform.Find("HitboxPivot");
            _hitboxPivot = existing != null
                ? existing
                : CreateHitboxPivot().transform;
        }

        _hitboxPivot.localScale = Vector3.one;

        AttachHitboxToPivot(attack1Hitbox);
        AttachHitboxToPivot(attack2Hitbox);
        NormalizeHitboxLocalX(attack1Hitbox);
        NormalizeHitboxLocalX(attack2Hitbox);
    }

    private GameObject CreateHitboxPivot()
    {
        var pivotObject = new GameObject("HitboxPivot");
        Transform pivotTransform = pivotObject.transform;
        pivotTransform.SetParent(transform, false);
        pivotTransform.localPosition = Vector3.zero;
        pivotTransform.localRotation = Quaternion.identity;
        pivotTransform.localScale = Vector3.one;
        return pivotObject;
    }

    private void AttachHitboxToPivot(GameObject hitbox)
    {
        if (hitbox == null || _hitboxPivot == null)
            return;

        if (hitbox.transform.parent == _hitboxPivot)
            return;

        hitbox.transform.SetParent(_hitboxPivot, true);
    }

    /// <summary>Chuẩn hóa offset hitbox về phía phải (X dương) — pivot scale sẽ lo phần flip.</summary>
    private static void NormalizeHitboxLocalX(GameObject hitbox)
    {
        if (hitbox == null)
            return;

        Vector3 localPos = hitbox.transform.localPosition;
        localPos.x = Mathf.Abs(localPos.x);
        hitbox.transform.localPosition = localPos;
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }

        if (closest != null && minDistance <= detectionRange)
            _context.PlayerTarget = closest;
        else
            _context.PlayerTarget = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
