    using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;
    [SerializeField] private Button btnBuy;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;

    private List<HeroData> availableHeroes;
    private int currentIndex;

    public void Setup(List<HeroData> heroes)
    {
        Debug.Log("HeroSlot Setup");

        availableHeroes = heroes;
        currentIndex = 0;

        if (btnLeft == null ||
            btnRight == null ||
            btnBuy == null ||
            iconImage == null ||
            priceText == null)
        {
            Debug.LogError("HeroSlot chưa được gán đầy đủ UI trong Inspector!");
            return;
        }

        btnLeft.onClick.RemoveAllListeners();
        btnRight.onClick.RemoveAllListeners();
        btnBuy.onClick.RemoveAllListeners();

        btnLeft.onClick.AddListener(PrevHero);
        btnRight.onClick.AddListener(NextHero);
        btnBuy.onClick.AddListener(BuyCurrentHero);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (availableHeroes == null || availableHeroes.Count == 0)
            return;

        HeroData hero = availableHeroes[currentIndex];

        iconImage.sprite = hero.icon;
        priceText.text = hero.price.ToString();
    }

    public void NextHero()
    {
        Debug.Log("Next");

        if (availableHeroes == null || availableHeroes.Count == 0)
            return;

        currentIndex++;

        if (currentIndex >= availableHeroes.Count)
            currentIndex = 0;

        UpdateDisplay();
    }

    public void PrevHero()
    {
        Debug.Log("Prev");

        if (availableHeroes == null || availableHeroes.Count == 0)
            return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = availableHeroes.Count - 1;

        UpdateDisplay();
    }

    public void BuyCurrentHero()
    {
        Debug.Log("Buy");

        if (availableHeroes == null || availableHeroes.Count == 0)
            return;

        HeroData hero = availableHeroes[currentIndex];

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.TryPurchase(hero.price, hero.itemID);
        }
        else
        {
            Debug.LogError("Không tìm thấy PlayerStats.Instance");
        }
    }
}