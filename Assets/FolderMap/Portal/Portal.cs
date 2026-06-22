using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Điểm dịch chuyển")]
    public Transform targetLocation;

    private bool playerInRange;
    private Transform player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.transform;

            Debug.Log("Nhấn E để dịch chuyển");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;

            Debug.Log("Rời khỏi vùng dịch chuyển");
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    void TeleportPlayer()
    {
        if (player == null || targetLocation == null)
            return;

        player.position = targetLocation.position;

        Debug.Log("Đã dịch chuyển đến: " + targetLocation.name);
    }
}