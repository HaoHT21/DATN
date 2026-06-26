using UnityEngine;
using System.Collections.Generic;

public class SplatterBlast : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ĐẠN BAY ---")]
    public float flySpeed = 9f;         // Tốc độ bay của chiêu mới
    [HideInInspector] public Vector2 moveDirection = Vector2.right;

    [Header("--- SÁT THƯƠNG ĐÒN ĐÁNH ---")]
    public int blastDamage = 50;        // Mày có thể tự chỉnh lượng dame ngoài Editor
    public float splashRadius = 0.33f;   // Bán kính vụ nổ quét quái xung quanh
    public LayerMask enemyLayer;        // Layer của Quái (Enemy)

    private Rigidbody2D rb;
    private Animator anim;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (anim != null) anim.enabled = false; // Tắt anim khi đang bay tĩnh
        if (rb != null) rb.linearVelocity = moveDirection * flySpeed;
        Destroy(gameObject, 4f); // Tránh rác map nếu bay hụt
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded || collision.CompareTag("Player")) return;

        // BẢO VỆ CHÍ MẠNG: Check trúng LayerEnemy, hoặc Tag "Boss"/"Enemy", hoặc Layer mang tên "Boss" để kích nổ
        bool isEnemyLayer = ((1 << collision.gameObject.layer) & enemyLayer) != 0;
        bool isBossTag = collision.CompareTag("Boss") || collision.CompareTag("Enemy");
        bool isBossLayer = LayerMask.LayerToName(collision.gameObject.layer) == "Boss";

        if (isEnemyLayer || isBossTag || isBossLayer)
        {
            TriggerExplosion();
        }
    }

    private void TriggerExplosion()
    {
        hasExploded = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = true; // Bung hoạt ảnh tung tóe

        // Quét sát thương (Bỏ enemyLayer cũ để quét trúng cả Boss lọt vào tầm nổ nhỏ)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D enemyInArea in hitEnemies)
        {
            if (enemyInArea.CompareTag("Player")) continue;

            // Kiểm tra xem mục tiêu lọt vào tầm nổ nhỏ có thuộc phe địch (Quái hoặc Boss) không
            bool isEnemyLayer = ((1 << enemyInArea.gameObject.layer) & enemyLayer) != 0;
            bool isBossTag = enemyInArea.CompareTag("Boss") || enemyInArea.CompareTag("Enemy");
            bool isBossLayer = LayerMask.LayerToName(enemyInArea.gameObject.layer) == "Boss";

            if (isEnemyLayer || isBossTag || isBossLayer)
            {
                EnemyHeath enemyHP = enemyInArea.GetComponent<EnemyHeath>();
                if (enemyHP != null && !enemyHP.IsDead)
                {
                    enemyHP.TakeDamage(blastDamage);
                    Debug.Log($"<color=green>[Splatter Skill]</color> Đã nổ {blastDamage} HP vào {enemyInArea.name}!");
                }
            }
        }

        Destroy(gameObject, 0.8f); // Chạy xong hiệu ứng tự hủy
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}