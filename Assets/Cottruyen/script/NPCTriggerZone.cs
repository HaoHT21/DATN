using UnityEngine;
using System.Collections;

public class NPCTriggerZone : MonoBehaviour
{
    [Header("Cấu hình NPC đích (Kéo NPC vào đây)")]
    public GameObject npcObject;

    [Header("Cấu hình Hiệu ứng (Effects)")]
    public GameObject spawnEffectPrefab;
    public GameObject despawnEffectPrefab;
    public float effectDuration = 1.5f;

    // =================================================================
    // ĐOẠN THÊM VÀO: Cấu hình riêng biệt khóa hồi sinh chỉ cho Zone 2
    // =================================================================
    [Header("Cấu hình Riêng Cho Khu Vực Đấu Boss (Zone 2)")]
    public bool isBadEndingZone = false; // Tích chọn ô này duy nhất trên NPC_Trigger_Zone (2)
    public GameObject badEndingCanvas;   // Kéo Panel UI Bad Ending vào đây

    private NPCInteraction npcInteraction;
    private bool hasTriggered = false;

    void Start()
    {
        if (npcObject != null)
        {
            npcInteraction = npcObject.GetComponent<NPCInteraction>();
            npcObject.SetActive(false); // Ẩn NPC lúc đầu
        }

        // Tự động ẩn UI Bad Ending khi mới vào game nếu lỡ quên tắt
        if (badEndingCanvas != null)
        {
            badEndingCanvas.SetActive(false);
        }
    }

    // =================================================================
    // ĐOẠN THÊM VÀO: Nhận lệnh từ Boss truyền sang để tự kích hoạt hội thoại
    // =================================================================
    public void TriggerDialogueFromBoss(GameObject spawnedNPC)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // Gán NPC vừa sinh ra làm đối tượng nói chuyện chính
        npcObject = spawnedNPC;

        StartCoroutine(SpawnNPCRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra Player đi vào vùng nguy hiểm
        if (other.CompareTag("Player") && isBadEndingZone)
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
            if (playerHP != null)
            {
                playerHP.SetInBadEndZone(true, badEndingCanvas);
            }
        }

        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(SpawnNPCRoutine());
        }
    }

    // =================================================================
    // ĐOẠN THÊM VÀO: Xử lý khi Player đang đứng sẵn trong vùng từ trước
    // =================================================================
    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            hasTriggered = true;
            StartCoroutine(SpawnNPCRoutine());
        }
    }

    // =================================================================
    // ĐOẠN THÊM VÀO: Trả lại trạng thái hồi sinh bình thường khi thoát vùng
    // =================================================================
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isBadEndingZone)
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
            if (playerHP != null)
            {
                playerHP.SetInBadEndZone(false, null);
            }
        }
    }

    IEnumerator SpawnNPCRoutine()
    {
        if (spawnEffectPrefab != null && npcObject != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, npcObject.transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        yield return new WaitForSecondsRealtime(effectDuration * 0.5f);

        if (npcObject != null) npcObject.SetActive(true);

        yield return new WaitForSecondsRealtime(effectDuration * 0.5f);

        // Lấy script NPCInteraction từ thực thể NPC hiện tại (hỗ trợ cho cả NPC sinh ra từ Boss)
        if (npcObject != null)
        {
            npcInteraction = npcObject.GetComponent<NPCInteraction>();
        }

        if (npcInteraction != null)
        {
            // Truyền chính TriggerZone này vào NPC để nó biết đường phản hồi
            npcInteraction.RegisterTriggerZone(this);
            npcInteraction.Interact();
        }
    }

    public void OnDialogueComplete()
    {
        StartCoroutine(DespawnNPCRoutine());
    }

    IEnumerator DespawnNPCRoutine()
    {
        if (despawnEffectPrefab != null && npcObject != null)
        {
            GameObject effect = Instantiate(despawnEffectPrefab, npcObject.transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        yield return new WaitForSecondsRealtime(effectDuration);

        if (npcObject != null) npcObject.SetActive(false);
        Destroy(gameObject);
    }
}