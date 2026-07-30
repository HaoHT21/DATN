using UnityEngine;

public class RescueMilestoneTrigger : MonoBehaviour
{
    [Header("Mốc điều kiện")]
    [Tooltip("Số con tin cần cứu để kích hoạt")]
    public int requiredHostageCount = 8;

    [Header("Tham chiếu Cổng Cổ Đại")]
    [Tooltip("Kéo AncientGateController vào đây")]
    public AncientGateController ancientGate;

    [Header("1. Vùng Trigger (Môi trường)")]
    [Tooltip("GameObject chứa vùng Trigger sẽ được BẬT khi đủ 8 con tin")]
    public GameObject targetTriggerZone;

    [Header("2. GameObject xuất hiện cạnh Player")]
    [Tooltip("Prefab hoặc GameObject sẽ xuất hiện ngay cạnh Player")]
    public GameObject objectToSpawnNextToPlayer;

    [Tooltip("Khoảng cách xuất hiện so với vị trí Player")]
    public Vector3 spawnOffset = new Vector3(1.5f, 0f, 0f);

    private bool _hasTriggered;

    private void OnEnable()
    {
        HostageRescueManager manager = HostageRescueManager.EnsureInstance();
        manager.OnRescueCountChanged += HandleRescueCountChanged;

        // Kiểm tra ngay khi Enable (nếu đã cứu đủ 8 con tin trước đó)
        CheckCondition(manager.RescuedCount);
    }

    private void OnDisable()
    {
        if (HostageRescueManager.Instance != null)
        {
            HostageRescueManager.Instance.OnRescueCountChanged -= HandleRescueCountChanged;
        }
    }

    private void HandleRescueCountChanged(int currentCount)
    {
        CheckCondition(currentCount);
    }

    private void CheckCondition(int count)
    {
        // Nếu đã kích hoạt rồi hoặc chưa đủ 8 con tin thì BỎ QUA
        if (_hasTriggered || count < requiredHostageCount)
            return;

        if (ancientGate == null)
            ancientGate = FindFirstObjectByType<AncientGateController>();

        // Nếu cổng đã bị mở trước đó (ví dụ do 8 viên ngọc mở) thì ngưng không đè lên nữa
        if (ancientGate != null && ancientGate.State != GateState.Closed)
            return;

        // Khóa lại để tránh chạy lặp
        _hasTriggered = true;

        // 1. Mở Cổng + Chạy Animation + Chỉ BẬT PORTAL TRẮNG
        if (ancientGate != null)
        {
            ancientGate.OpenGateWithWhitePortal();
        }

        // 2. Kích hoạt vùng Trigger môi trường
        if (targetTriggerZone != null)
        {
            targetTriggerZone.SetActive(true);
            Debug.Log($"[Milestone] Đã kích hoạt Vùng Trigger: {targetTriggerZone.name}");
        }

        // 3. Spawn GameObject cạnh vị trí Player
        SpawnObjectNearPlayer();
    }

    private void SpawnObjectNearPlayer()
    {
        if (objectToSpawnNextToPlayer == null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[RescueMilestoneTrigger] Không tìm thấy GameObject có Tag 'Player'!");
            return;
        }

        Vector3 spawnPosition = player.transform.position + spawnOffset;

        if (!objectToSpawnNextToPlayer.scene.IsValid())
        {
            Instantiate(objectToSpawnNextToPlayer, spawnPosition, Quaternion.identity);
        }
        else
        {
            objectToSpawnNextToPlayer.transform.position = spawnPosition;
            objectToSpawnNextToPlayer.SetActive(true);
        }

        Debug.Log($"[Milestone] Đã xuất hiện {objectToSpawnNextToPlayer.name} cạnh Player!");
    }
}