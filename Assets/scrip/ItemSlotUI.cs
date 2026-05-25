using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;

    private ItemData _currentItem;

    // Hàm này tự động chạy để vẽ giao diện cho ô đồ
    public void SetupSlot(ItemData data)
    {
        _currentItem = data;
        itemIconImage.sprite = data.itemIcon;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString() + "G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        // Gọi thông qua NetworkMenuManager quản lý tập trung của phòng game bạn
        if (NetworkMenuManager.Instance != null && NetworkMenuManager.Instance.LocalPlayerStats != null)
        {
            PlayerStats localPlayer = NetworkMenuManager.Instance.LocalPlayerStats;
            localPlayer.RPC_RequestPurchase(_currentItem.price, _currentItem.itemID);
        }
        else
        {
            Debug.LogError("Không tìm thấy dữ liệu Player nội bộ để thực hiện giao dịch!");
        }
    }
}