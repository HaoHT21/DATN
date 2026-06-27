using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Số thứ tự của ô này (Ô đầu tiên là 0, 1, 2...)")]
    public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryData.Instance == null || InventoryData.Instance.sharedInventory == null) return;

        // KIỂM TRA NẾU NGƯỜI CHƠI NHẤN CHUỘT PHẢI
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // CHỈ XỬ LÝ NẾU LÀ Ô THUỘC KHOLON (Slot Index bắt đầu từ 4 trở lên)
            if (slotIndex >= 0 && slotIndex < InventoryData.Instance.sharedInventory.Count)
            {
                // Chỉ hiển thị Menu nếu tại vị trí ô này thực sự đang chứa vũ khí (không bị null)
                if (InventoryData.Instance.sharedInventory[slotIndex] != null)
                {
                    if (SlotContextMenu.Instance != null)
                    {
                        // Mở Menu tại tọa độ chuột và truyền chính xác index (bắt đầu từ 4)
                        SlotContextMenu.Instance.ShowMenu(slotIndex, eventData.position);
                    }
                }
            }
        }
    }
}