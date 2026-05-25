using UnityEngine;
using Fusion;
using TMPro;

public class PlayerStats : NetworkBehaviour
{
    [Header("Score & Money Settings")]
    // Tự động đồng bộ số tiền xuyên mạng. Khi ví tiền thay đổi, tự kích hoạt hàm OnScoreChanged dưới Client
    [Networked, OnChangedRender(nameof(OnScoreChanged))]
    public int Score { get; set; }

    [Header("UI Components")]
    public TextMeshProUGUI scoreText;

    public override void Spawned()
    {
        // Máy chủ (Host) cấp sẵn 1000 xu ban đầu cho nhân vật khi vừa sinh ra
        if (Object.HasStateAuthority)
        {
            Score = 1000;
        }

        // Đăng ký nhân vật của chính máy này vào hệ thống quản lý để Shop dễ tìm kiếm
        if (Object.HasInputAuthority && NetworkMenuManager.Instance != null)
        {
            NetworkMenuManager.Instance.LocalPlayerStats = this;
        }

        TryFindScoreUI();
        UpdateUI();
    }

    // Hàm Hook bắt sự kiện thay đổi dữ liệu mạng chuẩn của Fusion 2.0 (Không dùng static)
    void OnScoreChanged()
    {
        UpdateUI();
    }

    /// <summary>
    /// Tự động tìm kiếm linh kiện hiển thị tiền trên Canvas để tránh lỗi load chậm của Client
    /// </summary>
    private void TryFindScoreUI()
    {
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("CoinScore");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    /// <summary>
    /// Hàm cộng xu mạng (Chỉ chạy duy nhất trên máy Host/Server để bảo mật chống hack)
    /// </summary>
    public void AddCoin(int amount)
    {
        if (Object.HasStateAuthority)
        {
            Score += amount;
        }
    }

    /// <summary>
    /// Ép văn bản trên màn hình UI cập nhật số tiền mới nhất
    /// </summary>
    public void UpdateUI()
    {
        TryFindScoreUI();

        if (scoreText != null)
        {
            scoreText.text = "" + Score;
            Debug.Log($"[Ví Mạng UI] Đã đồng bộ số hiển thị trên Canvas thành: {Score} xu.");
        }
    }

    // Lệnh RPC: Client bắn yêu cầu giao dịch xuyên không gian mạng lên cho Host xử lý trừ tiền
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int price, int id)
    {
        // Server kiểm tra số dư ví tiền thực tế trên mạng
        if (Score >= price)
        {
            Score -= price; // Máy chủ thực hiện trừ tiền trực tiếp trên ví gốc

            Debug.Log($"[RPC Thành Công] Đã trừ {price} xu. Số dư ví mạng còn lại: {Score}");

            // Kích hoạt hàm sinh món đồ rơi ra đất
            SpawnPurchasedItemOnGround(id);
        }
        else
        {
            Debug.LogWarning($"[RPC Từ Chối] Không đủ tiền! Ví có {Score} xu, món đồ giá {price} xu.");
        }
    }

    private void SpawnPurchasedItemOnGround(int id)
    {
        if (ShopManager.Instance == null) return;

        // Lấy Prefab chứa NetworkObject từ danh sách cấu hình ShopManager dựa vào ID món đồ
        GameObject itemPrefabToSpawn = ShopManager.Instance.GetPrefabByID(id);

        if (itemPrefabToSpawn != null)
        {
            // Tạo vị trí rơi ngay dưới chân người chơi (hơi lệch xuống dưới một chút)
            Vector3 spawnPosition = transform.position + new Vector3(0, -0.8f, 0);

            // Gọi lệnh sinh vật thể mạng đồng bộ tối cao của Fusion để tất cả Client cùng nhìn thấy món đồ rơi ra
            Runner.Spawn(itemPrefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}