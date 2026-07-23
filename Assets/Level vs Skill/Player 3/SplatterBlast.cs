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

    // CHÈN THÊM 2 DÒNG NÀY ĐỂ NHẬN DỮ LIỆU ÂM THANH TỪ PLAYER SKILL MANAGER TRUYỀN SANG
    [HideInInspector] public AudioClip hitSound;
    [HideInInspector] public float soundVolume = 1f;

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

        // CHÈN ĐOẠN NÀY: Phát âm thanh va chạm nổ tung tóe dạng 2D to rõ, đè bẹp nhạc nền
        if (hitSound != null)
        {
            GameObject tempAudio = new GameObject("TempSplatterHitAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = hitSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (bỏ qua khoảng cách Camera xa gần)
            aSource.volume = soundVolume; // Ăn theo volume chung của Manager truyền qua

            // =========================================================================
            // CHÈN THÊM VÀO ĐÂY: Ép âm thanh nổ trúng quái của chiêu I đi qua đúng kênh CombatSFX
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // =========================================================================

            aSource.Play();
            Destroy(tempAudio, hitSound.length); // Phát xong tự dọn dẹp Object tạm khỏi Hierarchy
        }

        // Quét sát thương (Bỏ enemyLayer cũ để quét trúng cả Boss lọt vào tầm nổ nhỏ)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D enemyInArea in hitEnemies)
        {
            if (enemyInArea.CompareTag("Player")) continue;

            // Kiểm tra xem mục tiêu lọt vào tầm nổ nhỏ có thuộc phe địch (Quái hoặc Boss) không
            bool isEnemyLayerInArea = ((1 << enemyInArea.gameObject.layer) & enemyLayer) != 0;
            bool isBossTagInArea = enemyInArea.CompareTag("Boss") || enemyInArea.CompareTag("Enemy");
            bool isBossLayerInArea = LayerMask.LayerToName(enemyInArea.gameObject.layer) == "Boss";

            if (isEnemyLayerInArea || isBossTagInArea || isBossLayerInArea)
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