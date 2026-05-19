using UnityEngine;

public class DoorTeleport : MonoBehaviour
{
    [Header("Điểm dịch chuyển")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra object có tag Player hay không
        if (other.CompareTag("Player"))
        {
            // Dịch chuyển người chơi tới spawnPoint
            other.transform.position = spawnPoint.position;
        }
    }
}
