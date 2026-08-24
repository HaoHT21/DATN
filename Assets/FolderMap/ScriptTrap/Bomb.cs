using UnityEngine;
using System;
using System.Collections;

public class Bomb : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHp = 1;
    private int currentHp;

    [Header("Explosion")]
    public float radius = 3f;
    public int damage = 50;
    public float knockbackForce = 10f; // Lực đẩy lùi kẻ địch và người chơi

    [Header("Respawn Settings")]
    public float respawnDelay = 10f; // Thời gian hồi phục bom (10 giây)

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D mainCollider; // Collider chính của quả bom
    private bool exploded = false;

    // Lưu lại Tag ban đầu của GameObject
    private string originalTag;

    // Các Event thông báo trạng thái
    public event Action OnExplode;
    public event Action OnRespawn;

    // Getter cho HP hiện tại nếu script khác cần đọc
    public int CurrentHp => currentHp;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<Collider2D>();

        currentHp = maxHp;

        // Lưu lại Tag ban đầu cài đặt trong Unity Inspector (VD: "Bomb", "Breakable",...)
        originalTag = gameObject.tag;
    }

    public void TakeDamage(int damageAmount)
    {
        if (exploded) return;

        currentHp -= damageAmount;

        if (currentHp <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (exploded) return;

        exploded = true;

        // Phát sự kiện nổ cho BombAudio biết
        OnExplode?.Invoke();

        if (animator != null)
            animator.Play("Boom");

        DamageAndKnockbackNearby();

        // Bắt đầu chuỗi ẩn bom và hồi phục lại sau 10 giây
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        // Chờ animation nổ diễn ra (0.5s)
        yield return new WaitForSeconds(0.5f);

        // 1. Ẩn quả bom, tắt collider và xóa Tag
        SetBombActive(false);

        // 2. Chờ 10 giây (respawnDelay)
        yield return new WaitForSeconds(respawnDelay);

        // 3. Hồi sinh quả bom trở lại (Khôi phục Tag)
        ResetBomb();
    }

    private void SetBombActive(bool active)
    {
        // Ẩn/hiện hình ảnh
        if (spriteRenderer != null)
            spriteRenderer.enabled = active;

        // Bật/tắt collider chính
        if (mainCollider != null)
            mainCollider.enabled = active;

        // Bật/Tắt Tag: Khi ẩn đổi thành "Untagged", khi hiện gán lại Tag ban đầu
        gameObject.tag = active ? originalTag : "Untagged";
    }

    private void ResetBomb()
    {
        currentHp = maxHp;
        exploded = false;

        // Trở về animation mặc định (Idle)
        if (animator != null)
            animator.Play("Idle");

        SetBombActive(true);

        // Phát sự kiện hồi sinh
        OnRespawn?.Invoke();
    }

    private void DamageAndKnockbackNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Tính hướng đẩy lùi từ tâm quả bom ra phía đối tượng
            Vector2 pushDirection = (hit.transform.position - transform.position).normalized;

            // Nếu vị trí quá gần tâm (trùng khớp), mặc định đẩy lên trên
            if (pushDirection == Vector2.zero) pushDirection = Vector2.up;

            // 1. Gây sát thương chung cho các object có IDamageable (vật thể phá hủy được,...)
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
            }

            // 2. Damage & Knockback cho Enemy
            EnemyHeath enemy = hit.GetComponent<EnemyHeath>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, true);
            }

            // 3. Damage & Knockback cho Player
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, pushDirection);
            }

            // 4. Áp dụng lực đẩy vật lý (Rigidbody2D)
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            // Thay thế isKinematic bằng bodyType != RigidbodyType2D.Kinematic
            if (rb != null && rb.bodyType != RigidbodyType2D.Kinematic)
            {
                // Thay thế velocity cũ bằng linearVelocity
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}