using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;

    public void SetItem(ItemData item)
    {
        if (item == null)
        {
            icon.enabled = false;
            return;
        }

        icon.enabled = true;
        icon.sprite = item.itemIcon;
    }
}