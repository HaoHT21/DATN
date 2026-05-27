using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image icon;
    public Button buyButton;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    private ItemData _data;

    public void SetupSlot(ItemData data)
    {
        _data = data;
        if (icon != null) icon.sprite = data.itemIcon;
        if (itemNameText != null) itemNameText.text = data.itemName;
        if (priceText != null) priceText.text = data.price.ToString();

        // Gán listener qua code, KHÔNG cần kéo thả trong Inspector
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClick);
    }

    private void OnBuyClick()
    {
        Debug.Log("[UI] Nút mua đã được nhấn!");

        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TryPurchase(_data.price, _data.itemID);
        }
        else
        {
            Debug.LogError("[UI] Không tìm thấy PlayerStats trong Scene!");
        }
    }
}