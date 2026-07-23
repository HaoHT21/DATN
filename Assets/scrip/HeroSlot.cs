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

    private TextMeshProUGUI btnBuyText;

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

        btnBuyText = btnBuy.GetComponentInChildren<TextMeshProUGUI>();

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

        bool isOwned = PlayerPrefs.GetInt("HiroOwned_" + hero.itemID, 0) == 1;

        if (currentIndex == 0 || hero.itemID == 1 || hero.itemID == 21)
        {
            isOwned = true;
        }

        if (isOwned)
        {
            priceText.text = "Đã sở hữu";
            priceText.color = Color.green;

            if (btnBuyText != null)
            {
                btnBuyText.text = "Đổi nhân vật";
            }
        }
        else
        {
            priceText.text = hero.price.ToString();
            priceText.color = Color.yellow;

            if (btnBuyText != null)
            {
                btnBuyText.text = "Mua";
            }
        }
    }

    public void NextHero()
    {
        if (availableHeroes == null || availableHeroes.Count == 0) return;
        currentIndex++;
        if (currentIndex >= availableHeroes.Count) currentIndex = 0;
        UpdateDisplay();
    }

    public void PrevHero()
    {
        if (availableHeroes == null || availableHeroes.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = availableHeroes.Count - 1;
        UpdateDisplay();
    }

    public void BuyCurrentHero()
    {
        if (availableHeroes == null || availableHeroes.Count == 0) return;

        HeroData hero = availableHeroes[currentIndex];

        bool isOwned = PlayerPrefs.GetInt("HiroOwned_" + hero.itemID, 0) == 1;
        if (currentIndex == 0 || hero.itemID == 1 || hero.itemID == 21) isOwned = true;

        if (isOwned)
        {
            Debug.Log($"[Shop] Đã chọn đổi sang nhân vật: {hero.heroName}");

            PlayerPrefs.SetInt("SelectedHeroID", hero.itemID);
            PlayerPrefs.Save();

            SwapPlayerOnMap(hero);
            UpdateDisplay();
        }
        else
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.TryPurchase(hero.price, hero.itemID);

                PlayerPrefs.SetInt("HiroOwned_" + hero.itemID, 1);
                PlayerPrefs.SetInt("SelectedHeroID", hero.itemID);
                PlayerPrefs.Save();

                SwapPlayerOnMap(hero);
                UpdateDisplay();
            }
            else
            {
                Debug.LogError("Không tìm thấy PlayerStats.Instance");
            }
        }
    }

    // HÀM SWAP PLAYER VÀ GIỮ NGUYÊN SÚNG TRÊN TAY CỰC MƯỢT
    private void SwapPlayerOnMap(HeroData hero)
    {
        if (hero.heroPrefab == null)
        {
            Debug.LogError($"[Shop] HeroData {hero.heroName} chưa gán Hero Prefab!");
            return;
        }

        // 1. TÌM PLAYER CŨ & TÁCH SÚNG RA NGOÀI ĐỂ KHÔNG BỊ DESTROY
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        List<Transform> currentGuns = new List<Transform>();

        if (oldPlayers.Length > 0)
        {
            spawnPos = oldPlayers[0].transform.position;
            spawnRot = oldPlayers[0].transform.rotation;

            foreach (GameObject p in oldPlayers)
            {
                p.tag = "Untagged"; // Đổi tag để tránh trùng

                Transform oldHolder = FindDeepChild(p.transform, "WeaponHolder");
                if (oldHolder != null)
                {
                    for (int i = 0; i < oldHolder.childCount; i++)
                    {
                        Transform gun = oldHolder.GetChild(i);
                        currentGuns.Add(gun);
                    }
                }

                // CỰC KỲ QUAN TRỌNG: Tách tất cả các con súng ra khỏi Player cũ ngay lập tức
                foreach (Transform gun in currentGuns)
                {
                    gun.SetParent(null); // Đưa súng ra ngoài Hierarchy tạm thời
                }

                p.SetActive(false); // Ẩn Player cũ
                Destroy(p);         // Xóa Player cũ
            }
        }

        // 2. SINH RA 1 CON PLAYER MỚI
        GameObject newPlayer = Instantiate(hero.heroPrefab, spawnPos, spawnRot);
        newPlayer.tag = "Player"; // Gán lại Tag chuẩn

        // 3. CẬP NHẬT CAMERA FOLLOW
        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            cam.target = newPlayer.transform;
        }

        // 4. GẮN LẠI CÁC CÂY SÚNG ĐÓ VÀO WEAPONHOLDER CỦA PLAYER MỚI
        Transform newHolder = FindDeepChild(newPlayer.transform, "WeaponHolder");

        if (newHolder != null && currentGuns.Count > 0)
        {
            foreach (Transform gun in currentGuns)
            {
                gun.SetParent(newHolder); // Gắn lại làm con của WeaponHolder mới
                gun.localPosition = Vector3.zero; // Căn về chính giữa tay
                gun.localRotation = Quaternion.identity;
                gun.localScale = Vector3.one;
                gun.gameObject.SetActive(true);

                // Ép Sprite Súng hiện rõ ràng trên tay
                SpriteRenderer sr = gun.GetComponent<SpriteRenderer>();
                if (sr == null) sr = gun.GetComponentInChildren<SpriteRenderer>();

                if (sr != null)
                {
                    sr.enabled = true;
                    sr.sortingOrder = 15; // Nổi đè lên trên Sprite nhân vật
                }
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}