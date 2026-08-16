using UnityEngine;

public class FixedMapMinimap : MonoBehaviour
{
    [System.Serializable]
    public struct MapInfo
    {
        public string mapName;          // Tên hoặc tag của map
        public Transform mapCenter;     // Điểm trung tâm của map này
        public float cameraSize;        // Độ phóng to/thu nhỏ (Orthographic Size) để vừa khít map này
    }

    [SerializeField] private MapInfo[] listMaps; // Danh sách các map trong game
    private Camera minimapCam;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
    }

    void Update()
    {
        // Tự động tìm xem Player đang đứng ở map nào dựa vào vị trí hoặc bạn có thể gọi hàm chuyển map riêng
        DetectCurrentMap();
    }

    private void DetectCurrentMap()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null || listMaps == null || listMaps.Length == 0) return;

        // Tìm map gần player nhất (hoặc bạn có thể tối ưu bằng Trigger khi bước vào vùng map)
        MapInfo nearestMap = listMaps[0];
        float minDistance = Vector3.Distance(player.transform.position, nearestMap.mapCenter.position);

        foreach (var map in listMaps)
        {
            if (map.mapCenter == null) continue;
            float dist = Vector3.Distance(player.transform.position, map.mapCenter.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestMap = map;
            }
        }

        // Cố định camera ở tâm map đó và chỉnh góc nhìn vừa khít map
        if (nearestMap.mapCenter != null)
        {
            Vector3 targetPos = nearestMap.mapCenter.position;
            targetPos.z = -10f; // Giữ khoảng cách camera 2D
            transform.position = targetPos;

            if (minimapCam != null)
            {
                minimapCam.orthographicSize = nearestMap.cameraSize;
            }
        }
    }
}