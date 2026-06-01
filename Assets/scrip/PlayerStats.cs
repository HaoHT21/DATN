using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int Score = 1000;
    public TextMeshProUGUI scoreText;
    
    // Thêm dòng này để các script khác gọi được PlayerStats
    public static PlayerStats Instance;

    private void Awake() => Instance = this; // Khởi tạo Singleton

    private void Start()
    {
        GameSessionSave.LoadInto(this);
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    public bool TryPurchase(int price, int itemID)
    {
        Debug.Log($"[Shop] Đang mua ID: {itemID}, Giá: {price}. Số dư: {Score}");

        if (Score >= price)
        {
            Score -= price;
            UpdateUI();
            SpawnPurchasedItem(itemID);
            Debug.Log("[Shop] Mua thành công!");
            return true;
        }
        else
        {
            Debug.Log("[Shop] Không đủ tiền!");
            return false;
        }
    }

    private void SpawnPurchasedItem(int id)
    {
        // LOGIC MỚI: Phân biệt ID để lấy prefab từ đúng nơi
        GameObject prefab = null;

        if (id >= 20) // Giả sử ID nhân vật bắt đầu từ 20 trở lên
        {
            prefab = HiroManager.Instance.GetHeroPrefabByID(id);
        }
        else // Các ID nhỏ hơn là vật phẩm thông thường
        {
            // Kiểm tra xem ShopManager có tồn tại không
            if (ShopManager.Instance != null)
                prefab = ShopManager.Instance.GetPrefabByID(id);
        }

        if (prefab != null)
        {
            // Xử lý đổi nhân vật nếu là nhân vật
            if (id >= 20)
            {
                GameObject currentObj = GameObject.FindGameObjectWithTag("Player");
                Vector3 spawnPos = currentObj ? currentObj.transform.position : Vector3.zero;
                if (currentObj) Destroy(currentObj);
                Instantiate(prefab, spawnPos, Quaternion.identity).tag = "Player";
            }
            else
            {
                // Xử lý vật phẩm thường như cũ
                Instantiate(prefab, transform.position + new Vector3(0, -0.8f, 0), Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError($"[Shop] Không tìm thấy Prefab cho ID: {id}");
        }
    }

    public void UpdateUI()
    {
        if (scoreText == null) scoreText = GameObject.Find("CoinScore")?.GetComponent<TextMeshProUGUI>();
        if (scoreText != null) scoreText.text = Score.ToString();
    }
}