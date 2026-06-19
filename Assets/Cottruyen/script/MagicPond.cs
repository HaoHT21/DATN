using UnityEngine;
using System.Collections;
using System.Reflection; // Thêm thư viện này để can thiệp vào biến private của script cũ

public class MagicPond : MonoBehaviour
{
    [Header("Cấu hình Giao diện")]
    [SerializeField] private TMPro.TMP_Text notificationText;

    [Header("Liên kết Trigger Zone Gốc")]
    [SerializeField] private NPCTriggerZone pondTriggerZone;

    private bool isPlayerNearby = false;
    private PlayerInventory playerInventory;
    private bool isActivated = false;

    void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        if (pondTriggerZone != null)
        {
            // 🔥 CHIẾN THUẬT MỚI: Ép biến private 'hasTriggered' của NPCTriggerZone thành true ngay lập tức.
            // Điều này khiến script cũ tưởng rằng nó đã chạy rồi, nên khi Player đi vào hồ, nó sẽ REJECT (bỏ qua), không sinh thần bừa bãi nữa.
            SetPrivateField(pondTriggerZone, "hasTriggered", true);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Bạn chưa kéo thành phần NPCTriggerZone vào ô Pond Trigger Zone!");
        }
    }

    void Update()
    {
        if (isPlayerNearby && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory != null && playerInventory.hasTruthStone)
            {
                StartCoroutine(ActivatePondRoutine());
            }
            else
            {
                if (notificationText != null)
                {
                    notificationText.text = "Bạn không có đá! Hãy đi tìm viên đá sự thật.";
                }
            }
        }
    }

    private IEnumerator ActivatePondRoutine()
    {
        isActivated = true;

        if (playerInventory != null)
        {
            playerInventory.hasTruthStone = false; // Tịch thu đá
        }

        if (notificationText != null)
            notificationText.gameObject.SetActive(false); // Ẩn chữ

        if (pondTriggerZone != null)
        {
            // 🔥 MỞ KHÓA: Trả biến 'hasTriggered' về lại bằng false để cho phép nó hoạt động đúng 1 lần duy nhất này
            SetPrivateField(pondTriggerZone, "hasTriggered", false);

            // Ép chạy Coroutine sinh hiệu ứng và gọi thần của script cũ
            pondTriggerZone.StartCoroutine("SpawnNPCRoutine");
            Debug.Log("[Hồ Nước] Gọi thần thành công sau khi dâng đá!");
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerInventory = other.GetComponent<PlayerInventory>();

            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(true);
                notificationText.text = "Cần viên đá sự thật... Nhấn [E] để dâng hiến";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerInventory = null;

            if (notificationText != null)
                notificationText.gameObject.SetActive(false);
        }
    }

    // Hàm phụ trợ dùng Reflection để can thiệp biến private của script khác mà không cần sửa file code của nó
    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}