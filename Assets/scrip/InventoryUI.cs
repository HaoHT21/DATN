using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image[] iconDisplays;
    private PlayerController player;

    void Update()
    {
        // Tự gán player khi player xuất hiện trong scene
        if (player == null) player = FindObjectOfType<PlayerController>();

        var inv = InventoryData.Instance.sharedInventory;
        for (int i = 0; i < iconDisplays.Length; i++)
        {
            if (i < inv.Count)
            {
                iconDisplays[i].sprite = inv[i].icon;
                iconDisplays[i].enabled = true;
                iconDisplays[i].transform.parent.GetComponent<Image>().color =
                    (i == InventoryData.Instance.currentWeaponIndex) ? Color.yellow : Color.white;
            }
            else iconDisplays[i].enabled = false;
        }
    }
}