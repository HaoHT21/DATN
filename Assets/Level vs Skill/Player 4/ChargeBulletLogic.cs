using UnityEngine;
using System.Collections.Generic;

public class ChargeBulletLogic : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ĐẠN BAY ---")]
    public float flySpeed = 12f;
    public int damage = 80;
    public LayerMask enemyLayer;

    // CHÈN THÊM 2 DÒNG NÀY ĐỂ NHẬN DỮ LIỆU ÂM THANH TỪ POISONCHARGESKILL TRUYỀN SANG
    [HideInInspector] public AudioClip hitSound;
    [HideInInspector] public float soundVolume = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool _isFlying = false;
    private bool _hasExploded = false; // Kiểm tra xem đã nổ chưa
    private Vector2 _flyDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void SetupCharge(Transform player)
    {
        if (anim != null) anim.Play("Charge_Loop");
    }

    // Khi Player thả tay ra đủ thời gian -> Chuyển sang dạng QUẢ CẦU BAY TĨNH
    public void Fire(Vector2 direction)
    {
        _isFlying = true;
        _flyDir = direction;

        transform.SetParent(null); // Bay độc lập

        if (anim != null)
        {
            // Kích hoạt Trigger để Animator chuyển từ Loop sang trạng thái quả cầu bay tĩnh (Charge_Fly)
            anim.SetTrigger("IsFlying");
        }

        if (rb != null) rb.linearVelocity = _flyDir * flySpeed;
    }

    // XỬ LÝ VA CHẠM: Khi quả cầu cắm phập vào người quái/Boss
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu đang bay và chưa nổ, đập trúng Player thì đéo xử lý
        if (!_isFlying || _hasExploded || collision.CompareTag("Player")) return;

        // BẢO VỆ CHÍ MẠNG: Check xem có đụng độ LayerEnemy, hoặc đụng trúng Tag "Boss", hoặc đụng trúng Layer mang tên "Boss" hay không
        bool isEnemyLayer = ((1 << collision.gameObject.layer) & enemyLayer) != 0;
        bool isBossTag = collision.CompareTag("Boss") || collision.CompareTag("Enemy");
        bool isBossLayer = LayerMask.LayerToName(collision.gameObject.layer) == "Boss";

        if (isEnemyLayer || isBossTag || isBossLayer)
        {
            TriggerExplosion(collision);
        }
    }

    // Hàm kích hoạt vụ nổ tại chỗ và trừ máu quái/Boss
    private void TriggerExplosion(Collider2D enemyCollider)
    {
        _hasExploded = true;

        // Dừng vật lý ngay lập tức, đứng khựng lại tại người con quái/Boss để nổ
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // CHÈN ĐOẠN NÀY: Phát âm thanh va chạm nổ tung tóe dạng 2D to rõ, đè bẹp nhạc nền
        if (hitSound != null)
        {
            GameObject tempAudio = new GameObject("TempChargeHitAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = hitSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (bỏ qua khoảng cách Camera xa gần)
            aSource.volume = soundVolume; // Ăn theo volume chung của Manager truyền qua

            aSource.Play();
            Destroy(tempAudio, hitSound.length); // Phát xong tự dọn dẹp Object tạm khỏi Hierarchy
        }

        // BÂY GIỜ MỚI CHO CHẠY ANIMATION NỔ TUNG (Từ frame 9 đến 16)
        if (anim != null)
        {
            anim.Play("Charge_Fire");
        }

        // Trừ máu con quái/Boss xấu số (Cả hai đều xài chung script EnemyHeath đúng không mày?)
        EnemyHeath enemyHP = enemyCollider.GetComponent<EnemyHeath>();
        if (enemyHP != null && !enemyHP.IsDead)
        {
            enemyHP.TakeDamage(damage);
            Debug.Log($"<color=red>[Charge Burst]</color> Quả cầu cắm phập vào {enemyCollider.name} nổ tung: -{damage} HP!");
        }

        // Chờ chạy nốt hoạt ảnh nổ (khoảng 0.5 giây) rồi mới xóa sổ hoàn toàn
        Destroy(gameObject, 0.5f);
    }
}