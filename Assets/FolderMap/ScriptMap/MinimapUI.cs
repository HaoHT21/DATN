using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RoomManager;

public class MinimapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mapContainer;   // Khung chứa Minimap
    [SerializeField] private GameObject roomPrefab;        // UI Prefab căn phòng

    [Header("Room Colors")]
    [SerializeField] private Color defaultRoomColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color currentRoomColor = Color.white;  // Màu phòng Player đang đứng
    [SerializeField] private Color clearedRoomColor = Color.green;
    [SerializeField] private Color bossRoomColor = Color.red;
    [SerializeField] private Color specialRoomColor = Color.yellow;

    [Header("Icons (Optional)")]
    [SerializeField] private Sprite bossIcon;
    [SerializeField] private Sprite specialIcon;

    // Cache lại Image của từng Room để đổi màu nhanh
    private Dictionary<RoomData, Image> roomImages = new Dictionary<RoomData, Image>();

    private Vector2 worldCenter;
    private Vector2 worldSize;
    private float calculatedScale = 1f;
    private bool isMapGenerated = false;

    public void GenerateMinimap(List<RoomData> allRooms, List<RoomWave> roomSettings)
    {
        ClearMinimap();

        if (mapContainer == null || roomPrefab == null || allRooms == null || allRooms.Count == 0) return;

        // 1. TÍNH BOUNDING BOX VÀ TÂM BẢN ĐỒ
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var room in allRooms)
        {
            if (room.bounds.xMin < minX) minX = room.bounds.xMin;
            if (room.bounds.yMin < minY) minY = room.bounds.yMin;
            if (room.bounds.xMax > maxX) maxX = room.bounds.xMax;
            if (room.bounds.yMax > maxY) maxY = room.bounds.yMax;
        }

        worldSize = new Vector2(maxX - minX, maxY - minY);
        worldCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

        if (worldSize.x <= 0 || worldSize.y <= 0) return;

        // 2. TÍNH SCALE PHÙ HỢP KHUNG MAP
        Vector2 containerSize = mapContainer.rect.size;
        float scaleX = containerSize.x / worldSize.x;
        float scaleY = containerSize.y / worldSize.y;
        calculatedScale = Mathf.Min(scaleX, scaleY) * 0.85f; // Chừa lề 15%

        // 3. TẠO CÁC PHÒNG TRÊN MINIMAP
        for (int i = 0; i < allRooms.Count; i++)
        {
            RoomData room = allRooms[i];
            RoomWave setting = (roomSettings != null && i < roomSettings.Count) ? roomSettings[i] : null;

            GameObject roomObj = Instantiate(roomPrefab, mapContainer);
            RectTransform rect = roomObj.GetComponent<RectTransform>();
            Image roomImage = roomObj.GetComponent<Image>();

            // Ép Anchor/Pivot về Trung tâm (0.5, 0.5)
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // Set kích thước UI Room
            float roomWidthUI = room.bounds.width * calculatedScale;
            float roomHeightUI = room.bounds.height * calculatedScale;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, roomWidthUI);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, roomHeightUI);

            // Đặt vị trí căn phòng trên Minimap
            Vector2 roomWorldCenter = room.bounds.center;
            float posX = (roomWorldCenter.x - worldCenter.x) * calculatedScale;
            float posY = (roomWorldCenter.y - worldCenter.y) * calculatedScale;
            rect.anchoredPosition = new Vector2(posX, posY);

            // Xử lý Boss / Special Icon (nếu có)
            Transform iconTransform = roomObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    bool isBoss = room.isBossRoom || (setting != null && setting.isBossRoom);
                    bool isSpecial = room.hasSpecialObject;

                    if (isBoss && bossIcon != null)
                    {
                        iconImage.sprite = bossIcon;
                        iconImage.gameObject.SetActive(true);
                    }
                    else if (isSpecial && specialIcon != null)
                    {
                        iconImage.sprite = specialIcon;
                        iconImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        iconImage.gameObject.SetActive(false);
                    }
                }
            }

            // Lưu cache Image để đổi màu về sau
            if (!roomImages.ContainsKey(room))
            {
                roomImages.Add(room, roomImage);
            }
        }

        isMapGenerated = true;
        UpdateMinimap(allRooms, roomSettings);
    }

    public void UpdateMinimap(List<RoomData> allRooms, List<RoomWave> roomSettings)
    {
        if (!isMapGenerated || allRooms == null) return;

        for (int i = 0; i < allRooms.Count; i++)
        {
            RoomData room = allRooms[i];
            RoomWave setting = (roomSettings != null && i < roomSettings.Count) ? roomSettings[i] : null;

            if (!roomImages.ContainsKey(room)) continue;

            Image roomImage = roomImages[room];
            bool isBoss = room.isBossRoom || (setting != null && setting.isBossRoom);

            // ĐỔI MÀU TRẠNG THÁI PHÒNG
            if (room.isPlayerInside)
                roomImage.color = currentRoomColor;      // Phòng Player đang đứng
            else if (isBoss)
                roomImage.color = bossRoomColor;         // Phòng Boss
            else if (room.hasSpecialObject)
                roomImage.color = specialRoomColor;     // Phòng đặc biệt
            else if (room.isCleared)
                roomImage.color = clearedRoomColor;     // Phòng đã dọn xong
            else
                roomImage.color = defaultRoomColor;     // Phòng bình thường chưa vào
        }
    }

    public void ClearMinimap()
    {
        isMapGenerated = false;

        if (mapContainer != null)
        {
            foreach (Transform child in mapContainer)
            {
                Destroy(child.gameObject);
            }
        }

        roomImages.Clear();
    }
}