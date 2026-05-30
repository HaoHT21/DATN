using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int Score = 1000;
    public TextMeshProUGUI scoreText;

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

    // Đảm bảo hàm này nhận đúng 2 tham số: price và itemID
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
        GameObject prefab = ShopManager.Instance.GetPrefabByID(id);
        if (prefab != null)
        {
            Instantiate(prefab, transform.position + new Vector3(0, -0.8f, 0), Quaternion.identity);
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