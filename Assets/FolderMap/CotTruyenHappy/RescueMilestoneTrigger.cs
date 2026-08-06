using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RescueMilestoneTrigger : MonoBehaviour
{
    [Header("Mốc điều kiện")]
    [Tooltip("Số con tin cần cứu để kích hoạt")]
    public int requiredHostageCount = 8;

    [Header("Tham chiếu Cổng Cổ Đại")]
    [Tooltip("Kéo AncientGateController vào đây")]
    public AncientGateController ancientGate;

    [Header("1. GameObject xuất hiện cạnh Player khi vào Trigger")]
    [Tooltip("Prefab hoặc GameObject sẽ xuất hiện ngay cạnh Player khi bước vào Trigger này")]
    public GameObject objectToSpawnNextToPlayer;

    [Tooltip("Khoảng cách xuất hiện so với vị trí Player")]
    public Vector3 spawnOffset = new Vector3(1.5f, 0f, 0f);

    [Header("2. Vùng Trigger (Môi trường) - Kích hoạt BẬT SAU")]
    [Tooltip("GameObject chứa vùng Trigger sẽ được BẬT SAU KHI NPC đã xuất hiện")]
    public GameObject targetTriggerZone;

    private bool _isMilestoneUnlocked; // Đã đủ 8 con tin chưa
    private bool _hasSpawnedObject;     // Đã spawn NPC cạnh Player chưa
    private GameObject _spawnedInstance;

    private void OnEnable()
    {
        HostageRescueManager manager = HostageRescueManager.EnsureInstance();
        manager.OnRescueCountChanged += HandleRescueCountChanged;

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
        if (_isMilestoneUnlocked || count < requiredHostageCount)
            return;

        if (ancientGate == null)
            ancientGate = FindFirstObjectByType<AncientGateController>();

        if (ancientGate != null && ancientGate.State != GateState.Closed)
            return;

        _isMilestoneUnlocked = true;

        // Mở Cổng
        if (ancientGate != null)
        {
            ancientGate.OpenGateWithWhitePortal();
        }

        // LƯU Ý: Đã bỏ bước bật targetTriggerZone ở đây để chờ Spawn NPC trước
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Điều kiện: Đã đủ 8 con tin + Là Player + Chưa Spawn NPC
        if (_isMilestoneUnlocked && !_hasSpawnedObject && other.CompareTag("Player"))
        {
            _hasSpawnedObject = true;

            // BƯỚC 1: Spawn/Đưa NPC ra cạnh Player trước
            SpawnOrMoveObjectNextToPlayer(other.gameObject);

            // BƯỚC 2: Rồi mới kích hoạt targetTriggerZone
            ActivateTargetTriggerZone();
        }
    }

    private void SpawnOrMoveObjectNextToPlayer(GameObject player)
    {
        if (objectToSpawnNextToPlayer == null)
            return;

        Vector3 spawnPosition = player.transform.position + spawnOffset;

        if (!objectToSpawnNextToPlayer.scene.IsValid())
        {
            if (_spawnedInstance == null)
            {
                _spawnedInstance = Instantiate(objectToSpawnNextToPlayer, spawnPosition, Quaternion.identity);
            }
            else
            {
                _spawnedInstance.transform.position = spawnPosition;
                _spawnedInstance.SetActive(true);
            }
        }
        else
        {
            objectToSpawnNextToPlayer.transform.position = spawnPosition;
            objectToSpawnNextToPlayer.SetActive(true);
        }

        Debug.Log($"[Milestone] BƯỚC 1: Đã xuất hiện {objectToSpawnNextToPlayer.name} cạnh Player!");
    }

    private void ActivateTargetTriggerZone()
    {
        if (targetTriggerZone != null)
        {
            targetTriggerZone.SetActive(true);
            Debug.Log($"[Milestone] BƯỚC 2: Kích hoạt Vùng Trigger môi trường: {targetTriggerZone.name}");
        }
    }
}