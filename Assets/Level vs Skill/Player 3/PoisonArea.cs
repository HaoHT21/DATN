using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoisonArea : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ĐẠN BAY ---")]
    public float flySpeed = 8f;         // Tốc độ bay của cục độc
    [HideInInspector] public Vector2 moveDirection = Vector2.right; // Hướng bay

    [Header("--- CÀI ĐẶT SÁT THƯƠNG NỔ ---")]
    public int instantDamage = 30;      // Sát thương nổ phát đầu
    public int dotDamagePerSec = 5;     // Sát thương độc rút mỗi giây
    public float poisonDuration = 5f;   // Thời gian dính độc (5 giây)
    public float splashRadius = 2.5f;   // Bán kính vụ nổ quét quái xung quanh
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
        // Tắt animation lúc mới bắn ra để giữ nguyên hình dạng tĩnh đang bay
        if (anim != null) anim.enabled = false;

        // Cho đạn bay vèo về phía trước
        if (rb != null) rb.linearVelocity = moveDirection * flySpeed;

        // Đề phòng bay hụt quái thì tự hủy sau 4 giây
        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded || collision.CompareTag("Player")) return;

        // BẢO VỆ CHÍ MẠNG: Check trúng LayerEnemy, hoặc Tag "Boss"/"Enemy", hoặc Layer mang tên "Boss" để kích nổ độc
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
        if (rb != null) rb.linearVelocity = Vector2.zero; // Dừng bay
        if (anim != null) anim.enabled = true;           // Bật hoạt ảnh nổ

        // Quét sát thương diện rộng (Bỏ enemyLayer cũ để quét trúng cả Boss)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D enemyInArea in hitEnemies)
        {
            if (enemyInArea.CompareTag("Player")) continue;

            // Kiểm tra xem mục tiêu lọt vào tầm nổ có phải kẻ địch (Quái hoặc Boss) không
            bool isEnemyLayer = ((1 << enemyInArea.gameObject.layer) & enemyLayer) != 0;
            bool isBossTag = enemyInArea.CompareTag("Boss") || enemyInArea.CompareTag("Enemy");
            bool isBossLayer = LayerMask.LayerToName(enemyInArea.gameObject.layer) == "Boss";

            if (isEnemyLayer || isBossTag || isBossLayer)
            {
                EnemyHeath enemyHP = enemyInArea.GetComponent<EnemyHeath>();
                if (enemyHP != null && !enemyHP.IsDead)
                {
                    enemyHP.TakeDamage(instantDamage);
                    Debug.Log($"<color=green>[Nổ Độc]</color> Nổ phập vào {enemyInArea.name}: -{instantDamage} HP!");
                    StartCoroutine(PoisonDotRoutine(enemyHP));
                }
            }
        }

        Destroy(gameObject, 1.5f); // Nổ xong tự hủy sạch rác Scene
    }

    private IEnumerator PoisonDotRoutine(EnemyHeath enemyHP)
    {
        float timer = 0f;
        while (timer < poisonDuration && enemyHP != null && !enemyHP.IsDead)
        {
            yield return new WaitForSeconds(1f);
            if (enemyHP != null && !enemyHP.IsDead)
            {
                enemyHP.TakeDamage(dotDamagePerSec);
                Debug.Log($"<color=darkgreen>[Độc Rút Máu]</color> {enemyHP.name} đang thấm độc: -{dotDamagePerSec} HP!");
            }
            timer += 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}