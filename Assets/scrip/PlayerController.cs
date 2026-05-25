using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 5f;

    [Header("Cấu hình chiến đấu")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public int damageAmount = 25;
    public float attackRate = 0.5f;

    [Header("Hệ thống vũ khí")]
    public Transform weaponHolder;
    public GameObject[] weaponSprites;

    // Đồng bộ chỉ số vũ khí qua mạng
    [Networked] public int CurrentWeaponIndex { get; set; }
    [Networked] public bool IsFacingRight { get; set; } = true;
    [Networked] public bool IsAttacking { get; set; }
    [Networked] private TickTimer attackTimer { get; set; }

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Collider2D[] _hitResults = new Collider2D[10];

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    public override void FixedUpdateNetwork()
    {
        // 1. Lấy dữ liệu Input từ LocalInputHandler gửi lên
        if (!GetInput(out PlayerInputData data)) return;

        // 2. DI CHUYỂN: Thực hiện trên toàn bộ các máy để dự đoán vị trí (Client-Side Prediction)
        // Lưu ý: Transform.position phải được đồng bộ bởi NetworkTransform component trên Unity
        Vector3 moveVec = new Vector3(data.MovementDirection.x, data.MovementDirection.y, 0).normalized;
        transform.position += moveVec * moveSpeed * Runner.DeltaTime;

        // 3. LOGIC CHỈ THỰC THI TRÊN SERVER (STATE AUTHORITY)
        if (Object.HasStateAuthority)
        {
            // Xoay hướng nhìn dựa trên hướng di chuyển
            if (data.MovementDirection.x > 0.1f) IsFacingRight = true;
            else if (data.MovementDirection.x < -0.1f) IsFacingRight = false;

            // Đổi vũ khí khi nhấn R (Logic xoay vòng: 0 -> 1 -> 2 -> 0)
            if (data.IsRPressed)
            {
                if (weaponSprites != null && weaponSprites.Length > 0)
                {
                    CurrentWeaponIndex = (CurrentWeaponIndex + 1) % (weaponSprites.Length + 1);
                }
            }

            // Xử lý đếm ngược thời gian hồi chiêu
            if (IsAttacking && attackTimer.Expired(Runner))
                IsAttacking = false;

            // Thực hiện tấn công
            if (data.IsAttackPressed && attackTimer.ExpiredOrNotRunning(Runner))
            {
                IsAttacking = true;
                attackTimer = TickTimer.CreateFromSeconds(Runner, attackRate);
                DealDamageToEnemies();
            }
        }

        // 4. Animation di chuyển (Cập nhật liên tục trên mỗi tick mạng)
        if (_animator != null)
            _animator.SetBool("isWalking", data.MovementDirection.magnitude > 0.1f);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_PickupWeapon(int index)
    {
        CurrentWeaponIndex = index;
    }

    public override void Render()
    {
        // Cập nhật hình ảnh (Visuals) mượt mà dựa trên dữ liệu mạng đã đồng bộ
        if (_sprite != null) _sprite.flipX = !IsFacingRight;
        if (_animator != null) _animator.SetBool("Attack", IsAttacking);

        // Xoay hướng vũ khí
        if (weaponHolder != null)
        {
            float targetRotation = IsFacingRight ? 0f : 180f;
            weaponHolder.localRotation = Quaternion.Euler(0, targetRotation, 0);
        }

        // Hiển thị đúng loại súng đang cầm
        if (weaponSprites != null)
        {
            for (int i = 0; i < weaponSprites.Length; i++)
            {
                if (weaponSprites[i] != null)
                    weaponSprites[i].SetActive(CurrentWeaponIndex == (i + 1));
            }
        }
    }

    void DealDamageToEnemies()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, _hitResults, enemyLayer);
        for (int i = 0; i < hitCount; i++)
        {
            if (_hitResults[i].TryGetComponent<EnemyHealth>(out var eHealth))
                eHealth.Rpc_TakeDamage(damageAmount);

            if (_hitResults[i].TryGetComponent<BossHealth>(out var bHealth))
                bHealth.Rpc_TakeDamage(damageAmount, "Player");
        }
    }
}