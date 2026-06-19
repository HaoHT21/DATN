using UnityEngine;

public class Portalto : MonoBehaviour
{
    // Thay vì kéo cánh cổng kia, bạn chỉ cần kéo một GameObject trống (SpawnPoint) ở Map bên kia vào đây
    [Header("Điểm đáp xuống ở Map bên kia")]
    public Transform spawnPointDestination;

    [Header("Thời gian chờ giữa 2 lần biến dịch")]
    [SerializeField] private float teleportCooldown = 1.0f;

    private static float nextTeleportTime = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= nextTeleportTime)
        {
            if (spawnPointDestination != null)
            {
                nextTeleportTime = Time.time + teleportCooldown;

                // Dịch chuyển Player tới thẳng Vị trí đích cố định
                collision.transform.position = spawnPointDestination.position;

                Debug.Log($"[Portal] Đã dịch chuyển thành công tới vị trí: {spawnPointDestination.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Chưa gán vị trí đáp xuống ở Map bên kia!");
            }
        }
    }
}