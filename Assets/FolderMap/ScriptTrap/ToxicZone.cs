using UnityEngine;

public class ToxicZone : MonoBehaviour
{
    [Header("Cấu hình Lan Rộng")]
    [Tooltip("Tốc độ lan rộng (đơn vị/giây)")]
    public float expandSpeed = 1.5f;

    [Tooltip("Tỷ lệ % tối đa so với kích thước phòng (ví dụ 0.85 = 85%)")]
    [Range(0.1f, 1f)] public float maxRoomCoverage = 0.85f;

    private Vector2 maxTargetScale;
    private bool isExpanding = false;

    /// <summary>
    /// Kích hoạt vùng độc và truyền kích thước phòng vào
    /// </summary>
    public void ActivateZone(float roomWidth, float roomHeight)
    {
        float targetX = roomWidth * maxRoomCoverage;
        float targetY = roomHeight * maxRoomCoverage;

        maxTargetScale = new Vector2(targetX, targetY);
        transform.localScale = Vector3.zero; // Bắt đầu từ 0
        isExpanding = true;
    }

    void Update()
    {
        if (!isExpanding) return;

        Vector3 currentScale = transform.localScale;

        // Phóng to dần cho đến khi chạm mốc tối đa
        if (currentScale.x < maxTargetScale.x || currentScale.y < maxTargetScale.y)
        {
            float nextX = Mathf.MoveTowards(currentScale.x, maxTargetScale.x, expandSpeed * Time.deltaTime);
            float nextY = Mathf.MoveTowards(currentScale.y, maxTargetScale.y, expandSpeed * Time.deltaTime);

            transform.localScale = new Vector3(nextX, nextY, 1f);
        }
        else
        {
            isExpanding = false; // Đã đạt kích thước tối đa
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player dẫm vào Vùng Độc!");
            // Xử lý trừ máu / hiệu ứng độc tại đây
        }
    }
}