using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image itemIconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ItemData _currentItem;

    public void SetupSlot(ItemData data)
    {
        if (data == null) return;

        _currentItem = data;
        if (itemIconImage != null) itemIconImage.sprite = data.itemIcon;
        if (nameText != null) nameText.text = data.itemName;
        if (priceText != null) priceText.text = data.price.ToString() + "G";

        // Làm sạch sự kiện cũ để tránh việc click 1 lần kích hoạt lệnh mua nhiều lần
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);

            // ÉP NÚT BẤM PHẢI BẬT TƯƠNG TÁC
            buyButton.interactable = true;
        }
    }

    public void OnBuyClicked()
    {
        if (_currentItem == null)
        {
            Debug.LogError("[Shop UI Error] Ô này không chứa dữ liệu vật phẩm ScriptableObject!");
            return;
        }

        // THỬ NGHIỆM IN LOG: Để kiểm tra xem chuột của bạn đã thực sự chạm được vào logic code chưa!
        Debug.LogWarning($"====== UI ĐÃ NHẬN CLICK MUA MÓN: {_currentItem.itemName} (Giá: {_currentItem.price}G) ======");

        // CÁCH 1: Tìm nhân vật chính qua cầu nối NetworkMenuManager
        if (NetworkMenuManager.Instance != null && NetworkMenuManager.Instance.LocalPlayerStats != null)
        {
            PlayerStats localPlayer = NetworkMenuManager.Instance.LocalPlayerStats;
            if (localPlayer != null)
            {
                localPlayer.RPC_RequestPurchase(_currentItem.price, _currentItem.itemID);
                return;
            }
        }

        // CÁCH 2 (FALLBACK): Nếu cầu nối trên chưa kịp nạp, tự lùng sục trực tiếp nhân vật chính trên Scene
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PlayerStats stats = p.GetComponent<PlayerStats>();
            // Phải đảm bảo đây là nhân vật do chính máy này điều khiển (Tránh tìm nhầm nhân vật của người chơi khác)
            if (stats != null && stats.Object != null && stats.Object.HasInputAuthority)
            {
                Debug.Log("[Shop UI - Cứu nguy] Đã tìm thấy Player bằng cơ chế quét trực tiếp thẻ Tag!");
                stats.RPC_RequestPurchase(_currentItem.price, _currentItem.itemID);
                return;
            }
        }

        Debug.LogError("[Shop UI Error] Thất bại! Không thể tìm thấy linh kiện PlayerStats mạng của bạn để thực hiện giao dịch!");
    }
}