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

    // LƯU DANH SÁCH DẠNG RAM (Mất khi tắt game, còn hiệu lực khi đang chơi)
    private static HashSet<int> sessionOwnedHeroes = new HashSet<int>();

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

    //Code thêm
    // Kiểm tra xem hero đã được sở hữu trong phiên chơi này chưa
    private bool CheckIsOwned(HeroData hero)
    {
        // 1. Mặc định hero đầu tiên hoặc ID 1, 21 luôn mở
        if (currentIndex == 0 || hero.itemID == 1 || hero.itemID == 21)
            return true;

        // 2. Kiểm tra xem đã mua trong phiên chơi này chưa
        return sessionOwnedHeroes.Contains(hero.itemID);
    }

    public void UpdateDisplay()
    {
        if (availableHeroes == null || availableHeroes.Count == 0)
            return;

        HeroData hero = availableHeroes[currentIndex];

        iconImage.sprite = hero.icon;

        //Thêm dòng này
        // Kiểm tra quyền sở hữu trong RAM
        bool isOwned = CheckIsOwned(hero);

        //Comment dòng này
        //bool isOwned = PlayerPrefs.GetInt("HiroOwned_" + hero.itemID, 0) == 1;

        //if (currentIndex == 0 || hero.itemID == 1 || hero.itemID == 21)
        //{
        //    isOwned = true;
        //}

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

        //Thêm dòng này
        // SỬA TẠI ĐÂY: Dùng CheckIsOwned(hero) thay vì đọc PlayerPrefs
        bool isOwned = CheckIsOwned(hero);

        //Comment dòng này
        //bool isOwned = PlayerPrefs.GetInt("HiroOwned_" + hero.itemID, 0) == 1;
        //if (currentIndex == 0 || hero.itemID == 1 || hero.itemID == 21) isOwned = true;

        if (isOwned)
        {
            Debug.Log($"[Shop] Đã chọn đổi sang nhân vật: {hero.heroName}");

            //Comment dòng này
            //PlayerPrefs.SetInt("SelectedHeroID", hero.itemID);
            //PlayerPrefs.Save();

            SwapPlayerOnMap(hero);
            UpdateDisplay();
        }
        else
        {
            if (PlayerStats.Instance != null)
            {
                // KIỂM TRA ĐỦ TIỀN MỚI CHO MUA
                bool isSuccess = PlayerStats.Instance.TryPurchase(hero.price, hero.itemID);

                if (isSuccess)
                {
                    //Thêm dòng này
                    // LƯU VÀO RAM TẠM THỜI (Không ghi vào ổ cứng/PlayerPrefs)
                    sessionOwnedHeroes.Add(hero.itemID);

                    //Comment dòng này
                    //PlayerPrefs.SetInt("HiroOwned_" + hero.itemID, 1);
                    //PlayerPrefs.SetInt("SelectedHeroID", hero.itemID);
                    //PlayerPrefs.Save();

                    SwapPlayerOnMap(hero);
                    UpdateDisplay();
                    Debug.Log($"<color=green>[Shop]</color> Mua thành công nhân vật: {hero.heroName}");
                }
                else
                {
                    Debug.LogWarning($"<color=red>[Shop]</color> Không đủ tiền mua nhân vật: {hero.heroName}!");
                }
            }
            else
            {
                Debug.LogError("Không tìm thấy PlayerStats.Instance");
            }
        }
    }

    // HÀM SWAP PLAYER VÀ GIỮ NGUYÊN SCALE SÚNG CHUẨN KHI MỚI LƯỢM
    private void SwapPlayerOnMap(HeroData hero)
    {
        if (hero.heroPrefab == null)
        {
            Debug.LogError($"[Shop] HeroData {hero.heroName} chưa gán Hero Prefab!");
            return;
        }

        // 1. TÌM PLAYER CŨ & LƯỢM SÚNG BẮT ĐẦU TÁCH
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        List<Transform> currentGuns = new List<Transform>();
        List<Vector3> originalGunScales = new List<Vector3>();

        if (oldPlayers.Length > 0)
        {
            spawnPos = oldPlayers[0].transform.position;
            spawnRot = oldPlayers[0].transform.rotation;

            foreach (GameObject p in oldPlayers)
            {
                p.tag = "Untagged";

                Transform oldHolder = FindDeepChild(p.transform, "WeaponHolder");
                if (oldHolder != null)
                {
                    for (int i = 0; i < oldHolder.childCount; i++)
                    {
                        Transform gun = oldHolder.GetChild(i);
                        currentGuns.Add(gun);
                        // LƯU LẠI SCALE CHUẨN CỦA SÚNG TRƯỚC KHIN TÁCH
                        originalGunScales.Add(gun.localScale);
                    }
                }

                foreach (Transform gun in currentGuns)
                {
                    gun.SetParent(null);
                }

                p.SetActive(false);
                Destroy(p);
            }
        }

        // 2. SINH PLAYER MỚI
        GameObject newPlayer = Instantiate(hero.heroPrefab, spawnPos, spawnRot);
        newPlayer.tag = "Player";

        // 3. CẬP NHẬT CINEMACHINE CAMERA FOLLOW CHUẨN UNITY 6
        Unity.Cinemachine.CinemachineCamera cmCam6 = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (cmCam6 != null)
        {
            cmCam6.Follow = newPlayer.transform;
            cmCam6.LookAt = newPlayer.transform;
        }

        // 4. GẮN SÚNG VÀO WEAPONHOLDER CỦA PLAYER MỚI VỚI SCALE GỐC
        Transform newHolder = FindDeepChild(newPlayer.transform, "WeaponHolder");

        if (newHolder != null && currentGuns.Count > 0)
        {
            for (int i = 0; i < currentGuns.Count; i++)
            {
                Transform gun = currentGuns[i];
                gun.SetParent(newHolder);
                gun.localPosition = Vector3.zero;
                gun.localRotation = Quaternion.identity;

                // TRẢ LẠI SCALE CHUẨN BAN ĐẦU CỦA SÚNG
                gun.localScale = originalGunScales[i];

                gun.gameObject.SetActive(true);

                SpriteRenderer sr = gun.GetComponent<SpriteRenderer>();
                if (sr == null) sr = gun.GetComponentInChildren<SpriteRenderer>();

                if (sr != null)
                {
                    sr.enabled = true;
                    sr.sortingOrder = 15;
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

    // ==========================================
    // HÀM RESET DỮ LIỆU SHOP VỀ BAN ĐẦU
    // ==========================================
    public static void ResetAllShopData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=green>[Shop Reset]</color> Đã xóa toàn bộ dữ liệu mua nhân vật!");

        HeroSlot[] slots = FindObjectsByType<HeroSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            slot.UpdateDisplay();
        }
    }
}