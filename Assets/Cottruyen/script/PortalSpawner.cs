using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    [Header("Cấu hình Cánh Cổng")]
    [SerializeField] private GameObject portalPrefab; // Kéo file Prefab cánh cổng dưới Project vào đây
    [SerializeField] private Transform portalSpawnPos; // Vị trí cánh cổng xuất hiện (ngang hồ nước)

    [Header("Cấu hình Điểm Đến")]
    [SerializeField] private Transform targetSpawnPosMap2; // Kéo ô 'SpawnPos_Map2' cố định vào đây!

    private NPCInteraction targetNPC;
    private bool portalSpawned = false;

    void Update()
    {
        if (portalSpawned) return;

        if (targetNPC == null)
        {
            targetNPC = FindFirstObjectByType<NPCInteraction>();
        }

        if (targetNPC != null && targetNPC.isFinished)
        {
            SpawnPortalAndAssignDestination();
        }
    }

    private void SpawnPortalAndAssignDestination()
    {
        portalSpawned = true;

        if (portalPrefab != null && portalSpawnPos != null && targetSpawnPosMap2 != null)
        {
            // 1. Sinh ra cánh cổng từ Prefab
            GameObject newPortal = Instantiate(portalPrefab, portalSpawnPos.position, Quaternion.identity);

            // 2. Lấy script Portal của cái cổng vừa tạo ra
            Portalto portaltoScript = newPortal.GetComponent<Portalto>();

            if (portaltoScript != null)
            {
                // 3. Tự động tiêm (gán) vị trí đích Map 2 cho nó bằng code!
                portaltoScript.spawnPointDestination = targetSpawnPosMap2;
            }

            Debug.Log("[Portal Spawner] Đã sinh cổng cạnh hồ và tự động kết nối tới Map 2 thành công!");
        }
    }
}