using UnityEngine;
using Fusion;

public class PlayerStats : NetworkBehaviour
{
    // Đồng bộ biến tiền trên toàn mạng, chỉ Host được quyền sửa đổi trực tiếp
    [Networked] public int CurrentMoney { get; set; }

    public override void Spawned()
    {
        // Khi người chơi được khởi sinh, cấp sẵn một lượng vốn ban đầu trên máy chủ (Host)
        if (Object.HasStateAuthority)
        {
            CurrentMoney = 1000; // Cho sẵn 1000 vàng để test mua súng thoải mái
        }

        // Đăng ký lưu vết người chơi nội bộ vào quản lý hệ thống
        if (Object.HasInputAuthority && NetworkMenuManager.Instance != null)
        {
            NetworkMenuManager.Instance.LocalPlayerStats = this;
        }
    }

    // RPC: Client gửi yêu cầu mua đồ, lệnh này sẽ bay xuyên không gian mạng lên thực thi tại máy Host
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int price, int id)
    {
        // Kiểm tra điều kiện ngặt nghèo ngay tại Host để chống hack tiền từ Client
        if (CurrentMoney >= price)
        {
            CurrentMoney -= price; // Thực hiện trừ tiền trên ví mạng công bằng

            Debug.Log($"[Giao dịch] Mua thành công! Số tiền còn lại: {CurrentMoney}G");

            // Tiến hành sinh vật phẩm thực tế rơi ra đất
            SpawnPurchasedItemOnGround(id);
        }
        else
        {
            Debug.LogWarning("[Giao dịch] Thất bại! Người chơi không đủ số dư để thanh toán.");
        }
    }

    private void SpawnPurchasedItemOnGround(int id)
    {
        if (ShopManager.Instance == null) return;

        // Truy vấn lấy file Prefab mạng chuẩn từ danh sách ScriptableObject
        GameObject itemPrefabToSpawn = ShopManager.Instance.GetPrefabByID(id);

        if (itemPrefabToSpawn != null)
        {
            // Tạo độ lệch nhỏ (Offset) để món đồ rơi ra ngay phía bên cạnh dưới chân nhân vật mua
            Vector3 spawnPosition = transform.position + new Vector3(0, -0.8f, 0);

            // Lệnh tối quan trọng của Photon Fusion: Sinh thực thể mạng đồng bộ 100% các máy khách
            Runner.Spawn(itemPrefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
