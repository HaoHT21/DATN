using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Biến này quyết định Player đã có đá hay chưa
    public bool hasTruthStone = false;

    // Hàm này để các script khác gọi khi Player nhặt được đá
    public void PickUpStone()
    {
        hasTruthStone = true;
        Debug.Log("Đã nhặt được Viên Đá Sự Thật!");
    }
}