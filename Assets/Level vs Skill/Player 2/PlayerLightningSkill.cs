using UnityEngine;

public class PlayerLightningSkill : MonoBehaviour
{
    [Header("Cấu hình Skill Sét (Phím I)")]
    public GameObject lightningPrefab;
    public float skillRadius = 4f;
    public float cooldown = 1.5f;
    public int damage = 30;             // Sát thương tia sét (30 máu)

    [Header("--- CẤU HÌNH ÂM THANH CHIÊU THỨC ---")]
    public AudioClip lightningSound;     // Ô kéo thả file nhạc sấm sét (.mp3, .wav)
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

    [Header("Bộ lọc Layer")]
    public LayerMask enemyLayer;

    private float _cooldownTimer;
    private PlayerHealth _playerHealth; // Cầu nối để check trạng thái sống chết

    void Awake()
    {
        // Tự động lấy component PlayerHealth trên người Player
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // TỐI ƯU: Nếu Player đã chết thì chặn đéo cho chạy logic hay bấm nút I nữa
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Đếm ngược hồi chiêu
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.I) && _cooldownTimer <= 0)
        {
            CastLightningSkill();
        }
    }

    void CastLightningSkill()
    {
        // ĐÃ SỬA: Bỏ lọc enemyLayer ở đây để quét trúng cả Boss mang layer riêng biệt
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, skillRadius);

        Collider2D closestEnemy = null;
        float minDistance = float.MaxValue;

        // Vòng lặp lọc ra xem trong đống thực thể vừa quét được, ai là Kẻ Địch (Quái hoặc Boss)
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Player")) continue;

            // Kiểm tra đa tầng xem mục tiêu có thuộc nhóm quái thường hoặc Boss không
            bool isEnemyLayer = ((1 << collider.gameObject.layer) & enemyLayer) != 0;
            bool isBossTag = collider.CompareTag("Boss") || collider.CompareTag("Enemy");
            bool isBossLayer = LayerMask.LayerToName(collider.gameObject.layer) == "Boss";

            if (isEnemyLayer || isBossTag || isBossLayer)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = collider;
                }
            }
        }

        // NÂNG CẤP UI: Nếu không tìm thấy bất kỳ quái hay Boss nào trong tầm đánh
        if (closestEnemy == null)
        {
            if (SkillNotification.Instance != null)
            {
                SkillNotification.Instance.ShowMessage("KHÔNG CÓ ĐỊCH TRONG PHẠM VI SÉT GIẬT!", Color.yellow);
            }

            Debug.Log("Không có quái hay Boss nào trong phạm vi dùng chiêu Sét!");
            return; // Thoát sớm, đéo trừ hồi chiêu
        }

        // Kích hoạt hồi chiêu ngay khi chắc chắn có mục tiêu hợp lệ
        _cooldownTimer = cooldown;

        // CHỈ SỬA KHÚC NÀY: Khởi tạo AudioSource 2D thủ công để tiếng sấm sét nổ to rõ, đè bẹp nhạc nền
        if (lightningSound != null)
        {
            GameObject tempAudio = new GameObject("TempLightningAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = lightningSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (nghe rõ bất kể camera xa gần)
            aSource.volume = Mathf.Clamp01(skillVolume / 100f); // Quy đổi từ hệ 100 về hệ 1.0 chuẩn Unity

            // ==========================================
            // LONG MẠCH: Gán âm thanh giật sét đi qua đúng kênh CombatSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            aSource.Play();
            Destroy(tempAudio, lightningSound.length); // Chạy xong tự dọn dẹp Object tạm khỏi Hierarchy
        }

        // Tiến hành gọi sét đánh thẳng đầu mục tiêu gần nhất tìm được
        Vector3 spawnPosition = closestEnemy.transform.position + new Vector3(0, 0.5f, 0);
        Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);

        // Gọi thẳng vào EnemyHeath để trừ máu quái/Boss
        if (closestEnemy.TryGetComponent<EnemyHeath>(out var enemyHP))
        {
            if (!enemyHP.IsDead)
            {
                enemyHP.TakeDamage(damage);
                Debug.Log($"<color=cyan>[Skill Sét]</color> Đã giật {damage} HP vào đầu {closestEnemy.name}!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
}