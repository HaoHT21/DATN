using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HiroManager : MonoBehaviour
{
    public static HiroManager Instance;

    public List<HeroData> allHeroes;

    public Transform slotContainer;
    public GameObject heroSlotPrefab;

    private void Awake()
    {
        Instance = this;
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
        foreach (HeroData hero in allHeroes)
        {
            if (hero.itemID == id)
                return hero.heroPrefab;
        }

        return null;
    }
}

// =========================================================================
// NÚT BẤM RESET DỮ LIỆU SHOP TRÊN THANH MENU UNITY (CHỈ DÙNG TRONG EDITOR)
// =========================================================================
#if UNITY_EDITOR
public class ShopTestEditor
{
    [MenuItem("Tools/Reset Shop Data (Xoa PlayerPrefs)")]
    public static void ResetShopData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[TEST SHOP]</color> Đã xóa sạch dữ liệu mua Hero! Giờ chạy game sẽ trở về trạng thái chưa mua.");
    }
}
#endif