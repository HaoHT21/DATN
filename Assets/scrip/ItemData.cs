using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Shop/Item Data")]
public class ItemData : ScriptableObject
{
    public int itemID;           // ID phân biệt (0, 1, 2, 3...)
    public string itemName;      // Tên vũ khí (Ví dụ: Súng Laser X)
    public int price;            // Giá tiền mua
    public Sprite itemIcon;      // Hình ảnh Pixel-Art lấy từ file ảnh của bạn
    public GameObject itemPrefab;// Prefab mạng thực tế (để rơi ra đất khi mua)
}