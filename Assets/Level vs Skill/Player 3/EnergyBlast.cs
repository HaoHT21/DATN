using UnityEngine;
using System.Collections.Generic;

public class EnergyBlast : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ĐẠN BAY ---")]
    public float flySpeed = 10f;        // Tốc độ bay (chiêu này cho bay nhanh tí cho phê)
    [HideInInspector] public Vector2 moveDirection = Vector2.right; // Hướng bay

    [Header("--- SÁT THƯƠNG ĐÒN ĐÁNH ---")]
    public int blastDamage = 70;        // Gây 70 dame lớn như Trung yêu cầu
    public float splashRadius = 2.5f;   // Bán kính vụ nổ quét quái xung quanh
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
        // Tắt animation lúc mới bắn ra để giữ nguyên hình dạng tĩnh đang bay
        if (anim != null) anim.enabled = false;

        // Cho cục năng lượng bay vèo về phía trước
        if (rb != null) rb.linearVelocity = moveDirection * flySpeed;

        // Đề phòng bay hụt quái thì tự hủy sau 4 giây để tránh rác Scene
        Destroy(gameObject, 4f);
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

        if (rb != null) rb.linearVelocity = Vector2.zero; // Dừng bay lập tức
        if (anim != null) anim.enabled = true;           // Bật hoạt ảnh vòng xoáy bung ra

        // CHÈN ĐOẠN NÀY: Phát âm thanh vụ nổ 2D to rõ đúng lúc chạm trúng đích kẻ địch
        if (hitSound != null)
        {
            GameObject tempAudio = new GameObject("TempEnergyHitAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = hitSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (bỏ qua khoảng cách Camera)
            aSource.volume = soundVolume; // Ăn theo volume chung của Manager truyền qua

            // =========================================================================
            // SỬA LỖI CHÍ MẠNG TẠI ĐÂY: Ép âm thanh nổ trúng quái đi qua đúng kênh CombatSFX
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // =========================================================================

            aSource.Play();
            Destroy(tempAudio, hitSound.length); // Chạy xong tự hủy object tạm
        }

        // Quét sát thương diện rộng (AoE) xung quanh vị trí nổ
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D enemyInArea in hitEnemies)
        {
            if (enemyInArea.CompareTag("Player")) continue;

            // Kiểm tra xem mục tiêu lọt vào tầm nổ có thuộc phe địch (Quái hoặc Boss) không
            bool isEnemyLayerInArea = ((1 << enemyInArea.gameObject.layer) & enemyLayer) != 0;
            bool isBossTagInArea = enemyInArea.CompareTag("Boss") || enemyInArea.CompareTag("Enemy");
            bool isBossLayerInArea = LayerMask.LayerToName(enemyInArea.gameObject.layer) == "Boss";

            if (isEnemyLayerInArea || isBossTagInArea || isBossLayerInArea)
            {
                // Gọi thẳng vào file quản lý máu quái/Boss để vả dame
                EnemyHeath enemyHP = enemyInArea.GetComponent<EnemyHeath>();
                if (enemyHP != null && !enemyHP.IsDead)
                {
                    enemyHP.TakeDamage(blastDamage); // Giật một phát đúng 70 dame lớn, ko rỉ máu thêm!
                    Debug.Log($"<color=lime>[Đại Bác Năng Lượng]</color> Đã nã {blastDamage} HP vào đầu {enemyInArea.name}!");
                }
            }
        }

        // Đợi chạy hết hoạt ảnh vòng xoáy bung ra thì tự xóa sổ khỏi Scene
        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}