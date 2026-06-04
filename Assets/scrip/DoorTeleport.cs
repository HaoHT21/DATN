using UnityEngine;

public class InternalTeleport : MonoBehaviour
{
    [Header("Cài đặt vị trí đích")]
    public Transform targetLocation; // Kéo object điểm đến vào đây

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu là Player chạm vào cửa
        if (collision.CompareTag("Player"))
        {
            // Thay đổi vị trí của Player sang vị trí của đích
            collision.transform.position = targetLocation.position;

            Debug.Log("Đã dịch chuyển đến: " + targetLocation.name);
        }
    }
}