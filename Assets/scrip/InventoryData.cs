using UnityEngine;

using System.Collections.Generic;



public class InventoryData : MonoBehaviour

{

    public static InventoryData Instance;



    // Lưu danh sách vũ khí dùng chung cho mọi nhân vật

    public List<PlayerController.WeaponItem> sharedInventory = new List<PlayerController.WeaponItem>();

    public int currentWeaponIndex = 0;



    private void Awake()

    {

        if (Instance == null)

        {

            Instance = this;

            DontDestroyOnLoad(gameObject);

        }

        else

        {

            Destroy(gameObject);

        }

    }

}

