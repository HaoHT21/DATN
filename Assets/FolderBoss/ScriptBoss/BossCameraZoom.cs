using UnityEngine;
using Unity.Cinemachine; // Namespace mới của Cinemachine 3.0+

public class BossCameraZoom : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [Tooltip("Virtual Camera đang bám theo Player")]
    public CinemachineCamera playerVirtualCamera; // Đã đổi sang CinemachineCamera

    [Header("Zoom Settings")]
    [Tooltip("Độ rộng Camera bình thường khi đi đường")]
    public float normalSize = 5f;

    [Tooltip("Độ rộng Camera khi đấu Boss (Càng to nhìn càng rộng)")]
    public float bossSize = 9f;

    [Tooltip("Tốc độ phóng to/thu nhỏ")]
    public float zoomSpeed = 2f;

    private float targetSize;

    private void Start()
    {
        if (playerVirtualCamera != null)
        {
            // Cinemachine 3.x dùng Lens.OrthographicSize (không có m_)
            playerVirtualCamera.Lens.OrthographicSize = normalSize;
            targetSize = normalSize;
        }
    }

    private void Update()
    {
        if (playerVirtualCamera == null) return;

        // Biến đổi Orthographic Size từ từ về giá trị targetSize để camera zoom mượt mà
        float currentSize = playerVirtualCamera.Lens.OrthographicSize;
        if (!Mathf.Approximately(currentSize, targetSize))
        {
            playerVirtualCamera.Lens.OrthographicSize = Mathf.Lerp(
                currentSize,
                targetSize,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    /// <summary>
    /// Phóng to tầm nhìn Camera khi vào phòng Boss
    /// </summary>
    public void ZoomOutForBoss()
    {
        Debug.Log(">>> CAMERA: ZOOM OUT FOR BOSS");
        targetSize = bossSize;
    }

    public void ResetZoom()
    {
        Debug.Log("<<< CAMERA: RESET ZOOM");
        targetSize = normalSize;
    }
}