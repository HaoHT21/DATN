using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("C?u hình Khung ch?a")]
    [SerializeField] private Transform itemContainer;   // Kéo ô "shop panel" (?ã g?n Grid Layout Group) vào ?ây
    [SerializeField] private GameObject itemSlotPrefab; // Kéo file Prefab m?u ItemSlot vào ?ây

    [Header("Kho hàng t?ng")]
    [SerializeField] private List<ItemData> allShopItems; // N?p t?t c? các file d? li?u súng ki?m vào ?ây

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateShopUI();
    }

    // T? ??ng r?i ??u các ô v? khí lên l??i UI
    public void GenerateShopUI()
    {
        // D?n s?ch các ô rác c? bên trong khung ch?a tr??c khi n?p
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // T?o vòng l?p duy?t qua kho tài nguyên hình ?nh súng ki?m c?a b?n
        foreach (ItemData item in allShopItems)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI slotScript = newSlot.GetComponent<ItemSlotUI>();

            if (slotScript != null)
            {
                slotScript.SetupSlot(item); // Kích ho?t hàm v? ch? và ?nh
            }
        }
    }

    // Hàm h? tr? tìm ki?m nhanh Prefab t??ng ?ng ?? Host th?c hi?n Spawn ra ??t
    public GameObject GetPrefabByID(int id)
    {
        ItemData found = allShopItems.Find(x => x.itemID == id);
        return found != null ? found.itemPrefab : null;
    }
}