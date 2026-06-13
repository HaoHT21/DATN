using UnityEngine;
using System.Collections;

public class BossDecisionTrigger : MonoBehaviour
{
    [Header("Hiệu ứng nổ khi biến hình (VFX)")]
    public GameObject deathEffectPrefab;

    [Header("Cấu hình Prefab sau phán quyết")]
    public GameObject executedSpawnPrefab; // Nhân vật mới khi kết liễu
    public GameObject nextPhasePrefab;     // Dạng mới khi tha mạng

    [Header("Khác")]
    public GameObject nextDialogueTriggerZone; // Vùng thoại tiếp theo
    public string deathAnimationStateName = "Death Animation";

    private bool isTriggered = false;

    // 1. Kích hoạt chuỗi logic khi Boss hết máu
    public void ActivateDecisionSequence()
    {
        if (isTriggered) return;
        StartCoroutine(HandleSequence());
    }

    private IEnumerator HandleSequence()
    {
        isTriggered = true;

        // Tắt va chạm để Boss không bị vướng
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Chạy hoạt ảnh gục ngã
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.Play(deathAnimationStateName);

        // Chờ diễn hoạt ảnh xong
        yield return new WaitForSeconds(1.5f);

        // Gọi UI phán quyết
        if (BossDecisionUI.Instance != null)
        {
            BossDecisionUI.Instance.ShowTriggerDecision(this);
        }
        else
        {
            Debug.LogError("⚠️ BossDecisionUI không tìm thấy trong scene!");
        }
    }

    // 2. Nhánh Kết Liễu
    public void ConfirmExecute()
    {
        StartCoroutine(FinalizeChoice(executedSpawnPrefab));
    }

    // 3. Nhánh Tha Mạng
    public void ConfirmSpare()
    {
        StartCoroutine(FinalizeChoice(nextPhasePrefab));
    }

    // 4. Hàm xử lý nổ và tạo nhân vật mới (dùng chung cho cả 2 nhánh)
    private IEnumerator FinalizeChoice(GameObject prefabToSpawn)
    {
        // Hiệu ứng nổ
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }

        // Ẩn Boss hiện tại
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(0.5f);

        // Sinh nhân vật mới hoặc dạng mới
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position, transform.rotation);
        }

        // Mở khóa vùng thoại tiếp theo
        if (nextDialogueTriggerZone != null)
        {
            nextDialogueTriggerZone.SetActive(true);
        }

        Destroy(gameObject);
    }
}