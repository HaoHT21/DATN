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

    [Header("Respawn Settings")]
    public float respawnDelay = 10f; // Thời gian hồi phục bom (10 giây)

    [Header("Collider sẽ bật khi nổ")]
    public Collider2D col;

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

        // Collider khu vực nổ mặc định tắt
        if (col != null)
            col.enabled = false;
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

        if (col != null)
            col.enabled = true;

        if (animator != null)
            animator.Play("Boom");

        DamageNearby();

        // Bắt đầu chuỗi ẩn bom và hồi phục lại sau 10 giây
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        // Chờ animation nổ diễn ra (0.5s)
        yield return new WaitForSeconds(0.5f);

        // 1. Tắt khu vực nổ
        if (col != null)
            col.enabled = false;

        // 2. Ẩn quả bom, tắt collider và xóa Tag
        SetBombActive(false);

        // 3. Chờ 10 giây (respawnDelay)
        yield return new WaitForSeconds(respawnDelay);

        // 4. Hồi sinh quả bom trở lại (Khôi phục Tag)
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

    private void DamageNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            // Các vật thể gần đó nhận damage nếu có IDamageable (Trừ chính quả bom này)
            if (hit.gameObject != gameObject && hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
            }

            // Damage Enemy
            EnemyHeath enemy = hit.GetComponent<EnemyHeath>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, true);
            }

            // Damage Player
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Vector2 hitDirection = (player.transform.position - transform.position).normalized;
                player.TakeDamage(damage, hitDirection);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}