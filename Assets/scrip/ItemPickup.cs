using Fusion;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    public int weaponIndex; // ID của khẩu súng (ví dụ: 1 là Pistol, 2 là Rifle)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ Host mới xử lý việc nhặt đồ để tránh xung đột mạng
        if (!Object || !Object.HasStateAuthority) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                // Gọi hàm nhặt súng trên Player
                player.Rpc_PickupWeapon(weaponIndex);

                // Sau khi nhặt xong thì xóa vật phẩm trên Map
                Runner.Despawn(Object);
            }
        }
    }
}