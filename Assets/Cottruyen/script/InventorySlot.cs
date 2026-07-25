using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Số thứ tự của ô này (0, 1, 2...)")]
    public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[InventorySlot] Đã bấm chuột vào ô: {slotIndex} | Nút: {eventData.button}");

        if (InventoryData.Instance == null)
        {
            Debug.LogError("[InventorySlot] Lỗi: Không tìm thấy InventoryData.Instance trong Scene!");
            return;
        }

        if (InventoryData.Instance.sharedInventory == null)
        {
            Debug.LogError("[InventorySlot] Lỗi: sharedInventory đang bị null!");
            return;
        }

        // Kiểm tra nhấn chuột phải (Right Click)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("[InventorySlot] Đã nhận diện CHUỘT PHẢI!");

            if (slotIndex >= 0 && slotIndex < InventoryData.Instance.sharedInventory.Count)
            {
                if (InventoryData.Instance.sharedInventory[slotIndex] != null)
                {
                    if (SlotContextMenu.Instance != null)
                    {
                        Debug.Log($"[InventorySlot] Thành công! Đang gọi Menu cho ô index: {slotIndex}");
                        SlotContextMenu.Instance.ShowMenu(slotIndex, eventData.position);
                    }
                    else
                    {
                        Debug.LogError("[InventorySlot] Lỗi: SlotContextMenu.Instance đang bị NULL! Bạn đã gắn script SlotContextMenu vào GameObject nào trong Scene chưa?");
                    }
                }
                else
                {
                    Debug.LogWarning($"[InventorySlot] Ô số {slotIndex} không có đồ (Item tại index này là null).");
                }
            }
            else
            {
                Debug.LogWarning($"[InventorySlot] SlotIndex ({slotIndex}) vượt quá số lượng đồ đang có trong túi ({InventoryData.Instance.sharedInventory.Count}).");
            }
        }
    }
}