using UnityEngine;

public class DeathBulletLogic : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ĐẠN BAY ---")]
    public float flySpeed = 11f;
    public int damage = 90; // Sát thương cực lớn
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool _isFlying = false;
    private bool _hasExploded = false;
    private Vector2 _flyDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void SetupCharge(Transform player)
    {
        if (anim != null) anim.Play("Death_Loop");
    }

    public void Fire(Vector2 direction)
    {
        _isFlying = true;
        _flyDir = direction;

        transform.SetParent(null); // Cắt dây rốn để bay tự do

        if (anim != null)
        {
            anim.SetTrigger("IsFlying"); // Kích hoạt chuyển sang ảnh bay tĩnh số 30
        }

        if (rb != null) rb.linearVelocity = _flyDir * flySpeed;

        Destroy(gameObject, 4f); // Bay hụt tự hủy sau 4s
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isFlying || _hasExploded || collision.CompareTag("Player")) return;

        // BẢO VỆ CHÍ MẠNG: Check trúng LayerEnemy, hoặc Tag "Boss"/"Enemy", hoặc Layer mang tên "Boss"
        bool isEnemyLayer = ((1 << collision.gameObject.layer) & enemyLayer) != 0;
        bool isBossTag = collision.CompareTag("Boss") || collision.CompareTag("Enemy");
        bool isBossLayer = LayerMask.LayerToName(collision.gameObject.layer) == "Boss";

        if (isEnemyLayer || isBossTag || isBossLayer)
        {
            TriggerExplosion(collision);
        }
    }

    private void TriggerExplosion(Collider2D enemyCollider)
    {
        _hasExploded = true;
        if (rb != null) rb.linearVelocity = Vector2.zero; // Dừng lại tại người quái/Boss để nổ

        if (anim != null) anim.Play("Death_Fire"); // Chạy dải ảnh nổ đầu lâu bốc lửa (31 - 49)

        EnemyHeath enemyHP = enemyCollider.GetComponent<EnemyHeath>();
        if (enemyHP != null && !enemyHP.IsDead)
        {
            enemyHP.TakeDamage(damage);
            Debug.Log($"<color=purple>[Đại Bác Diệt Vong]</color> Đã dập nát {enemyCollider.name} với {damage} HP!");
        }

        Destroy(gameObject, 0.6f); // Nổ xong tự hủy sạch sẽ
    }
}