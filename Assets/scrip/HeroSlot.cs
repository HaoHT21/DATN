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

    // =========================================================
    // RESET DANH SÁCH HERO ĐÃ SỞ HỮU (DÙNG CHO GAME MỚI)
    // =========================================================
    public static void ResetOwnedHeroes()
    {
        sessionOwnedHeroes.Clear();
        sessionOwnedHeroes.Add(20); // Đặt Hero mặc định ban đầu là ID 20
        CurrentHeroID = 20;         // Đưa HeroID hiện tại về mặc định ID 20
    }

    // =========================================================
    // NHÂN VẬT HIỆN TẠI
    // =========================================================
    public static int CurrentHeroID = 20;

    // =========================================================
    // DANH SÁCH NHÂN VẬT ĐÃ MUA TRONG RAM
    // =========================================================
    private static HashSet<int> sessionOwnedHeroes =
        new HashSet<int>();

    // =========================================================
    // LẤY DANH SÁCH NHÂN VẬT ĐÃ MUA
    // =========================================================
    public static List<int> GetOwnedHeroIDs()
    {
        return new List<int>(sessionOwnedHeroes);
    }

    // =========================================================
    // LOAD DANH SÁCH NHÂN VẬT ĐÃ MUA
    // =========================================================
    public static void LoadOwnedHeroIDs(List<int> ids)
    {
        sessionOwnedHeroes.Clear();

        if (ids == null)
            return;

        foreach (int id in ids)
        {
            sessionOwnedHeroes.Add(id);
        }
    }

    // =========================================================
    // SET HERO HIỆN TẠI KHI LOAD SAVE
    // =========================================================
    public static void SetCurrentHeroID(int id)
    {
        if (id <= 0)
            id = 20;

        CurrentHeroID = id;

        Debug.Log(
            $"[HeroSlot] CurrentHeroID được load = {CurrentHeroID}"
        );
    }

    // =========================================================
    // LẤY HERO HIỆN TẠI
    // =========================================================
    public static int GetCurrentHeroID()
    {
        return CurrentHeroID;
    }

    // =========================================================
    // SETUP SHOP
    // =========================================================
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
            Debug.LogError(
                "HeroSlot chưa được gán đầy đủ UI trong Inspector!"
            );
            return;
        }

        btnBuyText =
            btnBuy.GetComponentInChildren<TextMeshProUGUI>();

        btnLeft.onClick.RemoveAllListeners();
        btnRight.onClick.RemoveAllListeners();
        btnBuy.onClick.RemoveAllListeners();

        btnLeft.onClick.AddListener(PrevHero);
        btnRight.onClick.AddListener(NextHero);
        btnBuy.onClick.AddListener(BuyCurrentHero);

        UpdateDisplay();
    }

    // =========================================================
    // KIỂM TRA HERO ĐÃ SỞ HỮU CHƯA
    // =========================================================
    private bool CheckIsOwned(HeroData hero)
    {
        if (hero == null)
            return false;

        // Hero đầu tiên trong danh sách Shop luôn mở
        if (currentIndex == 0)
            return true;

        // ID 20 là Hero mặc định ban đầu, luôn luôn mở
        if (hero.itemID == 20)
            return true;

        // Kiểm tra danh sách hero đã mua
        return sessionOwnedHeroes.Contains(hero.itemID);
    }

    // =========================================================
    // CẬP NHẬT HIỂN THỊ SHOP
    // =========================================================
    public void UpdateDisplay()
    {
        if (availableHeroes == null ||
            availableHeroes.Count == 0)
            return;

        HeroData hero =
            availableHeroes[currentIndex];

        if (hero == null)
            return;

        iconImage.sprite = hero.icon;

        // Kiểm tra quyền sở hữu
        bool isOwned =
            CheckIsOwned(hero);

        // =====================================================
        // HERO ĐÃ SỞ HỮU
        // =====================================================
        if (isOwned)
        {
            priceText.text = "Đã sở hữu";
            priceText.color = Color.green;

            if (btnBuyText != null)
            {
                btnBuyText.text = "Đổi nhân vật";
            }
        }
        // =====================================================
        // HERO CHƯA SỞ HỮU
        // =====================================================
        else
        {
            priceText.text =
                hero.price.ToString();

            priceText.color =
                Color.yellow;

            if (btnBuyText != null)
            {
                btnBuyText.text = "Mua";
            }
        }
    }

    // =========================================================
    // NEXT HERO
    // =========================================================
    public void NextHero()
    {
        if (availableHeroes == null ||
            availableHeroes.Count == 0)
            return;

        currentIndex++;

        if (currentIndex >= availableHeroes.Count)
            currentIndex = 0;

        UpdateDisplay();
    }

    // =========================================================
    // PREVIOUS HERO
    // =========================================================
    public void PrevHero()
    {
        if (availableHeroes == null ||
            availableHeroes.Count == 0)
            return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex =
                availableHeroes.Count - 1;

        UpdateDisplay();
    }

    // =========================================================
    // MUA / ĐỔI NHÂN VẬT
    // =========================================================
    public void BuyCurrentHero()
    {
        if (availableHeroes == null ||
            availableHeroes.Count == 0)
            return;

        HeroData hero =
            availableHeroes[currentIndex];

        if (hero == null)
            return;

        // Kiểm tra đã sở hữu chưa
        bool isOwned =
            CheckIsOwned(hero);

        // =====================================================
        // ĐÃ SỞ HỮU -> ĐỔI NHÂN VẬT
        // =====================================================
        if (isOwned)
        {
            Debug.Log(
                $"[Shop] Đã chọn đổi sang nhân vật: {hero.heroName}"
            );

            // Đổi Player trên map (hàm sẽ tự lưu hero cũ và nạp hero mới)
            SwapPlayerOnMap(hero);

            // LƯU ID HERO ĐANG SỬ DỤNG
            CurrentHeroID = hero.itemID;

            UpdateDisplay();
        }
        // =====================================================
        // CHƯA SỞ HỮU -> MUA
        // =====================================================
        else
        {
            if (PlayerStats.Instance != null)
            {
                // Kiểm tra đủ tiền
                bool isSuccess =
                    PlayerStats.Instance.TryPurchase(
                        hero.price,
                        hero.itemID
                    );

                if (isSuccess)
                {
                    // THÊM HERO VÀO DANH SÁCH ĐÃ MUA
                    sessionOwnedHeroes.Add(
                        hero.itemID
                    );

                    // Đổi Player trên map (hàm sẽ tự lưu hero cũ và nạp hero mới)
                    SwapPlayerOnMap(hero);

                    // LƯU HERO ĐANG SỬ DỤNG TRONG RAM
                    CurrentHeroID = hero.itemID;

                    UpdateDisplay();

                    Debug.Log(
                        $"<color=green>[Shop]</color> " +
                        $"Mua thành công nhân vật: " +
                        $"{hero.heroName}"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"<color=red>[Shop]</color> " +
                        $"Không đủ tiền mua nhân vật: " +
                        $"{hero.heroName}!"
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "Không tìm thấy PlayerStats.Instance"
                );
            }
        }
    }

    // =========================================================
    // SWAP PLAYER
    // =========================================================
    private void SwapPlayerOnMap(HeroData hero)
    {
        if (hero == null)
            return;

        if (hero.heroPrefab == null)
        {
            Debug.LogError(
                $"[Shop] HeroData {hero.heroName} " +
                $"chưa gán Hero Prefab!"
            );
            return;
        }

        // =====================================================
        // 0. LƯU TIẾN TRÌNH LEVEL & EXP CỦA HERO CŨ TRƯỚC KHI BỊ XÓA
        // =====================================================
        SaveHeroProgress(CurrentHeroID);

        // =====================================================
        // 1. TÌM PLAYER CŨ
        // =====================================================
        GameObject[] oldPlayers =
            GameObject.FindGameObjectsWithTag("Player");

        Vector3 spawnPos =
            Vector3.zero;

        Quaternion spawnRot =
            Quaternion.identity;

        // Danh sách súng đang cầm
        List<Transform> currentGuns =
            new List<Transform>();

        List<Vector3> originalGunScales =
            new List<Vector3>();

        // Lấy vị trí Player cũ
        if (oldPlayers.Length > 0)
        {
            spawnPos =
                oldPlayers[0].transform.position;

            spawnRot =
                oldPlayers[0].transform.rotation;

            foreach (GameObject p in oldPlayers)
            {
                p.tag = "Untagged";

                // Tìm WeaponHolder
                Transform oldHolder =
                    FindDeepChild(
                        p.transform,
                        "WeaponHolder"
                    );

                if (oldHolder != null)
                {
                    for (
                        int i = 0;
                        i < oldHolder.childCount;
                        i++
                    )
                    {
                        Transform gun =
                            oldHolder.GetChild(i);

                        currentGuns.Add(gun);

                        // Lưu scale gốc
                        originalGunScales.Add(
                            gun.localScale
                        );
                    }
                }

                // Tách súng ra khỏi Player cũ
                foreach (Transform gun in currentGuns)
                {
                    gun.SetParent(null);
                }

                p.SetActive(false);
                Destroy(p);
            }
        }

        // =====================================================
        // 2. SINH PLAYER MỚI
        // =====================================================
        GameObject newPlayer =
            Instantiate(
                hero.heroPrefab,
                spawnPos,
                spawnRot
            );

        newPlayer.tag = "Player";

        // =====================================================
        // 2.1 KHÔI PHỤC TIẾN TRÌNH CHO HERO MỚI ĐƯỢC CHỌN
        // =====================================================
        LoadHeroProgress(hero.itemID, newPlayer);

        // =====================================================
        // 3. CẬP NHẬT CINEMACHINE CAMERA
        // =====================================================
        Unity.Cinemachine.CinemachineCamera cmCam6 =
            FindFirstObjectByType<
                Unity.Cinemachine.CinemachineCamera
            >();

        if (cmCam6 != null)
        {
            cmCam6.Follow =
                newPlayer.transform;

            cmCam6.LookAt =
                newPlayer.transform;
        }

        // =====================================================
        // 4. TÌM WEAPON HOLDER PLAYER MỚI
        // =====================================================
        Transform newHolder =
            FindDeepChild(
                newPlayer.transform,
                "WeaponHolder"
            );

        // =====================================================
        // 5. GẮN LẠI TOÀN BỘ SÚNG
        // =====================================================
        if (newHolder != null &&
            currentGuns.Count > 0)
        {
            for (
                int i = 0;
                i < currentGuns.Count;
                i++
            )
            {
                Transform gun =
                    currentGuns[i];

                gun.SetParent(
                    newHolder
                );

                gun.localPosition =
                    Vector3.zero;

                gun.localRotation =
                    Quaternion.identity;

                // Trả scale ban đầu
                gun.localScale =
                    originalGunScales[i];

                gun.gameObject.SetActive(
                    true
                );

                SpriteRenderer sr =
                    gun.GetComponent<SpriteRenderer>();

                if (sr == null)
                {
                    sr =
                        gun.GetComponentInChildren<
                            SpriteRenderer
                        >();
                }

                if (sr != null)
                {
                    sr.enabled = true;
                    sr.sortingOrder = 15;
                }
            }
        }

        // Đồng bộ Weapon Visual
        PlayerController newController =
            newPlayer.GetComponent<PlayerController>();

        if (newController != null)
        {
            newController.UpdateWeaponVisuals();
        }
    }

    // =========================================================
    // HỆ THỐNG LƯU / TẢI LEVEL & CHỈ SỐ TỪ PLAYERHEALTH
    // =========================================================
    private void SaveHeroProgress(int heroID)
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            PlayerPrefs.SetInt("Hero_Level_" + heroID, health.currentLevel);
            PlayerPrefs.SetInt("Hero_EXP_" + heroID, health.currentEXP);
            PlayerPrefs.SetInt("Hero_MaxHP_" + heroID, health.maxHealth);
            PlayerPrefs.SetInt("Hero_MaxMP_" + heroID, health.maxMana);
            PlayerPrefs.Save();

            Debug.Log($"<color=cyan>[Shop Save Progress]</color> Hero ID: {heroID} | Level: {health.currentLevel}, EXP: {health.currentEXP}, MaxHP: {health.maxHealth}, MaxMP: {health.maxMana}");
        }
    }

    private void LoadHeroProgress(int heroID, GameObject newPlayer)
    {
        PlayerHealth health = newPlayer.GetComponent<PlayerHealth>();
        if (health == null) health = FindFirstObjectByType<PlayerHealth>();

        if (health != null)
        {
            int savedLevel = PlayerPrefs.GetInt("Hero_Level_" + heroID, 1);
            int savedEXP = PlayerPrefs.GetInt("Hero_EXP_" + heroID, 0);
            int savedMaxHP = PlayerPrefs.GetInt("Hero_MaxHP_" + heroID, 100 + (savedLevel - 1) * 20);
            int savedMaxMP = PlayerPrefs.GetInt("Hero_MaxMP_" + heroID, 100 + (savedLevel - 1) * 10);

            health.currentLevel = savedLevel;
            health.currentEXP = savedEXP;
            health.maxHealth = savedMaxHP;
            health.currentHealth = savedMaxHP;
            health.maxMana = savedMaxMP;
            health.currentMana = savedMaxMP;

            health.UpdateUI();

            Debug.Log($"<color=yellow>[Shop Load Progress]</color> Hero ID: {heroID} | Nạp Level: {savedLevel}, EXP: {savedEXP}, MaxHP: {savedMaxHP}, MaxMP: {savedMaxMP}");
        }
    }

    // =========================================================
    // TÌM CHILD OBJECT THEO TÊN
    // =========================================================
    private Transform FindDeepChild(
        Transform parent,
        string name
    )
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result =
                FindDeepChild(
                    child,
                    name
                );

            if (result != null)
                return result;
        }

        return null;
    }

    // =========================================================
    // RESET TOÀN BỘ SHOP DATA
    // =========================================================
    public static void ResetAllShopData()
    {
        // Xóa PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Xóa danh sách hero đã mua trong RAM
        sessionOwnedHeroes.Clear();

        // Reset hero hiện tại về mặc định ID 20
        CurrentHeroID = 20;

        Debug.Log(
            "<color=green>[Shop Reset]</color> " +
            "Đã xóa toàn bộ dữ liệu mua nhân vật!"
        );

        // Cập nhật tất cả HeroSlot
        HeroSlot[] slots =
            FindObjectsByType<HeroSlot>(
                FindObjectsSortMode.None
            );

        foreach (var slot in slots)
        {
            slot.UpdateDisplay();
        }
    }
}