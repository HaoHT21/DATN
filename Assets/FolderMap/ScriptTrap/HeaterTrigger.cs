using UnityEngine;

public class HeaterTrigger : MonoBehaviour
{
    [Header("Chỉ số sưởi")]
    [Tooltip("Tốc độ giảm thanh đóng băng mỗi giây (Ví dụ: 0.5f = giảm 50%/giây)")]
    public float heatSpeed = 0.5f;

    [Tooltip("Nếu để trống, script sẽ tự động tìm IceZone trong Scene khi Player bước vào")]
    public IceZone targetIceZone;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm IceZone nếu chưa có
            if (targetIceZone == null)
            {
                targetIceZone = GetComponentInParent<IceZone>();

                if (targetIceZone == null)
                {
                    // Dùng FindFirstObjectByType thay cho FindObjectOfType bị Obsolete
                    targetIceZone = Object.FindFirstObjectByType<IceZone>();
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetIceZone != null)
            {
                // Giảm giá trị băng liên tục mỗi frame
                targetIceZone.ReduceFreezeValue(heatSpeed * Time.deltaTime);
            }
            else
            {
                // Tìm lại nếu vẫn null (Dùng FindFirstObjectByType)
                targetIceZone = Object.FindFirstObjectByType<IceZone>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Reset lại tham chiếu khi Player đi ra khỏi vùng sưởi
            targetIceZone = null;
        }
    }
}