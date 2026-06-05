using UnityEngine;



[CreateAssetMenu(fileName = "New Item", menuName = "Shop/Item Data")]

public class ItemData : ScriptableObject

{

    public int itemID;

    public string itemName;

    public int price;

    public Sprite itemIcon;

    public GameObject itemPrefab; // Prefab dùng khi vứt ra đất

    public GameObject visualPrefab; // Prefab hiển thị trên tay

    public GameObject bulletPrefab;

    public bool isGun;

    public int damage;

}

