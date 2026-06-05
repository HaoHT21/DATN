using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int Score = 1000;
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
        // 1. Lấy prefab từ Manager tương ứng
        GameObject prefab = (id >= 20) ? HiroManager.Instance?.GetHeroPrefabByID(id) : ShopManager.Instance?.GetPrefabByID(id);

        if (prefab != null)
        {
            // 2. Tìm vị trí của Player hiện tại trong Scene
            GameObject currentObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = (currentObj != null) ? currentObj.transform.position : Vector3.zero;

            // 3. Xử lý logic
            if (id >= 20)
            {
                // Thay thế nhân vật
                if (currentObj != null) Destroy(currentObj);

                GameObject newPlayer = Instantiate(prefab, spawnPos, Quaternion.identity);
                newPlayer.tag = "Player"; // Đảm bảo gán lại Tag để các lần mua sau tìm được
                Debug.Log($"[Shop] Đã thay thế nhân vật ID: {id}");
            }
            else
            {
                // Rớt vật phẩm dưới chân Player (Offset Y = -0.8f)
                Instantiate(prefab, spawnPos + new Vector3(0, -0.8f, 0), Quaternion.identity);
                Debug.Log($"[Shop] Đã spawn vật phẩm ID: {id} tại vị trí của Player");
            }
        }
        else
        {
            Debug.LogError($"[Shop] Không tìm thấy Prefab cho ID: {id}. Kiểm tra lại ShopManager/HiroManager!");
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