using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Transform content;

    public GameObject slotPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshUI()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in InventoryManager.Instance.items)
        {
            GameObject slot =
                Instantiate(slotPrefab, content);

            slot.GetComponent<InventorySlotUI>()
                .SetItem(item);
        }
    }
}