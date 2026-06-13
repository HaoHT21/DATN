using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    // Tạo bảng danh sách các hành động sau khi thoại xong
    public enum AfterDialogueAction
    {
        StartCombat,    // Tấn công Player (Kích hoạt NPCCombat)
        DespawnWithEffect // Biến mất (Gọi NPCTriggerZone)
    }

    // =================================================================
    // THÊM BIẾN NÀY ĐỂ DIRECTANIMATIONCONTROLLER THEO DÕI
    // =================================================================
    [HideInInspector] public bool isFinished = false;

    [Header("Cấu hình hành động sau khi thoại")]
    public AfterDialogueAction endAction = AfterDialogueAction.StartCombat;

    [Header("Cài đặt tốc độ chữ")]
    public float typingSpeed = 0.05f;

    [Header("Kịch bản hội thoại của NPC này")]
    public CycleContent[] npcScript;

    // Biến để lưu trữ vùng Trigger (nếu có) truyền vào
    private NPCTriggerZone assignedTriggerZone;

    // Hàm lưu lại thông tin Trigger Zone khi NPC được gọi từ vùng Trigger
    public void RegisterTriggerZone(NPCTriggerZone zone)
    {
        assignedTriggerZone = zone;
    }

    public void Interact()
    {
        // Mỗi khi bắt đầu tương tác lại, reset biến check về false
        isFinished = false;

        if (DialogueManager.Instance != null)
        {
            // Truyền dữ liệu sang DialogueManager để mở Panel lên chat
            DialogueManager.Instance.StartCycling(npcScript, this, typingSpeed);
        }
        else
        {
            Debug.LogError("Không tìm thấy DialogueManager trong Scene!");
        }
    }

    // Hàm này được DialogueManager tự động gọi khi kết thúc câu thoại cuối cùng
    public void BeginCombat()
    {
        // 🧠 Kiểm tra xem người dùng cài đặt cho NPC này làm gì sau khi thoại
        switch (endAction)
        {
            case AfterDialogueAction.StartCombat:
                HandleStartCombat();
                break;

            case AfterDialogueAction.DespawnWithEffect:
                HandleDespawn();
                break;
        }

        // ĐỐI THOẠI XONG HOÀN TOÀN -> Bật chốt chặn báo hiệu cho bộ bắn đạn
        isFinished = true;
    }

    // Xử lý logic TRẬN ĐẤU
    private void HandleStartCombat()
    {
        NPCCombat combatScript = GetComponent<NPCCombat>();
        if (combatScript != null)
        {
            combatScript.enabled = true; // Bật script tấn công lên
            Debug.Log($"[{gameObject.name}] Thoại xong -> Kích hoạt NPCCombat!");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Cài đặt là StartCombat nhưng không tìm thấy script NPCCombat đính kèm!");
        }
    }

    // Xử lý logic BIẾN MẤT
    private void HandleDespawn()
    {
        // Ưu tiên gọi chính xác Trigger Zone đã sinh ra con NPC này
        if (assignedTriggerZone != null)
        {
            assignedTriggerZone.OnDialogueComplete();
            Debug.Log($"[{gameObject.name}] Thoại xong -> Gọi Trigger Zone để chạy hiệu ứng biến mất!");
        }
        else
        {
            // Nếu không được sinh ra từ Trigger cụ thể, tự đi tìm vùng Trigger xung quanh
            NPCTriggerZone foundZone = FindObjectOfType<NPCTriggerZone>();
            if (foundZone != null)
            {
                foundZone.OnDialogueComplete();
            }
            else
            {
                // Nếu hoàn toàn không thấy hiệu ứng nào, ẩn luôn NPC đi tránh lỗi
                Debug.LogWarning($"[{gameObject.name}] Không tìm thấy NPCTriggerZone để chạy hiệu ứng. Tự động ẩn danh.");
                gameObject.SetActive(false);
            }
        }
    }
}