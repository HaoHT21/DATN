using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int Score = 100;
    public TextMeshProUGUI scoreText;

    // Singleton instance để các script khác (như CoinMagnet) truy cập
    public static PlayerStats Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Lưu ý: Nếu dữ liệu cũ reset tiền, hãy tạm thời comment dòng này lại để test
        //GameSessionSave.LoadInto(this);
        UpdateUI();
    }

    public int CoinCount => Score;

    public void AddCoin(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    public bool TrySpendCoins(int amount)
    {
        if (Score < amount)
            return false;

        Score -= amount;
        UpdateUI();
        return true;
    }

    public bool TryPurchase(int price, int itemID)
    {
        Debug.Log($"[Shop] Đang mua ID: {itemID}, Giá: {price}. Số dư: {Score}");

        if (Score >= price)
        {
            Score -= price;
            UpdateUI();

            // CHỈ spawn nếu là vật phẩm thông thường rớt dưới đất (itemID < 20)
            // Nếu là Hero (itemID >= 20), HeroSlot.cs sẽ tự xử lý SwapPlayerOnMap để giữ đúng Level, HP, Mana và Súng
            if (itemID < 20)
            {
                SpawnPurchasedItem(itemID);
            }

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
        // Lấy prefab vật phẩm từ ShopManager
        GameObject prefab = ShopManager.Instance?.GetPrefabByID(id);

        if (prefab != null)
        {
            // Tìm vị trí của Player hiện tại trong Scene
            GameObject currentObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = (currentObj != null) ? currentObj.transform.position : Vector3.zero;

            // Rớt vật phẩm dưới chân Player (Offset Y = -0.8f)
            Instantiate(prefab, spawnPos + new Vector3(0, -0.8f, 0), Quaternion.identity);
            Debug.Log($"[Shop] Đã spawn vật phẩm ID: {id} tại vị trí của Player");
        }
        else
        {
            Debug.LogError($"[Shop] Không tìm thấy Prefab cho ID: {id}. Kiểm tra lại ShopManager!");
        }
    }

    public void UpdateUI()
    {
        if (scoreText == null)
        {
            GameObject coinObj = GameObject.Find("CoinScore");
            if (coinObj != null) scoreText = coinObj.GetComponent<TextMeshProUGUI>();
        }

        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }
}