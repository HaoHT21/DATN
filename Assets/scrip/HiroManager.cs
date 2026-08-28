using System.Collections.Generic;
using UnityEngine;

public class HiroManager : MonoBehaviour
{
    public static HiroManager Instance;

    public List<HeroData> allHeroes;

    public Transform slotContainer;
    public GameObject heroSlotPrefab;

    private void Awake()
    {
        // Giữ Singleton tồn tại xuyên suốt các Scene để SaveManager luôn truy cập được
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CreateShopSlots();
    }

    private void CreateShopSlots()
    {
        if (slotContainer == null)
        {
            Debug.LogError("slotContainer chưa gán");
            return;
        }

        if (heroSlotPrefab == null)
        {
            Debug.LogError("heroSlotPrefab chưa gán");
            return;
        }

        GameObject slot = Instantiate(heroSlotPrefab, slotContainer);

        HeroSlot heroSlot = slot.GetComponent<HeroSlot>();

        if (heroSlot == null)
        {
            Debug.LogError("Prefab không có script HeroSlot");
            return;
        }

        heroSlot.Setup(allHeroes);
    }

    public GameObject GetHeroPrefabByID(int id)
    {
        if (allHeroes == null || allHeroes.Count == 0)
        {
            Debug.LogError("[HIRO MANAGER] Danh sách allHeroes đang bị TRỐNG hoặc NULL!");
            return null;
        }

        foreach (HeroData hero in allHeroes)
        {
            // Bổ sung kiểm tra null cho phần tử hero trong danh sách
            if (hero == null) continue;

            if (hero.itemID == id)
            {
                if (hero.heroPrefab == null)
                {
                    Debug.LogError($"[HIRO MANAGER] Tìm thấy HeroData có ID {id} nhưng heroPrefab chưa gán trong Inspector!");
                    return null;
                }

                return hero.heroPrefab;
            }
        }

        Debug.LogWarning($"[HIRO MANAGER] Không tìm thấy HeroData nào có itemID = {id} trong danh sách allHeroes!");
        return null;
    }
}