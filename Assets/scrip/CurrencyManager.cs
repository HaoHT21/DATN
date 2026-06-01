using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public int totalGold;
    public TextMeshProUGUI goldDisplayText;

    private void Awake() { Instance = this; }

    private void Start()
    {
        totalGold = PlayerPrefs.GetInt("Gold", 1000);
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (totalGold >= amount)
        {
            totalGold -= amount;
            PlayerPrefs.SetInt("Gold", totalGold);
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    public void UpdateGoldUI()
    {
        if (goldDisplayText != null) goldDisplayText.text = "Gold: " + totalGold;
    }
}