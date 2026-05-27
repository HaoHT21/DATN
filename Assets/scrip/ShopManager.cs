using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private List<ItemData> allShopItems;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => GenerateShopUI();

    public void GenerateShopUI()
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        foreach (ItemData item in allShopItems)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, itemContainer);
            newSlot.GetComponent<ItemSlotUI>()?.SetupSlot(item);
        }
    }

    public GameObject GetPrefabByID(int id)
    {
        ItemData found = allShopItems.Find(x => x.itemID == id);
        return found != null ? found.itemPrefab : null;
    }
}