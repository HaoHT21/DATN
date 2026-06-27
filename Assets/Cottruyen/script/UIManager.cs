using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject tutorialPanel;
    public GameObject inventoryPanel;
    public GameObject mainUIPanel; // Chính là Canvas hoặc MainUI_Manager để tắt hết

    [Header("Tooltip Reference")]
    public GameObject tooltipPanel;
    public TMPro.TextMeshProUGUI tooltipText; // Hoặc Text nếu dùng UI thường

    private void Start()
    {
        // Mặc định khi mở UI lên: Hiện Hướng dẫn, ẩn Vật phẩm và Tooltip
        OpenTutorial();
    }

    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        HideTooltip();
    }

    public void OpenInventory()
    {
        tutorialPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        HideTooltip();
    }

    // Hàm dùng cho nút "Tắt tất cả"
    public void CloseAllUI()
    {
        mainUIPanel.SetActive(false);
    }

    // Các hàm bổ trợ cho Tooltip
    public void ShowTooltip(string description, Vector3 position)
    {
        tooltipPanel.SetActive(true);
        tooltipText.text = description;

        // Đặt vị trí Tooltip lệch một chút so với vị trí chuột
        tooltipPanel.transform.position = position + new Vector3(15f, -15f, 0f);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    // Hàm dùng cho nút "Mở UI" nằm ngoài màn hình chính
    public void OpenMainUI()
    {
        mainUIPanel.SetActive(true);

        // Bạn có thể chọn mặc định mở tab nào khi UI xuất hiện:
        OpenTutorial(); // Hoặc OpenInventory(); nếu muốn ưu tiên hiện vật phẩm
    }
}