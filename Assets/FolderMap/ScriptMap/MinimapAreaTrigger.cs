using UnityEngine;

public class MinimapAreaTrigger : MonoBehaviour
{
    [SerializeField] private MinimapUI minimapUI;
    [SerializeField] private RoomManager roomManager;

    private bool isPlayerInsideArea = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideArea = true;

            if (minimapUI != null && roomManager != null)
            {
                var allRooms = roomManager.AllRooms;
                var roomSettings = roomManager.roomSettings;

                if (allRooms != null && allRooms.Count > 0)
                {
                    // Đã bỏ tham số other.transform để khớp với MinimapUI mới
                    minimapUI.GenerateMinimap(allRooms, roomSettings);
                }
            }
        }
    }

    private void Update()
    {
        if (isPlayerInsideArea && minimapUI != null && roomManager != null)
        {
            var allRooms = roomManager.AllRooms;
            if (allRooms != null)
            {
                // Cập nhật màu sắc các phòng
                minimapUI.UpdateMinimap(allRooms, roomManager.roomSettings);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideArea = false;

            if (minimapUI != null)
            {
                minimapUI.ClearMinimap();
            }
        }
    }
}