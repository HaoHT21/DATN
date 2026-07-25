using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerBlueLightningSkill : MonoBehaviour
{
    [Header("--- CẤU HÌNH SKILL SÉT XANH (PHÍM M) ---")]
    public KeyCode skillKey = KeyCode.M; // Phím M mặc định
    public GameObject lightningPrefab;  // Kéo Prefab hiệu ứng scifi_warp_003 vào đây
    public float skillRadius = 5f;       // Phạm vi tự động quét quái (bán kính vòng tròn)
    public float cooldown = 2f;          // Thời gian hồi chiêu
    public int damage = 70;              // Sát thương lớn: 70 dame theo yêu cầu

    [Header("--- CẤU HÌNH ÂM THANH CHIÊU THỨC ---")]
    public AudioClip blueLightningSound; // Ô kéo thả file nhạc sấm sét xanh dương (.mp3, .wav)
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

    [Header("--- BỘ LỌC LAYER DỰ PHÒNG ---")]
    public LayerMask enemyLayer;        // Kéo Layer của Quái (Enemy) vào đây để đối chiếu trong code

    private float _cooldownTimer;
    private PlayerHealth _playerHealth; // Cầu nối lấy Level và trạng thái sống chết từ PlayerHealth

    void Awake()
    {
        // Lấy component PlayerHealth gắn chung trên người Player 2
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Kiểm tra nếu Player đã chết thì chặn hoàn toàn, không cho chạy hồi chiêu hay bấm nút
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Đếm ngược hồi chiêu liên tục
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // Nhấn phím M để gọi sấm sét xanh dương
        if (Input.GetKeyDown(skillKey))
        {
            // BƯỚC KHÓA CHIÊU CUỐI: Check Level từ hệ thống PlayerHealth
            if (_playerHealth != null && _playerHealth.currentLevel < 7)
            {
                // CHÈN DÒNG NÀY ĐỂ VĂNG CHỮ ĐỎ THÔNG BÁO KHÓA CHIÊU:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [M] ĐANG KHÓA! CẦN LEVEL 7", Color.red);
                }

                Debug.LogWarning($"<color=cyan>[Skill M đang khóa]</color> Tuyệt chiêu cuối cần đạt Level 7 để mở khóa! (Cấp hiện tại của bạn: {_playerHealth.currentLevel})");
                return; // Chặn đứng tại đây
            }

            // Đủ Level 7 + Hết hồi chiêu + Có Prefab hiệu ứng thì mới cho triển chiêu
            if (_cooldownTimer <= 0 && lightningPrefab != null)
            {
                CastBlueLightning();
            }
        }
    }

    void CastBlueLightning()
    {
        // SỬA TẠI ĐÂY: Quét diện rộng không giới hạn Layer Mask ngay tại vòng quét vật lý để tránh bỏ sót quái/Boss
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, skillRadius);

        Collider2D closestEnemy = null;
        float minDistance = float.MaxValue;

        // Vòng lặp lọc thông minh đa tầng giống hệt chiêu I
        foreach (Collider2D collider in hitColliders)
        {
            // Nếu là chính Player thì bỏ qua đéo đánh
            if (collider.CompareTag("Player")) continue;

            // Kiểm tra xem mục tiêu có thuộc layer Quái thường, mang tag Boss/Enemy hoặc thuộc layer Boss hay không
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

        // Nếu sau khi lọc xong toàn bộ Scene mà vẫn đéo có con địch nào hợp lệ
        if (closestEnemy == null)
        {
            if (SkillNotification.Instance != null)
            {
                SkillNotification.Instance.ShowMessage("KHÔNG CÓ ĐỊCH TRONG PHẠM VI SÉT GIẬT!", Color.yellow);
            }

            Debug.Log("<color=cyan>[Sét Xanh]</color> Không có quái hay Boss nào trong phạm vi để giật sét!");
            return; // Thoát sớm, bảo toàn hồi chiêu
        }

        // Kích hoạt hồi chiêu ngay khi tìm thấy mục tiêu chuẩn xác
        _cooldownTimer = cooldown;

        // Khởi tạo AudioSource 2D thủ công để sấm sét xanh nổ to rõ, đè bẹp nhạc nền
        if (blueLightningSound != null)
        {
            GameObject tempAudio = new GameObject("TempBlueLightningAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = blueLightningSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn
            aSource.volume = Mathf.Clamp01(skillVolume / 100f); // Quy đổi chuẩn từ hệ 100 về hệ 1.0 của Unity

            // ==========================================
            // LONG MẠCH: Gán âm thanh sét xanh đi qua đúng kênh CombatSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            aSource.Play();
            Destroy(tempAudio, blueLightningSound.length); // Phát xong tự dọn dẹp Object tạm
        }

        // Thực hiện giật sét lên đầu con mục tiêu gần nhất lọc được
        if (closestEnemy != null)
        {
            // Tạo vị trí xuất hiện ngay tại quái (cộng thêm 0.2f để tia năng lượng ôm trọn người quái)
            Vector3 spawnPosition = closestEnemy.transform.position + new Vector3(0, 0.2f, 0);

            // Sinh ra hiệu ứng tia sét xanh dương tại vị trí quái
            Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);

            // Gây 70 sát thương thẳng vào máu quái hoặc Boss
            if (closestEnemy.TryGetComponent<EnemyHeath>(out var enemyHP))
            {
                if (!enemyHP.IsDead)
                {
                    enemyHP.TakeDamage(damage);
                    Debug.Log($"<color=blue>[Sét Xanh phím M]</color> Đã nã {damage} HP vào đầu {closestEnemy.name}!");
                }
            }
        }
    }

    // Vẽ vòng tròn phạm vi màu xanh dương ngoài Scene để dễ căn chỉnh độ xa gần
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
}