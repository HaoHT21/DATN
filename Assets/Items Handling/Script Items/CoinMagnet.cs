using UnityEngine;
using System.Collections.Generic;
using System;

public class CoinMagnet : MonoBehaviour
{
    [Header("Cấu hình Coin")]
    [SerializeField] private int coinValue = 1;
    public AudioClip coinPickupSound;

    [Header("--- CẤU HÌNH ÂM LƯỢNG XU ---")]
    [Range(0f, 100f)] public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng từ 0 đến 100 ngoài Inspector

    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;
    [SerializeField] private float acceleration = 2f;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask playerLayer;

    private Transform targetPlayer;
    private bool isFlying = false;
    private float currentSpeed;

    private void Start()
    {
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!isFlying)
            FindPlayerPhysics();
        else
            FlyToPlayer();
    }

    void FindPlayerPhysics()
    {
        // Quét các đối tượng trong layer Player
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (hit != null)
        {
            // Kiểm tra Tag "Player" thay vì tìm GetComponent để tránh lỗi do xóa script trên Prefab
            if (hit.CompareTag("Player"))
            {
                targetPlayer = hit.transform;
                isFlying = true;
            }
        }
    }

    void FlyToPlayer()
    {
        if (targetPlayer == null)
        {
            isFlying = false;
            currentSpeed = moveSpeed;
            return;
        }

        // Tăng tốc dần dần khi đang bay
        currentSpeed += acceleration * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, currentSpeed * Time.deltaTime);

        // Kiểm tra khoảng cách để thu thập
        float sqrDistance = (transform.position - targetPlayer.position).sqrMagnitude;
        if (sqrDistance < 0.04f)
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        // Khởi tạo AudioSource 2D thủ công và gán qua kênh envGroup (kênh bẫy / môi trường)
        if (coinPickupSound != null)
        {
            GameObject tempAudio = new GameObject("TempCoinAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = coinPickupSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn
            aSource.volume = Mathf.Clamp01(soundVolume / 100f); // Quy đổi chuẩn hệ 0-100 về 0.0-1.0 của Unity

            // Gán thẳng đi qua kênh envGroup giống hệt bên TrapAudio
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
            }

            aSource.Play();
            Destroy(tempAudio, coinPickupSound.length); // Phát xong tự hủy Object tạm
        }

        // 2. CỘNG XU VÀO STATS (Sử dụng Singleton Instance thay vì GetComponent - GIỮ NGUYÊN)
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCoin(coinValue);
            Debug.Log($"<color=yellow>[Coin]</color> Đã cộng {coinValue} xu vào hệ thống.");
        }
        else
        {
            Debug.LogWarning("[Coin] Không tìm thấy PlayerStats.Instance!");
        }

        // 3. Hủy đối tượng
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.84f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}